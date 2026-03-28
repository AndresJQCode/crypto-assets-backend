using Api.Infrastructure.Middlewares;

namespace Api.Extensions;

/// <summary>
/// Extensiones para configurar el middleware de autorización de permisos
/// </summary>
internal static class PermissionMiddlewareExtensions
{
    /// <summary>
    /// Agregar el middleware de autorización de permisos
    /// </summary>
    public static IApplicationBuilder UsePermissionAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PermissionAuthorizationMiddleware>();
    }

    /// <summary>
    /// Configurar las opciones del middleware de permisos
    /// </summary>
    public static IServiceCollection AddPermissionMiddleware(this IServiceCollection services)
    {
        return services;
    }
}
