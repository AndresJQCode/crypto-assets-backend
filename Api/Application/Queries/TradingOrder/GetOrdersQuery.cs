using MediatR;
using Api.Application.Dtos.Common;
using Api.Application.Dtos.TradingOrder;

namespace Api.Application.Queries.TradingOrder;

/// <summary>
/// Query to get paginated trading orders with filters.
/// </summary>
public record GetOrdersQuery(
    int Page = 1,
    int Limit = 10,
    Guid? ConnectorInstanceId = null,
    string? Status = null, // "open" | "closed"
    string? State = null, // "Created" | "Submitted" | "PartiallyFilled" | "Filled" | "Cancelled" | "Rejected"
    string? Side = null, // "buy" | "sell"
    string? Type = null, // "market" | "limit" | "stop_loss" | "take_profit"
    string? Pair = null, // Symbol filter (e.g., "BTC", "BTCUSDT")
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<PaginatedDataDto<TradingOrderDto>>;
