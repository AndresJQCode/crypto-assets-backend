using System.Text.Json;
using Api.Application.Dtos.Auth;
using Api.Application.Services.Auth;
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

namespace Api.Application.Commands.AuthCommands;

internal sealed class LoginCommandHandler(
    UserManager<User> userManager,
    IOptionsMonitor<AppSettings> appSettings,
    IJwtTokenService jwtTokenService,
    IUserInfoService userInfoService,
    IRecaptchaService recaptchaService,
    IOutOfTransactionAuditLogWriter auditLogWriter) : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private AppSettings.RecaptchaSettings Recaptcha => appSettings.CurrentValue.Recaptcha;

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validar reCAPTCHA si está habilitado
        if (Recaptcha.Enabled && Recaptcha.RequiresValidation("/auth/login"))
        {
            var recaptchaResult = await recaptchaService.ValidateTokenAsync(recaptchaToken: request.RecaptchaToken ?? string.Empty, recaptchaAction: "LOGIN");

            if (!recaptchaResult.Success)
            {
                throw new BadRequestException($"Validación de reCAPTCHA fallida: {recaptchaResult.ErrorMessage}");
            }
        }

        User? user = await userManager.FindByEmailAsync(request.Email);

        // si el usuario no existe, se lanza una excepción y guarda log de auditoría (en scope independiente para que no se revierta)
        if (user is null)
        {
            var auditLog = new AuditLog(
                entityType: AppConstants.AuditEntityTypes.Authentication,
                entityId: Guid.Empty,
                action: AppConstants.AuditActions.LoginFailed,
                userId: null,
                userName: request.Email,
                reason: "Intento de login fallido: user_not_found",
                additionalData: JsonSerializer.Serialize(new { Email = request.Email })
            );
            await auditLogWriter.SaveAsync(auditLog, cancellationToken);
            throw new BadRequestException("Credenciales inválidas");
        }

        bool loginResult = await userManager.CheckPasswordAsync(user, request.Password);

        if (!loginResult || !user.IsActive)
        {
            string authResult = !loginResult ? "invalid_password" : "user_inactive";

            // Registrar intento de login fallido
            InfrastructureMetrics.AuthenticationAttemptsTotal
                .WithLabels(MetricsLabelsConstants.Authentication.Login, authResult)
                .Inc();

            // Registrar en auditoría
            var additionalData = JsonSerializer.Serialize(new
            {
                Email = request.Email,
                Reason = authResult
            });

            var auditLog = new AuditLog(
                entityType: AppConstants.AuditEntityTypes.Authentication,
                entityId: user.Id,
                action: AppConstants.AuditActions.LoginFailed,
                userId: user.Id,
                userName: user.Email ?? request.Email,
                reason: $"Intento de login fallido: {authResult}",
                additionalData: additionalData
            );

            await auditLogWriter.SaveAsync(auditLog, cancellationToken);
            throw new BadRequestException("Credenciales inválidas");
        }

        // Generar access token y refresh token usando el servicio JWT
        string? accessToken = jwtTokenService.GenerateToken(user, AppConstants.Authentication.DefaultProvider);
        string? refreshToken = jwtTokenService.GenerateRefreshToken(user);

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
