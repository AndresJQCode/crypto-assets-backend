using MediatR;
using Api.Application.Dtos.TradingOrder;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.Exceptions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.TradingOrder;

public class GetOrderByIdQueryHandler(ApiContext context)
    : IRequestHandler<GetOrderByIdQuery, TradingOrderDto>
{
    public async Task<TradingOrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await context.TradingOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Trading order {request.OrderId} not found");

        return MapToDto(order);
    }

    private static TradingOrderDto MapToDto(Domain.AggregatesModel.TradingOrderAggregate.TradingOrder order)
    {
        var pair = TradingPairDto.Parse(order.Symbol);

        var state = order.Status switch
        {
            OrderStatus.New => "Created",
            OrderStatus.PartiallyFilled => "PartiallyFilled",
            OrderStatus.Filled => "Filled",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Rejected => "Rejected",
            OrderStatus.Untriggered => "Created",
            OrderStatus.Triggered => "Submitted",
            OrderStatus.Deactivated => "Cancelled",
            _ => "Created"
        };

        var status = order.Status switch
        {
            OrderStatus.New or OrderStatus.PartiallyFilled or OrderStatus.Untriggered or OrderStatus.Triggered => "open",
            _ => "closed"
        };

        var type = order.OrderType switch
        {
            OrderType.Market => "market",
            OrderType.Limit => "limit",
            OrderType.StopLoss or OrderType.StopLossLimit => "stop_loss",
            OrderType.TakeProfit or OrderType.TakeProfitLimit => "take_profit",
            _ => "limit"
        };

        var side = order.Side == OrderSide.Buy ? "buy" : "sell";
        var remainingQuantity = order.Quantity - order.FilledQuantity;

        decimal? totalValue = null;
        if (order.AveragePrice.HasValue && order.FilledQuantity > 0)
        {
            totalValue = order.AveragePrice.Value * order.FilledQuantity;
        }
        else if (order.Price > 0 && order.Quantity > 0)
        {
            totalValue = order.Price * order.Quantity;
        }

        return new TradingOrderDto
        {
            Id = order.Id,
            ExchangeId = order.ConnectorInstanceId,
            ExchangeOrderId = order.ExternalOrderId,
            Pair = pair,
            Type = type,
            Side = side,
            State = state,
            Status = status,
            Quantity = order.Quantity,
            FilledQuantity = order.FilledQuantity,
            RemainingQuantity = remainingQuantity,
            Price = order.Price > 0 ? order.Price : null,
            AverageFilledPrice = order.AveragePrice,
            TotalValue = totalValue,
            CurrentPrice = null,
            Pnl = null,
            PnlPercentage = null,
            CreatedAt = order.CreatedTime,
            UpdatedAt = order.UpdatedTime ?? order.CreatedTime,
            SubmittedAt = order.CreatedTime,
            CompletedAt = order.Status == OrderStatus.Filled ? order.UpdatedTime : null
        };
    }
}
