using Domain.AggregatesModel.OrderAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly ApiContext _context;

    public OrderRepository(
        ApiContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrderRepository> logger)
        : base(context, httpContextAccessor, logger)
    {
        _context = context;
    }

    public async Task<Order?> GetByExternalIdAsync(
        EcommercePlatform platform,
        string externalOrderId,
        CancellationToken ct)
    {
        // NOTE: TenantId filtering should be handled by global query filter or TenantContext
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(
                o => o.Platform == platform && o.ExternalOrderId == externalOrderId,
                ct);
    }

    public async Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct)
    {
        // NOTE: TenantId filtering should be handled by global query filter or TenantContext
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }
}
