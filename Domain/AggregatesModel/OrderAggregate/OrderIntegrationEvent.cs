using Domain.SeedWork;

namespace Domain.AggregatesModel.OrderAggregate;

public class OrderIntegrationEvent : Entity<Guid>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public Guid? OrderId { get; private set; }
    public string ExternalOrderId { get; private set; } = default!;
    public EcommercePlatform Platform { get; private set; }
    public string EventType { get; private set; } = default!;
    public string EventPayload { get; private set; } = default!;
    public DateTime EventTimestamp { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public EventProcessingStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsDeleted { get; private set; }

    private OrderIntegrationEvent() { }

    public static OrderIntegrationEvent Create(
        Guid tenantId,
        string idempotencyKey,
        string externalOrderId,
        EcommercePlatform platform,
        string eventType,
        string eventPayload,
        DateTime eventTimestamp)
    {
        return new OrderIntegrationEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IdempotencyKey = idempotencyKey,
            ExternalOrderId = externalOrderId,
            Platform = platform,
            EventType = eventType,
            EventPayload = eventPayload,
            EventTimestamp = eventTimestamp,
            ReceivedAt = DateTime.UtcNow,
            Status = EventProcessingStatus.Pending,
            RetryCount = 0
        };
    }

    public void MarkAsProcessed(Guid orderId)
    {
        OrderId = orderId;
        Status = EventProcessingStatus.Processed;
    }

    public void MarkAsFailed(string error)
    {
        Status = EventProcessingStatus.Failed;
        ErrorMessage = error;
        RetryCount++;
    }

    public void MarkAsSkipped(string error)
    {
        Status = EventProcessingStatus.Skipped;
        ErrorMessage = error;
    }
}
