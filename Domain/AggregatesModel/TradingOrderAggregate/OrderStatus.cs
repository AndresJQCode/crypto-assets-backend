namespace Domain.AggregatesModel.TradingOrderAggregate;

/// <summary>
/// Trading order status lifecycle.
/// </summary>
public enum OrderStatus
{
    None = 0,
    New = 1,
    PartiallyFilled = 2,
    Filled = 3,
    Cancelled = 4,
    Rejected = 5,
    Untriggered = 6,  // Conditional order not yet triggered
    Triggered = 7,    // Conditional order has been triggered
    Deactivated = 8   // Order has been deactivated
}
