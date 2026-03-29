using Api.Application.Dtos.Permission;
using Api.Application.Queries.PermissionQueries;
using Api.Constants;
using Api.Extensions;
using Domain.Exceptions;
using MediatR;

namespace Api.Apis.PermissionsEndpoints;

internal static class PermissionsApi
{
    public static RouteGroupBuilder MapPermissionsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("permissions");

        // GET /permissions - Obtiene todos los permisos
        group.MapGet("", async (IMediator mediator, string? resource = null) =>
        {
            var query = new GetAllPermissionsSimpleQuery
            {
                Resource = resource
            };
            var permissions = await mediator.Send(query);
            return Results.Ok(permissions);
        })
        .WithName("GetAllPermissions")
        .WithSummary("Obtener todos los permisos")
        .WithDescription("Obtiene todos los permisos disponibles con filtros opcionales. Requiere permiso: Permissions.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
        .Produces<IEnumerable<PermissionDto>>();

        // GET /permissions/{id} - Obtiene un permiso por ID
        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var query = new GetPermissionByIdQuery(id);
            var permission = await mediator.Send(query);

            return permission is not null ? Results.Ok(permission) : Results.NotFound();
        })
        .WithName("GetPermissionById")
        .WithSummary("Obtener permiso por ID")
        .WithDescription("Obtiene un permiso específico por su ID. Requiere permiso: Permissions.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
        .Produces<PermissionDto>()
        .Produces<NotFoundException>();

        return group;
    }
}
