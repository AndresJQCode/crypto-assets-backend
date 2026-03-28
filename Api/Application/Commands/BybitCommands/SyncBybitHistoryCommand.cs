namespace Api.Application.Commands.BybitCommands;

/// <summary>
/// Command to synchronize full trading history from Bybit
/// </summary>
public record SyncBybitHistoryCommand : IRequest<SyncBybitHistoryResult>
{
    /// <summary>
    /// Connector instance ID that contains Bybit credentials
    /// </summary>
    public required Guid ConnectorInstanceId { get; init; }

    /// <summary>
    /// Start date for historical sync (if null, syncs all available history)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for historical sync (if null, uses current date)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Trading pair to sync (if null, syncs all pairs)
    /// </summary>
    public string? Symbol { get; init; }
}

/// <summary>
/// Result of the Bybit history synchronization
/// </summary>
public record SyncBybitHistoryResult
{
    public int TotalOrdersFetched { get; init; }
    public int NewOrdersStored { get; init; }
    public int UpdatedOrders { get; init; }
    public int TotalApiCalls { get; init; }
    public DateTime SyncStartTime { get; init; }
    public DateTime SyncEndTime { get; init; }
    public List<string> Errors { get; init; } = [];
}
