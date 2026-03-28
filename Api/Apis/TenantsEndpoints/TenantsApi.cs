using Api.Application.Commands.TenantCommands;
using Api.Application.Dtos.Tenant;
using Api.Application.Queries.TenantQueries;
using Api.Extensions;
using Infrastructure.Constants;
using MediatR;

namespace Api.Apis.TenantsEndpoints;

internal static class TenantsApi
{
    public static RouteGroupBuilder MapTenantsEndpoints(this RouteGroupBuilder adminGroup)
    {
        var group = adminGroup.MapGroup("/tenants")
            .WithTags("Admin - Tenants");

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllTenantsQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetAllTenants")
        .WithSummary("Listar tenants")
        .WithDescription("Lista todos los tenants. Solo SuperAdmin.")
        .Produces<IReadOnlyList<TenantDto>>();

        group.MapGet("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTenantByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetTenantById")
        .WithSummary("Obtener tenant por ID")
        .WithDescription("Obtiene un tenant por su ID. Solo SuperAdmin.")
        .Produces<TenantDto>()
        .Produces(404);

        group.MapPut("/{id:guid}", async (IMediator mediator, Guid id, UpdateTenantDto dto, CancellationToken ct) =>
        {
            var cmd = new UpdateTenantCommand { Id = id, Name = dto.Name, Slug = dto.Slug, IsActive = dto.IsActive };
            var result = await mediator.Send(cmd, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateTenant")
        .WithSummary("Actualizar tenant")
        .WithDescription("Actualiza un tenant existente. Solo SuperAdmin.")
        .Produces<TenantDto>()
        .Produces(404);

        group.MapDelete("/{id:guid}", async (IMediator mediator, Guid id, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteTenantCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeleteTenant")
        .WithSummary("Eliminar tenant")
        .WithDescription("Elimina (soft delete) un tenant. Solo SuperAdmin.")
        .Produces(204)
        .Produces(404)
        .Produces(400);

        return group;
    }
}
