using Domain.AggregatesModel.TradingOrderAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class TradingOrderRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<TradingOrderRepository> logger)
    : Repository<TradingOrder>(context, httpContextAccessor, logger), ITradingOrderRepository
{
    private readonly ApiContext _context = context;

    public async Task<List<TradingOrder>> GetOpenOrdersByConnectorAsync(Guid connectorInstanceId, CancellationToken ct = default)
    {
        return await _context.TradingOrders
            .AsNoTracking()
            .Where(o => o.ConnectorInstanceId == connectorInstanceId
                && (o.Status == OrderStatus.New
                    || o.Status == OrderStatus.PartiallyFilled
                    || o.Status == OrderStatus.Untriggered))
            .OrderByDescending(o => o.CreatedTime)
            .ToListAsync(ct);
    }

    public async Task<List<TradingOrder>> GetOrderHistoryAsync(
        Guid connectorInstanceId,
        DateTime? startDate,
        DateTime? endDate,
        int pageSize,
        int page,
        CancellationToken ct = default)
    {
        var query = _context.TradingOrders
            .AsNoTracking()
            .Where(o => o.ConnectorInstanceId == connectorInstanceId);

        if (startDate.HasValue)
            query = query.Where(o => o.CreatedTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(o => o.CreatedTime <= endDate.Value);

        return await query
            .OrderByDescending(o => o.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<TradingOrder?> GetByExternalOrderIdAsync(
        string externalOrderId,
        Guid connectorInstanceId,
        CancellationToken ct = default)
    {
        return await _context.TradingOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ExternalOrderId == externalOrderId
                && o.ConnectorInstanceId == connectorInstanceId, ct);
    }
}
