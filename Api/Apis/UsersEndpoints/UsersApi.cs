using Api.Application.Commands.UserCommands;
using Api.Application.Dtos;
using Api.Application.Dtos.Permission;
using Api.Application.Dtos.Role;
using Api.Application.Dtos.User;
using Api.Application.Queries.PermissionQueries;
using Api.Application.Queries.RoleQueries;
using Api.Application.Queries.Users;
using Api.Application.Queries.UsersQueries;
using Api.Constants;
using Api.Extensions;
using Api.Utilities;
using Domain.AggregatesModel.PermissionAggregate;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;


namespace Api.Apis.UsersEndpoints;

internal static class UsersApi
{
    public static RouteGroupBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder api = app.MapGroup("users");
        api.MapGet("/", GetAll)
            .WithName("GetAllUsers")
            .WithSummary("Obtener todos los usuarios")
            .WithDescription("Obtiene una lista paginada de todos los usuarios. Requiere permiso: Users.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Read)
            .Produces<PaginationResponseDto<UserResponseDto>>();
        api.MapGet("/{id:guid}", GetById)
            .WithName("GetUserById")
            .WithSummary("Obtener usuario por ID")
            .WithDescription("Obtiene un usuario específico por su ID. Requiere permiso: Users.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Read)
            .Produces<UserResponseDto>()
            .Produces<NotFoundException>();
        api.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteUser")
            .WithSummary("Eliminar usuario")
            .WithDescription("Elimina un usuario por su ID (eliminación lógica). Requiere permiso: Users.Delete")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Delete)
            .Produces<NoContent>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();
        api.MapPut("/{id:guid}/status", UpdateStatus)
            .WithName("UpdateUserStatus")
            .WithSummary("Actualizar estado del usuario")
            .WithDescription("Activa o desactiva un usuario por su ID. Requiere permiso: Users.Update")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Update)
            .Produces<NoContent>()
            .Produces<NotFoundException>()
            .Produces<ProblemHttpResult>();
        api.MapPost("", Create)
            .WithName("CreateUser")
            .WithSummary("Crear usuario")
            .WithDescription("Crea un nuevo usuario en el sistema. Requiere permiso: Users.Create")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Create)
            .Produces<UserResponseDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest);
        api.MapPut("/{id:guid}", Update)
            .WithName("UpdateUser")
            .WithSummary("Actualizar usuario")
            .WithDescription("Actualiza un usuario existente incluyendo sus datos básicos y roles asignados. Si se proporcionan RoleIds, se actualizarán los roles del usuario. Requiere permiso: Users.Update")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Users, PermissionConstants.Actions.Update)
            .Produces<UserResponseDto>()
            .Produces<NotFoundException>()
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .Produces<ProblemHttpResult>();

        // GET /users/{id}/permissions - Obtiene los permisos de un usuario específico
        api.MapGet("/{id:guid}/permissions", GetUserPermissions)
            .WithName("GetUserPermissions")
            .WithSummary("Obtener permisos de usuario")
            .WithDescription("Obtiene los permisos de un usuario específico. Requiere permiso: Permissions.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
            .Produces<IEnumerable<UserPermissionDto>>()
            .Produces<ProblemHttpResult>();

        // GET /users/{id}/permissions/detailed - Obtiene los permisos detallados de un usuario específico
        api.MapGet("/{id:guid}/permissions/detailed", GetUserPermissionsDetailed)
            .WithName("GetUserPermissionsDetailed")
            .WithSummary("Obtener permisos detallados por usuario")
            .WithDescription("Obtiene los permisos detallados asignados a un usuario específico. Requiere permiso: Permissions.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
            .Produces<IEnumerable<PermissionDto>>()
            .Produces<ProblemHttpResult>();

        // GET /users/{id}/permissions/check - Verifica si un usuario tiene un permiso específico
        api.MapGet("/{id:guid}/permissions/check", CheckUserPermission)
            .WithName("CheckUserPermission")
            .WithSummary("Verificar permiso de usuario")
            .WithDescription("Verifica si un usuario tiene un permiso específico. Requiere permiso: Permissions.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
            .Produces<bool>()
            .Produces<ProblemHttpResult>();

        // GET /users/{id}/permissions-by-roles - Obtiene todos los permisos de un usuario (a través de sus roles)
        api.MapGet("/{id:guid}/permissions-by-roles", GetUserPermissionsByRoles)
            .WithName("GetUserPermissionsByRoles")
            .WithSummary("Obtener permisos de usuario por roles")
            .WithDescription("Obtiene todos los permisos de un usuario específico a través de sus roles asignados. Requiere permiso: Permissions.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Permissions, PermissionConstants.Actions.Read)
            .Produces<IEnumerable<PermissionDto>>()
            .Produces<ProblemHttpResult>();

        // GET /users/{id}/roles - Obtiene los roles de un usuario
        api.MapGet("/{id:guid}/roles", GetUserRoles)
            .WithName("GetUserRoles")
            .WithSummary("Obtener roles de usuario")
            .WithDescription("Obtiene los roles asignados a un usuario específico. Requiere permiso: Roles.Read")
            .RequireAuthorization()
            .RequirePermission(PermissionConstants.Resources.Roles, PermissionConstants.Actions.Read)
            .Produces<IEnumerable<RoleDto>>()
            .Produces<ProblemHttpResult>();


        return api;
    }

    private static async Task<PaginationResponseDto<UserResponseDto>> GetAll(IMediator mediator, IHttpContextAccessor httpContextAccessor, CancellationToken cancellationToken)
    {
        PaginationParameters? paginationParameters = PaginationHelper.GetPaginationParametersFromQueryString(httpContextAccessor);
        return await mediator.Send(new GetAllUsersQuery { PaginationParameters = paginationParameters }, cancellationToken);
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
