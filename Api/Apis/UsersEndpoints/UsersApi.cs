using Api.Application.Commands.UserCommands;
using Api.Application.Dtos;
using Api.Application.Dtos.Permission;
using Api.Application.Dtos.Role;
using Api.Application.Dtos.User;
using Api.Application.Queries.PermissionQueries;
using Api.Application.Queries.RoleQueries;
using Api.Application.Queries.Users;
using Api.Application.Queries.UsersQueries;
using Api.Extensions;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.Exceptions;
using Infrastructure.Constants;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Api.Apis.UsersEndpoints;

internal static class UsersApi
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder tenantGroup)
    {
        RouteGroupBuilder api = tenantGroup.MapGroup("/users")
            .WithTags("Tenant - Users");

        api.MapGetPaginated<GetAllUsersQuery, UserResponseDto>(
            "/",
            () => new GetAllUsersQuery())
            .WithName("GetAllUsers")
            .WithSummary("Obtener todos los usuarios")
            .WithDescription("Obtiene una lista paginada de todos los usuarios con búsqueda y ordenamiento (query: ?page=1&limit=20&search=nombre&sortBy=name&sortOrder=asc)")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Read)
            .Produces<PaginationResponseDto<UserResponseDto>>();
        api.MapGet("/{id:guid}", GetById)
            .WithName("GetUserById")
            .WithSummary("Obtener usuario por ID")
            .WithDescription("Obtiene un usuario específico por su ID. Requiere permiso: Users.Read")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Read)
            .Produces<UserResponseDto>()
            .Produces<NotFoundException>();
        api.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteUser")
            .WithSummary("Eliminar usuario")
            .WithDescription("Elimina un usuario por su ID (eliminación lógica). Requiere permiso: Users.Delete")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Delete)
            .Produces<NoContent>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();
        api.MapPut("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateUserStatus")
            .WithSummary("Actualizar estado del usuario")
            .WithDescription("Activa o desactiva un usuario por su ID. Requiere permiso: Users.Update")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Update)
            .Produces<NoContent>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();
        api.MapPost("", Create)
            .WithName("CreateUser")
            .WithSummary("Crear usuario")
            .WithDescription("Crea un nuevo usuario en el sistema. Requiere permiso: Users.Create")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Create)
            .Produces<UserResponseDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);
        api.MapPut("/{id:guid}", Update)
            .WithName("UpdateUser")
            .WithSummary("Actualizar usuario")
            .WithDescription("Actualiza un usuario existente incluyendo sus datos básicos y roles asignados. Si se proporcionan RoleIds, se actualizarán los roles del usuario. Requiere permiso: Users.Update")
            .RequirePermission(PermissionResourcesConstants.Users, PermissionActionsConstants.Update)
            .Produces<UserResponseDto>()
            .Produces<NotFoundException>()
            .Produces<BadRequestException>(StatusCodes.Status400BadRequest);

        // GET /users/{id}/permissions - Obtiene los permisos de un usuario específico
        api.MapGet("/{id:guid}/permissions", GetUserPermissions)
            .WithName("GetUserPermissions")
            .WithSummary("Obtener permisos de usuario")
            .WithDescription("Obtiene los permisos de un usuario específico. Requiere permiso: Permissions.Read")
            .RequirePermission(PermissionResourcesConstants.Permissions, PermissionActionsConstants.Read)
            .Produces<IEnumerable<UserPermissionDto>>();

        // GET /users/{id}/permissions/detailed - Obtiene los permisos detallados de un usuario específico
        api.MapGet("/{id:guid}/permissions/detailed", GetUserPermissionsDetailed)
            .WithName("GetUserPermissionsDetailed")
            .WithSummary("Obtener permisos detallados por usuario")
            .WithDescription("Obtiene los permisos detallados asignados a un usuario específico. Requiere permiso: Permissions.Read")
            .RequirePermission(PermissionResourcesConstants.Permissions, PermissionActionsConstants.Read)
            .Produces<IEnumerable<PermissionDto>>();

        // GET /users/{id}/permissions/check - Verifica si un usuario tiene un permiso específico
        api.MapGet("/{id:guid}/permissions/check", CheckUserPermission)
            .WithName("CheckUserPermission")
            .WithSummary("Verificar permiso de usuario")
            .WithDescription("Verifica si un usuario tiene un permiso específico. Requiere permiso: Permissions.Read")
            .RequirePermission(PermissionResourcesConstants.Permissions, PermissionActionsConstants.Read)
            .Produces<bool>();

        // GET /users/{id}/permissions-by-roles - Obtiene todos los permisos de un usuario (a través de sus roles)
        api.MapGet("/{id:guid}/permissions-by-roles", GetUserPermissionsByRoles)
            .WithName("GetUserPermissionsByRoles")
            .WithSummary("Obtener permisos de usuario por roles")
            .WithDescription("Obtiene todos los permisos de un usuario específico a través de sus roles asignados. Requiere permiso: Permissions.Read")
            .RequirePermission(PermissionResourcesConstants.Permissions, PermissionActionsConstants.Read)
            .Produces<IEnumerable<PermissionDto>>();

        // GET /users/{id}/roles - Obtiene los roles de un usuario
        api.MapGet("/{id:guid}/roles", GetUserRoles)
            .WithName("GetUserRoles")
            .WithSummary("Obtener roles de usuario")
            .WithDescription("Obtiene los roles asignados a un usuario específico. Requiere permiso: Roles.Read")
            .RequirePermission(PermissionResourcesConstants.Roles, PermissionActionsConstants.Read)
            .Produces<IEnumerable<RoleDto>>();


        return api;
    }

    private static async Task<UserResponseDto> GetById(IMediator mediator, Guid id, CancellationToken cancellationToken) => await mediator.Send(new GetUserByIdQuery(id), cancellationToken);


    private static async Task<IResult> Delete(IMediator mediator, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (result)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

    private static async Task<IResult> UpdateStatus(IMediator mediator, Guid id, UpdateUserStatusDto request, CancellationToken cancellationToken)
    {
        UpdateUserStatusCommand command = new UpdateUserStatusCommand(id, request.IsActive);
        bool result = await mediator.Send(command, cancellationToken);

        if (result)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

    private static async Task<IResult> Create(IMediator mediator, CreateUserDto request, CancellationToken cancellationToken)
    {
        CreateUserCommand? command = new CreateUserCommand(Email: request.Email, Name: request.Name, Roles: request.Roles);
        UserResponseDto? result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/users/{result.Id}", result);
    }

    private static async Task<IResult> GetUserPermissions(IMediator mediator, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsDetailedQuery(id);
        IEnumerable<UserPermissionDto>? permissions = await mediator.Send(query, cancellationToken);
        return TypedResults.Ok(permissions);
    }

    private static async Task<IResult> GetUserPermissionsDetailed(IMediator mediator, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPermissionsByUserIdQuery(id);
        IEnumerable<PermissionDto>? permissions = await mediator.Send(query, cancellationToken: cancellationToken);
        return TypedResults.Ok(permissions);
    }

    private static async Task<IResult> CheckUserPermission(IMediator mediator, Guid id, string resource, string action, CancellationToken cancellationToken)
    {
        var query = new CheckUserPermissionQuery(UserId: id, Resource: resource, Action: action);
        bool hasPermission = await mediator.Send(query, cancellationToken: cancellationToken);
        return TypedResults.Ok(hasPermission);
    }

    private static async Task<IResult> GetUserPermissionsByRoles(IMediator mediator, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsQuery(UserId: id);
        IEnumerable<Permission>? permissions = await mediator.Send(query, cancellationToken: cancellationToken);
        return TypedResults.Ok(permissions);
    }

    private static async Task<IResult> GetUserRoles(IMediator mediator, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserRolesQuery(UserId: id);
        IEnumerable<RoleDto>? roles = await mediator.Send(query, cancellationToken: cancellationToken);
        return TypedResults.Ok(roles);
    }


    private static async Task<IResult> Update(IMediator mediator, Guid id, UpdateUserDto request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(Id: id, Name: request.Name, Email: request.Email, RoleIds: request.RoleIds);
        UserResponseDto? result = await mediator.Send(command, cancellationToken: cancellationToken);
        return TypedResults.Ok(result);
    }
}
