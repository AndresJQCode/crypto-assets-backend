using System.Security.Claims;
using MediatR;
using Api.Application.Dtos.TradingOrder;
using Domain.AggregatesModel.TradingOrderAggregate;
using Domain.Exceptions;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Queries.TradingOrder;

public class GetPnLMetricsQueryHandler(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetPnLMetricsQuery, PnLMetricsDto>
{
    public async Task<PnLMetricsDto> Handle(GetPnLMetricsQuery request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnAuthorizedException();

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnAuthorizedException();

        // Get all user's orders
        var orders = await context.TradingOrders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);

        // Separate open and closed orders
        var closedOrders = orders.Where(o =>
            o.Status == OrderStatus.Filled ||
            o.Status == OrderStatus.Cancelled ||
            o.Status == OrderStatus.Rejected ||
            o.Status == OrderStatus.Deactivated).ToList();

        var filledOrders = closedOrders.Where(o => o.Status == OrderStatus.Filled).ToList();

        var openOrders = orders.Where(o =>
            o.Status == OrderStatus.New ||
            o.Status == OrderStatus.PartiallyFilled ||
            o.Status == OrderStatus.Untriggered ||
            o.Status == OrderStatus.Triggered).ToList();

        // Calculate realized PnL (from filled orders)
        // For simplicity, we'll use fee as a proxy for PnL calculation
        // In a real system, you'd track buy/sell pairs and calculate actual PnL
        decimal realizedPnL = 0;
        int profitableTradesCount = 0;

        foreach (var order in filledOrders)
        {
            // Simplified PnL calculation
            // In reality, you need to match buy/sell pairs and calculate profit/loss
            // For now, we'll assume fees represent costs
            if (order.Fee.HasValue)
            {
                realizedPnL -= order.Fee.Value; // Fees are costs
            }

            // Count as profitable if average price is favorable
            // This is a placeholder - real implementation needs buy/sell matching
            if (order.AveragePrice.HasValue && order.Price > 0)
            {
                var priceDeviation = order.AveragePrice.Value - order.Price;
                if ((order.Side == OrderSide.Buy && priceDeviation < 0) ||
                    (order.Side == OrderSide.Sell && priceDeviation > 0))
                {
                    profitableTradesCount++;
                }
            }
        }

        // Calculate unrealized PnL (from open orders)
        // This would require current market prices - placeholder for now
        decimal unrealizedPnL = 0;

        // Calculate win rate
        decimal winRate = filledOrders.Count > 0
            ? (decimal)profitableTradesCount / filledOrders.Count * 100
            : 0;

        // Total PnL
        decimal totalPnL = realizedPnL + unrealizedPnL;

        return new PnLMetricsDto
        {
            TotalPnL = totalPnL,
            RealizedPnL = realizedPnL,
            UnrealizedPnL = unrealizedPnL,
            WinRate = winRate,
            TotalTrades = closedOrders.Count
        };
    }
}
