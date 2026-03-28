namespace Domain.AggregatesModel.OrderAggregate;

public enum PaymentStatus
{
    None = 0,
    Pending = 1,
    Authorized = 2,
    Paid = 3,
    PartiallyRefunded = 4,
    Refunded = 5,
    Voided = 6
}
