using Domain.AggregatesModel.OrderAggregate;

namespace Domain.Events;

// Base
public abstract record OrderDomainEvent(Guid OrderId, string ExternalOrderId, EcommercePlatform Platform);

// Eventos concretos (estructura sin implementación de handlers)
public record OrderCreatedEvent(
    Guid OrderId,
    string ExternalOrderId,
    EcommercePlatform Platform,
    Guid CustomerId,
    decimal Total,
    string Currency
) : OrderDomainEvent(OrderId, ExternalOrderId, Platform);

public record OrderUpdatedEvent(
    Guid OrderId,
    string ExternalOrderId,
    EcommercePlatform Platform
) : OrderDomainEvent(OrderId, ExternalOrderId, Platform);

public record OrderCancelledEvent(
    Guid OrderId,
    string ExternalOrderId,
    EcommercePlatform Platform,
    string CancellationReason
) : OrderDomainEvent(OrderId, ExternalOrderId, Platform);

public record OrderFulfilledEvent(
    Guid OrderId,
    string ExternalOrderId,
    EcommercePlatform Platform,
    DateTime FulfilledAt
) : OrderDomainEvent(OrderId, ExternalOrderId, Platform);

public record OrderPaidEvent(
    Guid OrderId,
    string ExternalOrderId,
    EcommercePlatform Platform,
    decimal Amount,
    DateTime PaidAt
) : OrderDomainEvent(OrderId, ExternalOrderId, Platform);

// TODO: Implementar handlers en Api/Application/EventHandlers/
// public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent> { }
