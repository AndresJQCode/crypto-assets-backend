using Api.Application.Commands.PermissionRoleCommands;
using Api.Application.Commands.RoleCommands;
using Api.Application.Dtos.Permission;
using Api.Application.Dtos.Role;
using Api.Application.Dtos.User;
using Api.Application.Queries.RoleQueries;
using Api.Constants;
using Api.Extensions;
using Domain.Exceptions;
using MediatR;

namespace Api.Apis.RolesEndpoints;

internal static class RolesApi
{
    public static RouteGroupBuilder MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("roles");


        // GET /roles - Obtiene todos los roles (con o sin permisos)
        group.MapGet("/", async (IMediator mediator, bool includePermissions = false) =>
        {
            if (includePermissions)
            {
                var query = new GetAllRolesWithPermissionsQuery();
                IEnumerable<RoleWithPermissionsDto>? roles = await mediator.Send(query);
                return Results.Ok(roles);
            }
            else
            {
                var query = new GetAllRolesSimpleQuery();
                IEnumerable<RoleDto>? roles = await mediator.Send(query);
                return Results.Ok(roles);
            }
        })
        .WithName("GetAllRoles")
        .WithSummary("Obtener todos los roles")
        .WithDescription("Obtiene todos los roles disponibles. Use ?includePermissions=true para incluir permisos. Requiere permiso: Roles.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Read)
        .Produces<IEnumerable<RoleDto>>()
        .Produces<IEnumerable<RoleWithPermissionsDto>>();

        // GET /roles/{id} - Obtiene un rol por ID (con o sin permisos)
        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id, bool includePermissions = false) =>
        {
            if (includePermissions)
            {
                var query = new GetRoleByIdWithPermissionsQuery(id);
                var role = await mediator.Send(query);

                if (role == null)
                {
                    return Results.NotFound(new { message = "Rol no encontrado" });
                }

                return Results.Ok(role);
            }
            else
            {
                var query = new GetRoleByIdQuery(id);
                var role = await mediator.Send(query);

                if (role == null)
                {
                    return Results.NotFound(new { message = "Rol no encontrado" });
                }

                return Results.Ok(role);
            }
        })
        .WithName("GetRoleById")
        .WithSummary("Obtener rol por ID")
        .WithDescription("Obtiene un rol específico por su ID. Use ?includePermissions=true para incluir permisos. Requiere permiso: Roles.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Read)
        .Produces<RoleWithPermissionsDto>()
        .Produces<NotFoundException>();

        // GET /roles/{id}/users - Obtiene los usuarios de un rol
        group.MapGet("/{id:guid}/users", async (IMediator mediator, Guid id) =>
        {
            var query = new GetRoleUsersQuery(id);
            var users = await mediator.Send(query);
            return Results.Ok(users);
        })
        .WithName("GetRoleUsers")
        .WithSummary("Obtener usuarios de rol")
        .WithDescription("Obtiene los usuarios asignados a un rol específico. Requiere permiso: Roles.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Read)
        .Produces<IEnumerable<UserResponseDto>>();

        // POST /roles - Crea un nuevo rol
        group.MapPost("/", async (IMediator mediator, CreateRoleDto dto) =>
        {
            var command = new CreateRoleCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                PermissionIds = dto.PermissionIds
            };

            var role = await mediator.Send(command);
            return Results.Created($"/roles/{role.Id}", role);
        })
        .WithName("CreateRole")
        .WithSummary("Crear rol")
        .WithDescription("Crea un nuevo rol en el sistema y asigna permisos si se proporcionan. Requiere permiso: Roles.Create")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Create)
        .Produces<RoleDto>(201)
        .Produces<BadRequestException>();

        // PUT /roles/{id} - Actualiza un rol existente con sus permisos
        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateRoleWithPermissionsCommand command) =>
        {
            command.Id = id; // Asegurar que el ID del path se use

            var result = await mediator.Send(command);
            return Results.Ok(result);
        })
        .WithName("UpdateRoleWithPermissions")
        .WithSummary("Actualizar rol con permisos")
        .WithDescription("Actualiza un rol existente incluyendo su nombre, descripción y permisos en una sola operación. Requiere permiso: Roles.Update")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Update)
        .Produces<UpdateRoleWithPermissionsResponse>()
        .Produces<NotFoundException>()
        .Produces<BadRequestException>();

        // DELETE /roles/{id} - Elimina un rol
        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var command = new DeleteRoleCommand(id);
            var result = await mediator.Send(command);

            if (result)
            {
                return Results.NoContent();
            }

            return Results.BadRequest(new { message = "Error al eliminar el rol" });
        })
        .WithName("DeleteRole")
        .WithSummary("Eliminar rol")
        .WithDescription("Elimina un rol del sistema. Requiere permiso: Roles.Delete")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Delete)
        .Produces<NotFoundException>()
        .Produces<BadRequestException>();


        // GET /roles/{id}/permissions - Obtiene todos los permisos de un rol específico
        group.MapGet("/{id:guid}/permissions", async (IMediator mediator, Guid id) =>
        {
            var query = new GetRolePermissionsQuery(id);
            var permissions = await mediator.Send(query);
            return Results.Ok(permissions);
        })
        .WithName("GetRolePermissions")
        .WithSummary("Obtener permisos de rol")
        .WithDescription("Obtiene todos los permisos asignados a un rol específico. Requiere permiso: Permissions.Read")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
        .Produces<IEnumerable<PermissionDto>>();


        // DELETE /roles/{id}/permissions/{permissionId} - Remueve un permiso de un rol
        group.MapDelete("/{id:guid}/permissions/{permissionId:guid}", async (IMediator mediator, Guid id, Guid permissionId) =>
        {
            var command = new RemovePermissionFromRoleCommand(permissionId: permissionId, roleId: id);
            var result = await mediator.Send(command);

            if (result)
            {
                return Results.NoContent();
            }

            return Results.BadRequest(new { message = "Error al remover el permiso del rol", success = false });
        })
        .WithName("RemovePermissionFromRole")
        .WithSummary("Remover permiso de rol")
        .WithDescription("Remueve un permiso específico de un rol. Requiere permiso: Permissions.Assign")
        .RequireAuthorization()
        .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Update)
        .Produces<NotFoundException>()
        .Produces<BadRequestException>();

        return group;
    }
}
