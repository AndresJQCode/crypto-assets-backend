namespace Domain.AggregatesModel.OrderAggregate;

public enum OrderStatus
{
    None = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4,
    Refunded = 5
}
