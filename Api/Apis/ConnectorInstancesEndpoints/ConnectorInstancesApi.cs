using Api.Application.Commands.ConnectorInstanceCommands;
using Api.Application.Dtos.ConnectorInstance;
using Api.Application.Queries.ConnectorInstanceQueries;
using Api.Extensions;
using Infrastructure.Constants;
using MediatR;

namespace Api.Apis.ConnectorInstancesEndpoints;

internal static class ConnectorInstancesApi
{
    public static RouteGroupBuilder MapConnectorInstancesEndpoints(this RouteGroupBuilder tenantGroup)
    {
        var group = tenantGroup.MapGroup("/connector-instances")
            .WithTags("Tenant - Connector Instances");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetConnectorInstancesQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetConnectorInstances")
        .WithSummary("Listar conectores del tenant")
        .WithDescription("Lista los conectores del tenant actual. Requiere permiso: ConnectorInstances.Read")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Read)
        .Produces<IReadOnlyList<ConnectorInstanceDto>>();

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetConnectorInstanceByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetConnectorInstanceById")
        .WithSummary("Obtener conector por ID")
        .WithDescription("Obtiene un conector por su ID. Requiere permiso: ConnectorInstances.Read")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Read)
        .Produces<ConnectorInstanceDto>()
        .Produces(404);

        group.MapPost("/", async (IMediator mediator, CreateConnectorInstanceDto dto, CancellationToken ct) =>
        {
            var command = new CreateConnectorInstanceCommand
            {
                ConnectorDefinitionId = dto.ConnectorDefinitionId,
                Name = dto.Name,
                ConfigurationJson = dto.ConfigurationJson,
                AccessToken = dto.AccessToken
            };
            var result = await mediator.Send(command, ct);
            return Results.Created($"/connector-instances/{result.Id}", result);
        })
        .WithName("CreateConnectorInstance")
        .WithSummary("Crear conector")
        .WithDescription("Crea una nueva instancia de conector (p. ej. tras OAuth). Requiere permiso: ConnectorInstances.Create")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Create)
        .Produces<ConnectorInstanceDto>(201)
        .Produces(400)
        .Produces(404);

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateConnectorInstanceDto dto, CancellationToken ct) =>
        {
            var command = new UpdateConnectorInstanceCommand
            {
                Id = id,
                Name = dto.Name,
                ConfigurationJson = dto.ConfigurationJson
            };
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateConnectorInstance")
        .WithSummary("Actualizar conector")
        .WithDescription("Actualiza nombre y/o configuración del conector. Requiere permiso: ConnectorInstances.Update")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Update)
        .Produces<ConnectorInstanceDto>()
        .Produces(404)
        .Produces(400);

        group.MapPatch("/{id:guid}/enabled", async (IMediator mediator, Guid id, bool isEnabled, CancellationToken ct) =>
        {
            var command = new SetConnectorInstanceEnabledCommand(id, isEnabled);
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("SetConnectorInstanceEnabled")
        .WithSummary("Habilitar o deshabilitar conector")
        .WithDescription("Habilita o deshabilita un conector. Requiere permiso: ConnectorInstances.Update")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Update)
        .Produces<ConnectorInstanceDto>()
        .Produces(404)
        .Produces(400);

        group.MapPost("/{id:guid}/validate", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ValidateConnectorInstanceCommand(id), ct);
            return Results.Ok(result);
        })
        .WithName("ValidateConnectorInstance")
        .WithSummary("Validar conexión del conector")
        .WithDescription("Comprueba el estado de la conexión del conector. Requiere permiso: ConnectorInstances.Update")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Update)
        .Produces<ValidateConnectorInstanceResult>()
        .Produces(404)
        .Produces(400);

        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteConnectorInstanceCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeleteConnectorInstance")
        .WithSummary("Eliminar conector")
        .WithDescription("Elimina (soft delete) un conector. Requiere permiso: ConnectorInstances.Delete")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Delete)
        .Produces(204)
        .Produces(404)
        .Produces(400);

        return group;
    }
}
