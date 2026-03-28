using Domain.AggregatesModel.OrderAggregate;

namespace Api.Application.Services.Ecommerce;

public interface IOrderMapper
{
    Order MapToOrder(string eventPayload, Guid tenantId, Guid customerId, DateTime eventTimestamp);
    void UpdateOrder(Order order, string eventPayload, DateTime eventTimestamp);
}
