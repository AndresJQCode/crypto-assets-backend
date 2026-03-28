using Domain.SeedWork;

namespace Domain.AggregatesModel.OrderAggregate;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByExternalIdAsync(EcommercePlatform platform, string externalOrderId, CancellationToken ct);
    Task<List<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);
}
