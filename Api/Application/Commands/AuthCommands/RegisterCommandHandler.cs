using System.Text.Json;
using Api.Application.Dtos.Auth;
using Api.Application.Services.Auth;
using Api.Extensions;
using Api.Utilities;
using Domain.AggregatesModel.AuditAggregate;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.TenantAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Application.Commands.AuthCommands;

internal sealed class RegisterCommandHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IJwtTokenService jwtTokenService,
    IUserInfoService userInfoService,
    IRecaptchaService recaptchaService,
    IOptionsMonitor<AppSettings> appSettings,
    ITenantRepository tenantRepository) : IRequestHandler<RegisterCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (appSettings.CurrentValue.Recaptcha.Enabled && appSettings.CurrentValue.Recaptcha.RequiresValidation("/auth/register"))
        {
            var recaptchaResult = await recaptchaService.ValidateTokenAsync(recaptchaToken: request.RecaptchaToken ?? string.Empty, recaptchaAction: "REGISTER");
            if (!recaptchaResult.Success)
                throw new BadRequestException($"Validación de reCAPTCHA fallida: {recaptchaResult.ErrorMessage}");
        }

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new BadRequestException("Ya existe un usuario con ese email");

        var slug = TenantSlugHelper.GenerateFromName(request.TenantName);
        Tenant? existingTenant = await tenantRepository.GetFirstOrDefaultByFilter(t => t.Slug == slug, cancellationToken: cancellationToken);
        if (existingTenant is not null)
        {
            slug = TenantSlugHelper.AppendRandomSuffix(slug);
            existingTenant = await tenantRepository.GetFirstOrDefaultByFilter(t => t.Slug == slug, cancellationToken: cancellationToken);
            if (existingTenant != null)
            {
                throw new BadRequestException("Ya existe una empresa con ese nombre. Elija otro.");
            }
        }

        // Create tenant using factory method that raises TenantRegisteredEvent
        var tenant = Tenant.CreateForRegistration(
            tenantName: request.TenantName,
            tenantSlug: slug,
            adminEmail: request.Email,
            adminName: request.Name,
            createdBy: null);

        await tenantRepository.Create(tenant, cancellationToken);
        await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken); // ← Domain events dispatched here

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Name = request.Name,
            WhatsAppNumber = request.WhatsAppNumber.Trim(),
            TenantId = tenant.Id,
        };
        user.Activate();

        // Crear el usuario con contraseña
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Error al crear el usuario: {errors}");
        }

        // Asignar el rol Admin al usuario
        var adminRole = await roleManager.FindByNameAsync(RolesEnum.Admin.ToString());
        if (adminRole == null)
        {
            throw new BadRequestException("El rol Admin no existe en el sistema");
        }

        var roleResult = await userManager.AddToRoleAsync(user, adminRole.Name!);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new BadRequestException($"Error al asignar el rol Admin: {errors}");
        }

        tenant.CreatedBy = user.Id;
        tenant.LastModifiedBy = user.Id;
        await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

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

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userInfo
        };
    }
}
