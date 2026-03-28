using Api.Application.Dtos.Trading;
using MediatR;

namespace Api.Application.Queries.BybitQueries;

internal sealed class GetOpenOrdersQuery : IRequest<List<TradingOrderDto>>
{
    public string? Symbol { get; set; }
}
