using Api.Application.Commands.ConnectorDefinitionCommands;
using Api.Application.Dtos;
using Api.Application.Dtos.ConnectorDefinition;
using Api.Application.Queries.ConnectorDefinitionQueries;
using Api.Extensions;
using MediatR;

namespace Api.Apis.ConnectorDefinitionsEndpoints;

internal static class ConnectorDefinitionsApi
{
    public static RouteGroupBuilder MapConnectorDefinitionsEndpoints(this RouteGroupBuilder adminGroup)
    {
        var group = adminGroup.MapGroup("/connector-definitions")
            .WithTags("Admin - Connector Definitions");

        group.MapGetPaginated<GetAllConnectorDefinitionsQuery, ConnectorDefinitionDto>(
            "/",
            () => new GetAllConnectorDefinitionsQuery())
            .WithName("GetAllConnectorDefinitions")
            .WithSummary("Listar definiciones de conectores")
            .WithDescription("Lista definiciones de conectores con paginación (query: page, limit). Solo SuperAdmin.")
            .Produces<PaginationResponseDto<ConnectorDefinitionDto>>();

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetConnectorDefinitionByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetConnectorDefinitionById")
        .WithSummary("Obtener definición de conector por ID")
        .WithDescription("Obtiene una definición de conector por su ID. Solo SuperAdmin.")
        .Produces<ConnectorDefinitionDto>()
        .Produces(404);

        group.MapPost("/", async (IMediator mediator, CreateConnectorDefinitionDto dto, CancellationToken ct) =>
        {
            var command = new CreateConnectorDefinitionCommand
            {
                Name = dto.Name,
                LogoUrl = dto.LogoUrl,
                ProviderType = dto.ProviderType,
                CategoryType = dto.CategoryType,
                RequiresOAuth = dto.RequiresOAuth,
                Description = dto.Description
            };
            var result = await mediator.Send(command, ct);
            return Results.Created($"/admin/connector-definitions/{result.Id}", result);
        })
        .WithName("CreateConnectorDefinition")
        .WithSummary("Crear definición de conector")
        .WithDescription("Crea una nueva definición de conector en el catálogo. Solo SuperAdmin.")
        .Produces<ConnectorDefinitionDto>(201)
        .Produces(400);

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateConnectorDefinitionDto dto, CancellationToken ct) =>
        {
            var command = new UpdateConnectorDefinitionCommand
            {
                Id = id,
                Name = dto.Name,
                LogoUrl = dto.LogoUrl,
                Description = dto.Description
            };
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateConnectorDefinition")
        .WithSummary("Actualizar definición de conector")
        .WithDescription("Actualiza una definición de conector existente. Solo SuperAdmin.")
        .Produces<ConnectorDefinitionDto>()
        .Produces(404)
        .Produces(400);

        group.MapPatch("/{id:guid}/active", async (IMediator mediator, Guid id, bool isActive, CancellationToken ct) =>
        {
            var command = new SetConnectorDefinitionActiveCommand(id, isActive);
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("SetConnectorDefinitionActive")
        .WithSummary("Activar o desactivar definición de conector")
        .WithDescription("Activa o desactiva una definición de conector. Solo SuperAdmin.")
        .Produces<ConnectorDefinitionDto>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteConnectorDefinitionCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeleteConnectorDefinition")
        .WithSummary("Eliminar definición de conector")
        .WithDescription("Elimina (soft delete) una definición de conector. Solo SuperAdmin.")
        .Produces(204)
        .Produces(404)
        .Produces(400);

        return group;
    }
}
