using Api.Application.Commands.BybitCommands;
using Api.Application.Dtos.ConnectorInstance;
using Api.Application.Dtos.Trading;
using Api.Application.Queries.BybitQueries;
using Api.Extensions;
using Infrastructure.Constants;
using MediatR;

namespace Api.Apis.BybitEndpoints;

internal static class BybitApi
{
    public static RouteGroupBuilder MapBybitEndpoints(this RouteGroupBuilder tenantGroup)
    {
        var group = tenantGroup.MapGroup("/bybit")
            .WithTags("Tenant - Bybit Trading");

        // POST /api/tenants/{tenantId}/bybit/connect
        group.MapPost("/connect", async (
            IMediator mediator,
            CreateBybitConnectorDto dto,
            CancellationToken ct) =>
        {
            var command = new CreateBybitConnectorCommand
            {
                Name = dto.Name,
                ApiKey = dto.ApiKey,
                ApiSecret = dto.ApiSecret,
                IsTestnet = dto.IsTestnet
            };

            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/connectors/{result.Id}", result);
        })
        .WithName("ConnectBybit")
        .WithSummary("Connect Bybit account with API keys")
        .WithDescription("Creates a Bybit connector instance by validating and storing encrypted API credentials. Only one Bybit connector per user is allowed.")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Create)
        .Produces<ConnectorInstanceDto>(201)
        .Produces(400)
        .Produces(401);

        // GET /api/tenants/{tenantId}/bybit/orders/open?symbol={BTCUSDT}
        group.MapGet("/orders/open", async (
            IMediator mediator,
            string? symbol,
            CancellationToken ct) =>
        {
            var query = new GetOpenOrdersQuery
            {
                Symbol = symbol
            };

            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetBybitOpenOrders")
        .WithSummary("Get active orders from Bybit")
        .WithDescription("Fetches current open orders (New, PartiallyFilled, Untriggered) from Bybit exchange for the authenticated user. Results are cached for 30 seconds to respect rate limits. The connector is automatically detected based on the user's tenant.")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Read)
        .Produces<List<TradingOrderDto>>()
        .Produces(400)
        .Produces(401)
        .Produces(404);

        // POST /api/tenants/{tenantId}/bybit/sync-history
        group.MapPost("/sync-history", async (
            IMediator mediator,
            SyncBybitHistoryRequest request,
            CancellationToken ct) =>
        {
            var command = new SyncBybitHistoryCommand
            {
                ConnectorInstanceId = request.ConnectorInstanceId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Symbol = request.Symbol
            };

            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("SyncBybitHistory")
        .WithSummary("Synchronize full trading history from Bybit")
        .WithDescription("Fetches and stores complete trading history from Bybit into the database. Supports filtering by date range and trading pair. Process runs in chunks to avoid rate limits.")
        .RequirePermission(PermissionResourcesConstants.ConnectorInstances, PermissionActionsConstants.Update)
        .Produces<SyncBybitHistoryResult>()
        .Produces(400)
        .Produces(401)
        .Produces(404);

        return group;
    }
}
