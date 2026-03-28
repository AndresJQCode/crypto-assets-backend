namespace Domain.AggregatesModel.OrderAggregate;

public enum FulfillmentStatus
{
    None = 0,
    Unfulfilled = 1,
    PartiallyFulfilled = 2,
    Fulfilled = 3,
    Restocked = 4
}
