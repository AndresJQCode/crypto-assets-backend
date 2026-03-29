using Api.Application.Services.Auth;
using Api.Infrastructure.Services;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Services.Auth;

namespace Api.Extensions;

internal static class AuthExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuración OAuth (AllowPublicUsers: crear usuario si no existe vs solo permitir usuarios existentes)
        services.Configure<OAuthSettings>(configuration.GetSection("Authentication"));

        // Obtener configuración para timeouts
        var authSettings = configuration.GetSection("Authentication").Get<AppSettings.AuthenticationSettings>();
        var googleTimeout = authSettings?.Google?.TimeoutSeconds ?? 30;
        var microsoftTimeout = authSettings?.Microsoft?.TimeoutSeconds ?? 30;

        // Registrar servicios de infraestructura con timeouts configurados
        services.AddHttpClient<IGoogleOAuthService, GoogleOAuthService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(googleTimeout);
        });

        services.AddHttpClient<IMicrosoftOAuthService, MicrosoftOAuthService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(microsoftTimeout);
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IUserInfoService, UserInfoService>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Registrar los proveedores de autenticación
        services.AddScoped<IAuthProviderService, GoogleAuthProviderService>();
        services.AddScoped<IAuthProviderService, MicrosoftAuthProviderService>();

        // Registrar el factory
        services.AddScoped<IAuthProviderFactory, AuthProviderFactory>();

        return services;
    }
}
