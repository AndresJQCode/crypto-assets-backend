using MediatR;
using Api.Application.Dtos.TradingOrder;

namespace Api.Application.Queries.TradingOrder;

/// <summary>
/// Query to get a trading order by ID.
/// </summary>
public record GetOrderByIdQuery(Guid OrderId) : IRequest<TradingOrderDto>;
