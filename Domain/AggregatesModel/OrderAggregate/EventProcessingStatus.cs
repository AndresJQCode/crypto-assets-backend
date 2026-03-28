namespace Domain.AggregatesModel.OrderAggregate;

public enum EventProcessingStatus
{
    None = 0,
    Pending = 1,
    Processed = 2,
    Failed = 3,
    Skipped = 4
}
