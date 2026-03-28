namespace Api.Application.Dtos.ConnectorInstance;

/// <summary>
/// Request DTO for synchronizing Bybit trading history
/// </summary>
public record SyncBybitHistoryRequest
{
    /// <summary>
    /// Connector instance ID (Bybit connector)
    /// </summary>
    public required Guid ConnectorInstanceId { get; init; }

    /// <summary>
    /// Start date for sync (optional, defaults to 2 years ago)
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for sync (optional, defaults to now)
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// Trading pair to filter (optional, e.g., "BTCUSDT")
    /// </summary>
    public string? Symbol { get; init; }
}
