using System.Text.Json;
using Api.Application.Dtos.Auth;
using Api.Application.Services.Auth;
using Api.Utilities;
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

internal sealed class ExchangeCodeCommandHandler(IAuthProviderFactory authProviderFactory, IOptionsMonitor<OAuthSettings> oAuthSettings, UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<ExchangeCodeCommandHandler> logger, ITenantRepository tenantRepository, IUserInfoService userInfoService,
    IJwtTokenService jwtTokenService) : IRequestHandler<ExchangeCodeCommand, ExchangeCodeResponseDto>
{
    private static readonly JsonSerializerOptions TenantRequestJsonOptions = new() { PropertyNameCaseInsensitive = true };


    public async Task<ExchangeCodeResponseDto> Handle(ExchangeCodeCommand request, CancellationToken cancellationToken)
    {
        // Decodificar state (URL-encoded JSON) a TenantRequest si viene presente
        TenantRequest? tenantRequest = null;
        if (!string.IsNullOrEmpty(request.State))
        {
            string decoded = Uri.UnescapeDataString(request.State);
            try
            {
                tenantRequest = JsonSerializer.Deserialize<TenantRequest>(decoded, TenantRequestJsonOptions);
            }
            catch (JsonException ex)
            {
                throw new BadRequestException($"El state recibido no es un JSON válido: {ex.Message}");
            }
        }

        // Validar que el proveedor sea compatible
        if (!authProviderFactory.IsProviderSupported(request.Provider))
        {
            throw new ArgumentException($"El proveedor '{request.Provider}' no es compatible. Proveedores disponibles: Google, Microsoft");
        }

        // identificar el flujo de autenticación mediante el state
        if (tenantRequest?.Flow == "register" && !oAuthSettings.CurrentValue.AllowPublicUsers)
        {
            throw new UnAuthorizedException("El sistema no permite registro público de usuarios");
        }

        // Obtener el servicio del proveedor específico
        IAuthProviderService? providerService = authProviderFactory.GetProvider(request.Provider);

        // Intercambiar el código por un token y obtener información del usuario
        string? accessTokenProvider = await providerService.ExchangeCodeAsync(request.Code);
        ExternalUserInfo? externalUserInfo = await providerService.GetExternalUserInfoAsync(accessTokenProvider);

        // buscar usuario existente o crear nuevo
        User? user = await userManager.FindByEmailAsync(externalUserInfo.Email);
        if (user is not null && tenantRequest?.Flow == "register")
        {
            throw new BadRequestException("Ya existe un usuario con ese email");
        }

        if (user is null)
        {
            if (tenantRequest?.Flow == "login")
            {
                throw new BadRequestException("El usuario no existe en el sistema, debe registrarse primero");
            }
            else if (tenantRequest?.Flow == "register")
            {
                user = await CreateUserAsync(providerService.MapExternalUserInfoToUser(externalUserInfo), roleManager, userManager, logger);

                string tenantName = tenantRequest?.TenantName?.Trim() ?? externalUserInfo.Email;
                string tenantSlug = TenantSlugHelper.GenerateFromName(tenantName);
                Tenant? existingBySlug = await tenantRepository.GetFirstOrDefaultByFilter(t => t.Slug == tenantSlug, cancellationToken: cancellationToken);
                if (existingBySlug is not null)
                {
                    tenantSlug = TenantSlugHelper.AppendRandomSuffix(tenantSlug);
                }
                Tenant tenant = Tenant.CreateForRegistration(
                    tenantName: tenantName,
                    tenantSlug: tenantSlug,
                    adminEmail: externalUserInfo.Email,
                    adminName: externalUserInfo.Name ?? string.Empty,
                    createdBy: user.Id);

                await tenantRepository.Create(tenant, cancellationToken);
                await tenantRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
        else
        {

            // si el usuario está inactivo, retornar badrequest
            if (!user.IsActive)
            {
                throw new BadRequestException("El usuario está inactivo, debe contactar al administrador");
            }

            // si el usario tiene tenant, verificar que el tenant esté activo
            if (user.TenantId.HasValue)
            {
                Tenant? tenant = await tenantRepository.GetById(user.TenantId.Value, cancellationToken: cancellationToken);
                if (tenant is not null && !tenant.IsActive)
                {
                    throw new BadRequestException("La empresa no está activa, debe contactar al administrador");
                }
            }
        }
        // Generar access token y refresh token usando el servicio JWT
        var accessToken = jwtTokenService.GenerateToken(user!, AppConstants.Authentication.DefaultProvider);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user!);

        // Guardar tokens en el usuario
        IdentityResult resultSetAccessToken = await userManager.SetAuthenticationTokenAsync(user!, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.AccessTokenName, accessToken);
        IdentityResult resultSetRefreshToken = await userManager.SetAuthenticationTokenAsync(user!, AppConstants.Authentication.DefaultProvider, AppConstants.Authentication.RefreshTokenName, refreshToken);

        if (!resultSetAccessToken.Succeeded || !resultSetRefreshToken.Succeeded)
        {
            throw new InvalidOperationException("Error setting tokens");
        }

        AuthUserDto? userInfo = await userInfoService.GetUserInfoAsync(user!);

        return new ExchangeCodeResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = userInfo,
            Provider = request.Provider
        };
    }
    private async Task<User> CreateUserAsync(UserInfoMapping externalUserInfo, RoleManager<Role> roleManager, UserManager<User> userManager, ILogger<ExchangeCodeCommandHandler> logger)
    {

        User newUser = new User
        {
            UserName = externalUserInfo.Email,
            Email = externalUserInfo.Email,
            EmailConfirmed = externalUserInfo.EmailConfirmed,
            Name = externalUserInfo.Name ?? string.Empty
        };

        IdentityResult? result = await userManager.CreateAsync(newUser);
        if (result is not null && !result.Succeeded)
        {
            throw new InvalidOperationException($"Error al crear usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Asignar el rol Admin al usuario
        Role? adminRole = await roleManager.FindByNameAsync(RolesEnum.Admin.ToString());
        if (adminRole is not null)
        {
            var roleResult = await userManager.AddToRoleAsync(newUser, adminRole.Name!);
            if (!roleResult.Succeeded)
            {
                logger.LogWarning("Error al asignar rol Admin al usuario {Email}: {Errors}",
                    newUser.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        return newUser;
    }
}
