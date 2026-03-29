// Este archivo ya no es necesario ya que todos los endpoints han sido movidos
// a sus respectivos APIs siguiendo las convenciones REST:
// - Endpoints de usuarios -> UsersApi
// - Endpoints de roles -> RolesApi
// - Endpoints de permisos -> PermissionsApi

namespace Api.Apis.PermissionRolesEndpoints;

internal static class PermissionRolesApi
{
    public static RouteGroupBuilder MapPermissionRolesEndpoints(this IEndpointRouteBuilder app)
    {
        // Este grupo ya no tiene endpoints ya que todos han sido movidos
        // a sus respectivos APIs para seguir las convenciones REST
        var group = app.MapGroup("permission-roles");

        // TODO: Agregar aquí futuras funcionalidades específicas de relaciones
        // que no pertenezcan a ningún recurso específico

        return group;
    }
}
