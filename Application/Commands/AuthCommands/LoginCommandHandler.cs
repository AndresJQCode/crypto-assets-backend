using System.Text.Json;
using Application.Dtos.Auth;
using Application.Services.Auth;
using Api.Extensions;
using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Constants;
using Infrastructure.Metrics;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Commands.AuthCommands;

internal sealed class LoginCommandHandler(
    UserManager<User> userManager,
    IOptionsMonitor<AppSettings> appSettings,
    IJwtTokenService jwtTokenService,
    IUserInfoService userInfoService,
    IRecaptchaService recaptchaService,
    IHttpContextAccessor httpContextAccessor,
    ApiContext context) : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private AppSettings.RecaptchaSettings Recaptcha => appSettings.CurrentValue.Recaptcha;

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validar reCAPTCHA si está habilitado
        if (Recaptcha.Enabled && Recaptcha.RequiresValidation("/auth/login"))
        {
            var remoteIp = httpContextAccessor.HttpContext?.GetClientIpAddress();
            var recaptchaResult = await recaptchaService.ValidateTokenAsync(request.RecaptchaToken ?? string.Empty, remoteIp);

            if (!recaptchaResult.Success)
            {
                throw new BadRequestException($"Validación de reCAPTCHA fallida: {recaptchaResult.ErrorMessage}");
            }
        }

        string authResult = "success";

        User? user = await userManager.FindByEmailAsync(request.Email);

        // Por seguridad, siempre validar contraseña aunque el usuario no exista
        // Esto evita revelar qué emails están registrados en el sistema
        bool loginResult = user != null && await userManager.CheckPasswordAsync(user, request.Password);

        if (!loginResult || user == null || !user.IsActive)
        {
            authResult = user == null ? "user_not_found" : (!loginResult ? "invalid_password" : "user_inactive");

            // Registrar intento de login fallido
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.Authentication.Login, authResult)
                .Inc();

            // Registrar en auditoría
            var remoteIp = httpContextAccessor.HttpContext?.GetClientIpAddress();
            var additionalData = JsonSerializer.Serialize(new
            {
                Email = request.Email,
                RemoteIpAddress = remoteIp,
                Reason = authResult
            });

            var auditLog = new AuditLog(
                entityType: AppConstants.AuditEntityTypes.Authentication,
                entityId: user?.Id ?? Guid.Empty,
                action: AppConstants.AuditActions.LoginFailed,
                userId: user?.Id,
                userName: user?.Email ?? request.Email,
                reason: $"Intento de login fallido: {authResult}",
                additionalData: additionalData
            );

            context.AuditLogs.Add(auditLog);
            await context.SaveEntitiesAsync(cancellationToken);

            throw new BadRequestException("Credenciales inválidas");
        }

        // Generar access token y refresh token usando el servicio JWT
        var accessToken = jwtTokenService.GenerateToken(user, AppConstants.Authentication.DefaultProvider);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user);

        // Guardar tokens en el usuario
        IdentityResult resultSetAccessToken = await userManager.SetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.AccessTokenName, accessToken);
        IdentityResult resultSetRefreshToken = await userManager.SetAuthenticationTokenAsync(user, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.RefreshTokenName, refreshToken);

        if (!resultSetAccessToken.Succeeded || !resultSetRefreshToken.Succeeded)
        {
            throw new DomainException("Error setting tokens");
        }

        // Obtener información completa del usuario (roles y permisos)
        AuthUserDto? userInfo = await userInfoService.GetUserInfoAsync(user);

        // Registrar login exitoso
        InfrastructureMetrics.AuthenticationAttemptsTotal
            .WithLabels(MetricsLabelsConstants.Authentication.Login, MetricsLabelsConstants.Authentication.Success)
            .Inc();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userInfo
        };
    }
}
