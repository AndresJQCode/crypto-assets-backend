using MediatR;
using Api.Application.Dtos.Common;
using Api.Application.Dtos.TradingOrder;
using Domain.AggregatesModel.TradingOrderAggregate;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.TradingOrder;

public class GetOrdersQueryHandler(ApiContext context)
    : IRequestHandler<GetOrdersQuery, PaginatedDataDto<TradingOrderDto>>
{
    public async Task<PaginatedDataDto<TradingOrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Start with base query
        var query = context.TradingOrders.AsNoTracking().AsQueryable();

        // Apply filters
        if (request.ConnectorInstanceId.HasValue)
        {
            query = query.Where(o => o.ConnectorInstanceId == request.ConnectorInstanceId.Value);
        }

        // Status filter: "open" | "closed"
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (request.Status.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                // Open statuses: New, PartiallyFilled, Untriggered, Triggered
                query = query.Where(o =>
                    o.Status == OrderStatus.New ||
                    o.Status == OrderStatus.PartiallyFilled ||
                    o.Status == OrderStatus.Untriggered ||
                    o.Status == OrderStatus.Triggered);
            }
            else if (request.Status.Equals("closed", StringComparison.OrdinalIgnoreCase))
            {
                // Closed statuses: Filled, Cancelled, Rejected, Deactivated
                query = query.Where(o =>
                    o.Status == OrderStatus.Filled ||
                    o.Status == OrderStatus.Cancelled ||
                    o.Status == OrderStatus.Rejected ||
                    o.Status == OrderStatus.Deactivated);
            }
        }

        // State filter (specific OrderStatus)
        if (!string.IsNullOrEmpty(request.State))
        {
            var stateMapping = request.State switch
            {
                "Created" => OrderStatus.New,
                "Submitted" => OrderStatus.New, // Both map to New initially
                "PartiallyFilled" => OrderStatus.PartiallyFilled,
                "Filled" => OrderStatus.Filled,
                "Cancelled" => OrderStatus.Cancelled,
                "Rejected" => OrderStatus.Rejected,
                _ => (OrderStatus?)null
            };

            if (stateMapping.HasValue)
            {
                query = query.Where(o => o.Status == stateMapping.Value);
            }
        }

        // Side filter
        if (!string.IsNullOrEmpty(request.Side))
        {
            var sideMapping = request.Side.ToUpperInvariant() switch
            {
                "BUY" => OrderSide.Buy,
                "SELL" => OrderSide.Sell,
                _ => (OrderSide?)null
            };

            if (sideMapping.HasValue)
            {
                query = query.Where(o => o.Side == sideMapping.Value);
            }
        }

        // Type filter
        if (!string.IsNullOrEmpty(request.Type))
        {
            var typeMapping = request.Type.ToUpperInvariant() switch
            {
                "MARKET" => OrderType.Market,
                "LIMIT" => OrderType.Limit,
                "STOP_LOSS" => OrderType.StopLoss,
                "TAKE_PROFIT" => OrderType.TakeProfit,
                _ => (OrderType?)null
            };

            if (typeMapping.HasValue)
            {
                query = query.Where(o => o.OrderType == typeMapping.Value);
            }
        }

        // Pair filter (symbol)
        if (!string.IsNullOrEmpty(request.Pair))
        {
            var normalizedPair = request.Pair.Replace("/", "").Replace("-", "").ToUpperInvariant();
            query = query.Where(o => o.Symbol.Contains(normalizedPair));
        }

        // Date filters
        if (request.DateFrom.HasValue)
        {
            query = query.Where(o => o.CreatedTime >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(o => o.CreatedTime <= request.DateTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering (most recent first)
        query = query.OrderByDescending(o => o.CreatedTime);

        // Apply pagination
        var skip = (request.Page - 1) * request.Limit;
        var orders = await query
            .Skip(skip)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var orderDtos = orders.Select(MapToDto).ToList();

        return new PaginatedDataDto<TradingOrderDto>
        {
            Data = orderDtos,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.Limit),
            Page = request.Page,
            Limit = request.Limit
        };
    }

    private static TradingOrderDto MapToDto(Domain.AggregatesModel.TradingOrderAggregate.TradingOrder order)
    {
        // Parse trading pair from symbol
        var pair = TradingPairDto.Parse(order.Symbol);

        // Map OrderStatus to frontend State
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

        // Determine frontend status: "open" or "closed"
        var status = order.Status switch
        {
            OrderStatus.New or OrderStatus.PartiallyFilled or OrderStatus.Untriggered or OrderStatus.Triggered => "open",
            _ => "closed"
        };

        // Map OrderType to frontend format
        var type = order.OrderType switch
        {
            OrderType.Market => "market",
            OrderType.Limit => "limit",
            OrderType.StopLoss or OrderType.StopLossLimit => "stop_loss",
            OrderType.TakeProfit or OrderType.TakeProfitLimit => "take_profit",
            _ => "limit"
        };

        // Map OrderSide to frontend format
        var side = order.Side == OrderSide.Buy ? "buy" : "sell";

        // Calculate remaining quantity
        var remainingQuantity = order.Quantity - order.FilledQuantity;

        // Calculate total value
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
            CurrentPrice = null, // TODO: Fetch from market data service
            Pnl = null, // TODO: Calculate PnL
            PnlPercentage = null, // TODO: Calculate PnL%
            CreatedAt = order.CreatedTime,
            UpdatedAt = order.UpdatedTime ?? order.CreatedTime,
            SubmittedAt = order.CreatedTime, // Approximation
            CompletedAt = order.Status == OrderStatus.Filled ? order.UpdatedTime : null
        };
    }
}
