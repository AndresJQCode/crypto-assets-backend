// Este archivo ya no es necesario ya que todos los endpoints han sido movidos
// a sus respectivos APIs siguiendo las convenciones REST:
// - Endpoints de usuarios -> UsersApi
// - Endpoints de roles -> RolesApi
// - Endpoints de permisos -> PermissionsApi

namespace Api.Apis.PermissionRolesEndpoints;

internal static class PermissionRolesApi
{
    public static RouteGroupBuilder MapPermissionRolesEndpoints(this RouteGroupBuilder tenantGroup)
    {
        // Este grupo ya no tiene endpoints ya que todos han sido movidos
        // a sus respectivos APIs para seguir las convenciones REST
        var group = tenantGroup.MapGroup("/permission-roles")
            .WithTags("Tenant - Permission Roles");

        // TODO: Agregar aquí futuras funcionalidades específicas de relaciones
        // que no pertenezcan a ningún recurso específico

        return group;
    }
}
