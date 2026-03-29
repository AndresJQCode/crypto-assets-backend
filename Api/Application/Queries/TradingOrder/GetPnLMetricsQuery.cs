using MediatR;
using Api.Application.Dtos.TradingOrder;

namespace Api.Application.Queries.TradingOrder;

/// <summary>
/// Query to get PnL (Profit and Loss) metrics for the current user.
/// </summary>
public record GetPnLMetricsQuery : IRequest<PnLMetricsDto>;
