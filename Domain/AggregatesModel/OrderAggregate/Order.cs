using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.OrderAggregate;

public class Order : Entity<Guid>, IAggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public string ExternalOrderId { get; private set; } = default!;
    public EcommercePlatform Platform { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public FulfillmentStatus FulfillmentStatus { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = default!;

    public DateTime OrderDate { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? FulfilledAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public DateTime LastSyncedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public ShippingAddress ShippingAddress { get; private set; } = default!;
    public BillingAddress BillingAddress { get; private set; } = default!;

    public string? PlatformMetadata { get; private set; }
    public bool IsDeleted { get; private set; }

    private Order() { }

    public static Order CreateFromEcommerce(
        Guid tenantId,
        Guid customerId,
        string orderNumber,
        string externalOrderId,
        EcommercePlatform platform,
        decimal total,
        string currency,
        DateTime orderDate,
        ShippingAddress shippingAddress,
        BillingAddress billingAddress)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CustomerId = customerId,
            OrderNumber = orderNumber,
            ExternalOrderId = externalOrderId,
            Platform = platform,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            FulfillmentStatus = FulfillmentStatus.Unfulfilled,
            Total = total,
            Currency = currency,
            OrderDate = orderDate,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            LastSyncedAt = DateTime.UtcNow
        };

        // Domain Event (estructura, sin implementación)
        // order.AddDomainEvent(new OrderCreatedEvent(order.Id, order.ExternalOrderId, order.Platform));

        return order;
    }

    public void UpdateFromEcommerce(
        OrderStatus? status,
        PaymentStatus? paymentStatus,
        FulfillmentStatus? fulfillmentStatus,
        decimal? subtotal,
        decimal? tax,
        decimal? shippingCost,
        decimal? discount,
        decimal? total,
        DateTime eventTimestamp,
        string? platformMetadata = null)
    {
        if (eventTimestamp < LastSyncedAt)
        {
            throw new DomainException(
                $"Event timestamp {eventTimestamp} is older than last sync {LastSyncedAt}. Ignoring update.");
        }

        if (status.HasValue && status != Status)
        {
            Status = status.Value;
        }

        if (paymentStatus.HasValue && paymentStatus != PaymentStatus)
        {
            PaymentStatus = paymentStatus.Value;
            if (paymentStatus == PaymentStatus.Paid && !PaidAt.HasValue)
            {
                PaidAt = eventTimestamp;
                // Domain Event: OrderPaidEvent
            }
        }

        if (fulfillmentStatus.HasValue && fulfillmentStatus != FulfillmentStatus)
        {
            FulfillmentStatus = fulfillmentStatus.Value;
            if (fulfillmentStatus == FulfillmentStatus.Fulfilled && !FulfilledAt.HasValue)
            {
                FulfilledAt = eventTimestamp;
                // Domain Event: OrderFulfilledEvent
            }
        }

        if (subtotal.HasValue) Subtotal = subtotal.Value;
        if (tax.HasValue) Tax = tax.Value;
        if (shippingCost.HasValue) ShippingCost = shippingCost.Value;
        if (discount.HasValue) Discount = discount.Value;
        if (total.HasValue) Total = total.Value;

        if (platformMetadata != null)
        {
            PlatformMetadata = platformMetadata;
        }

        LastSyncedAt = eventTimestamp;

        // Domain Event: OrderUpdatedEvent
    }

    public void Cancel(string reason, DateTime cancelledAt)
    {
        if (Status == OrderStatus.Cancelled)
        {
            throw new DomainException("Order is already cancelled");
        }

        if (FulfillmentStatus == FulfillmentStatus.Fulfilled)
        {
            throw new DomainException("Cannot cancel a fulfilled order");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = cancelledAt;
        LastSyncedAt = DateTime.UtcNow;

        // Domain Event: OrderCancelledEvent
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Sum(i => i.Subtotal);
        Total = Subtotal + Tax + ShippingCost - Discount;
    }
}
