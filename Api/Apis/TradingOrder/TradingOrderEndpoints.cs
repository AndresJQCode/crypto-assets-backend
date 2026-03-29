using Api.Application.Dtos.Common;
using Api.Application.Dtos.TradingOrder;
using Api.Application.Queries.TradingOrder;
using Api.Extensions;
using MediatR;

namespace Api.Apis.TradingOrder;

public static class TradingOrderEndpoints
{
    public static RouteGroupBuilder MapTradingOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("TradingOrders");

        // GET /api/orders - Get paginated list of orders with filters
        group.MapGet("/", async (
            IMediator mediator,
            int page = 1,
            int limit = 10,
            Guid? connectorInstanceId = null,
            string? status = null,
            string? state = null,
            string? side = null,
            string? type = null,
            string? pair = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            CancellationToken ct = default) =>
        {
            var query = new GetOrdersQuery(
                page,
                limit,
                connectorInstanceId,
                status,
                state,
                side,
                type,
                pair,
                dateFrom,
                dateTo);

            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequirePermission("TradingOrders", "Read")
        .WithName("GetOrders")
        .WithDescription("Get paginated list of trading orders with optional filters")
        .Produces<PaginatedDataDto<TradingOrderDto>>(200)
        .Produces(401);

        // GET /api/orders/{id} - Get order by ID
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetOrderByIdQuery(id);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequirePermission("TradingOrders", "Read")
        .WithName("GetOrderById")
        .WithDescription("Get a trading order by ID")
        .Produces<TradingOrderDto>(200)
        .Produces(404);

        return group;
    }

    public static RouteGroupBuilder MapPnLMetricsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pnl-metrics")
            .WithTags("PnLMetrics");

        // GET /api/pnl-metrics - Get PnL metrics
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var query = new GetPnLMetricsQuery();
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        })
        .RequirePermission("TradingOrders", "Read")
        .WithName("GetPnLMetrics")
        .WithDescription("Get profit and loss metrics for the current user")
        .Produces<PnLMetricsDto>(200)
        .Produces(401);

        return group;
    }
}
