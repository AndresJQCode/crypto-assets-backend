using Api.Infrastructure.Middlewares;

namespace Api.Extensions;
/// <summary>
/// Extensión para aplicar permisos a endpoints de manera declarativa
/// </summary>
internal static class EndpointPermissionExtensions
{
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, string resource, string action)
    {
        return builder.WithMetadata(new RequirePermissionAttribute(resource, action));
    }
}
