using Api.Application.Dtos.Auth;
using Domain.AggregatesModel.RoleAggregate;
using Domain.AggregatesModel.UserAggregate;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Api.Application.Services.Auth;

/// <summary>
/// Clase base abstracta que contiene la lógica común para los proveedores de autenticación OAuth
/// </summary>
internal abstract class BaseAuthProviderService(
        // IJwtTokenService jwtTokenService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        // IUserInfoService userInfoService,
        // IOptionsMonitor<OAuthSettings> oAuthSettings,
        ILogger logger) : IAuthProviderService
{


    public abstract string ProviderName { get; }

    /// <summary>
    /// Intercambia el código de autorización por un token de acceso del proveedor externo
    /// </summary>
    protected abstract Task<string> ExchangeCodeForAccessTokenAsync(string code);

    /// <summary>
    /// Obtiene la información del usuario desde el proveedor externo
    /// </summary>
    protected abstract Task<ExternalUserInfo> GetExternalUserWithAccessTokenAsync(string accessToken);


    /// <summary>
    /// Valida si el usuario puede autenticarse (puede ser sobrescrito por implementaciones específicas)
    /// </summary>
    protected virtual Task ValidateUserBeforeAuthenticationAsync(User user)
    {
        // Por defecto, verificar que el usuario esté activo
        if (!user.IsActive)
        {
            logger.LogWarning("Intento de autenticación con usuario inactivo: {Email}", user.Email);
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Mapea la información del usuario externo a un objeto User para crear/actualizar
    /// </summary>
    public abstract UserInfoMapping MapExternalUserInfoToUser(ExternalUserInfo externalUserInfo);

    public async Task<string> ExchangeCodeAsync(string code)
    {
        try
        {
            string? accessToken = await ExchangeCodeForAccessTokenAsync(code);
            return accessToken;
            // var externalUserInfo = await GetExternalUserInfoAsync(accessToken);

            // User user;
            // if (oAuthSettings.CurrentValue.AllowPublicUsers)
            // {
            //     user = await FindOrCreateUserAsync(externalUserInfo);
            // }
            // else
            // {
            //     var foundUser = await FindUserOnlyAsync(externalUserInfo);
            //     if (foundUser == null)
            //     {
            //         logger.LogWarning("Intento de acceso OAuth con usuario no registrado (AllowPublicUsers=false): {Email}", externalUserInfo.Email);
            //         throw new UnAuthorizedException("Usuario no registrado. El acceso con proveedor externo está restringido a usuarios existentes.");
            //     }
            //     user = foundUser;
            //     // Actualizar datos del usuario existente (nombre, etc.)
            //     await UpdateExistingUserAsync(user, MapExternalUserInfoToUser(externalUserInfo));
            // }

            // // 4. Validar usuario antes de autenticar
            // await ValidateUserBeforeAuthenticationAsync(user);

            // // 5. Generar JWT token y refresh token para la aplicación
            // var jwtToken = jwtTokenService.GenerateToken(user, AppConstants.Authentication.DefaultProvider);
            // var refreshToken = jwtTokenService.GenerateRefreshToken(user);

            // // 6. Guardar tokens en el usuario
            // await SaveTokensAsync(user, jwtToken, refreshToken);

            // // 7. Obtener información completa del usuario (roles y permisos)
            // var userInfoWithPermissions = await userInfoService.GetUserInfoAsync(user);

            // // 8. Confirmar email si no está confirmado
            // await ConfirmEmailIfNeededAsync(user);

            // return new ExchangeCodeResponseDto
            // {
            //     AccessToken = jwtToken,
            //     RefreshToken = refreshToken,
            //     User = userInfoWithPermissions,
            //     Provider = ProviderName
            // };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al procesar código de autorización de {Provider}", ProviderName);
            throw;
        }
    }

    public async Task<ExternalUserInfo> GetExternalUserInfoAsync(string accessToken) => await GetExternalUserWithAccessTokenAsync(accessToken) ?? throw new InvalidOperationException("No se pudo obtener la información del usuario externo");

    /// <summary>
    /// Guarda los tokens de autenticación en el usuario
    /// </summary>
    private async Task SaveTokensAsync(User user, string jwtToken, string refreshToken)
    {
        var resultSetAccessToken = await userManager.SetAuthenticationTokenAsync(
            user,
            AppConstants.Authentication.DefaultProvider,
            AppConstants.Authentication.AccessTokenName,
            jwtToken);

        var resultSetRefreshToken = await userManager.SetAuthenticationTokenAsync(
            user,
            AppConstants.Authentication.DefaultProvider,
            AppConstants.Authentication.RefreshTokenName,
            refreshToken);

        if (!resultSetAccessToken.Succeeded || !resultSetRefreshToken.Succeeded)
        {
            var errors = string.Join(", ", resultSetAccessToken.Errors.Concat(resultSetRefreshToken.Errors).Select(e => e.Description));
            logger.LogError("Error al guardar tokens en la base de datos: {Errors}", errors);
            throw new InvalidOperationException($"Error al guardar tokens: {errors}");
        }
    }

    /// <summary>
    /// Confirma el email del usuario si no está confirmado
    /// </summary>
    private async Task ConfirmEmailIfNeededAsync(User user)
    {
        if (!user.EmailConfirmed)
        {
            var result = await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
            if (!result.Succeeded)
            {
                logger.LogError("Error al confirmar email: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    /// <summary>
    /// Busca un usuario por email sin crearlo. Retorna null si no existe.
    /// </summary>
    private async Task<User?> FindUserOnlyAsync(ExternalUserInfo externalUserInfo)
    {
        var email = MapExternalUserInfoToUser(externalUserInfo).Email;
        return await userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// Busca o crea un usuario basado en la información del proveedor externo
    /// </summary>
    private async Task<User> FindOrCreateUserAsync(ExternalUserInfo externalUserInfo)
    {
        var mapping = MapExternalUserInfoToUser(externalUserInfo);
        var email = mapping.Email;

        // Buscar usuario existente por email
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            // Actualizar información del usuario si es necesario
            await UpdateExistingUserAsync(existingUser, mapping);
            return existingUser;
        }

        // Crear nuevo usuario
        return await CreateNewUserAsync(mapping);
    }

    /// <summary>
    /// Actualiza un usuario existente con la información del proveedor externo
    /// </summary>
    protected virtual async Task UpdateExistingUserAsync(User existingUser, UserInfoMapping mapping)
    {
        // Por defecto, actualizar UserName si es diferente
        if (existingUser.UserName != mapping.Email)
        {
            existingUser.UserName = mapping.Email;
            existingUser.NormalizedUserName = mapping.Email.ToUpperInvariant();
            await userManager.UpdateAsync(existingUser);
        }
    }

    /// <summary>
    /// Crea un nuevo usuario basado en la información del proveedor externo
    /// </summary>
    private async Task<User> CreateNewUserAsync(UserInfoMapping mapping)
    {
        var newUser = new User
        {
            UserName = mapping.Email,
            Email = mapping.Email,
            EmailConfirmed = mapping.EmailConfirmed,
            NormalizedEmail = mapping.Email.ToUpperInvariant(),
            // NormalizedUserName = mapping.NormalizedUserName ?? mapping.Email.ToUpperInvariant(),
            Name = mapping.Name ?? string.Empty
        };

        var result = await userManager.CreateAsync(newUser);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Error al crear usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Asignar el rol Admin al usuario
        var adminRole = await roleManager.FindByNameAsync(RolesEnum.Admin.ToString());
        if (adminRole != null)
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

/// <summary>
/// Clase que contiene la información del usuario de diferentes proveedores OAuth
/// </summary>
internal sealed class ExternalUserInfo
{
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public bool EmailConfirmed { get; init; }
}

/// <summary>
/// DTO para mapear información del usuario externo a propiedades de User
/// </summary>
internal sealed class UserInfoMapping
{
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool EmailConfirmed { get; set; }
}
