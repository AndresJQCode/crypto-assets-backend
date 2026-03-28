using Domain.SeedWork;

namespace Domain.AggregatesModel.TradingOrderAggregate;

/// <summary>
/// Repository interface for TradingOrder aggregate.
/// Extends IRepository with custom queries for trading operations.
/// </summary>
public interface ITradingOrderRepository : IRepository<TradingOrder>
{
    /// <summary>
    /// Gets all open orders for a specific connector instance.
    /// Open orders include: New, PartiallyFilled, Untriggered statuses.
    /// </summary>
    /// <param name="connectorInstanceId">Connector instance ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of open trading orders.</returns>
    Task<List<TradingOrder>> GetOpenOrdersByConnectorAsync(Guid connectorInstanceId, CancellationToken ct = default);

    /// <summary>
    /// Gets order history for a connector with optional date range and pagination.
    /// </summary>
    /// <param name="connectorInstanceId">Connector instance ID.</param>
    /// <param name="startDate">Start date (optional).</param>
    /// <param name="endDate">End date (optional).</param>
    /// <param name="pageSize">Number of records per page.</param>
    /// <param name="page">Page number (1-indexed).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of trading orders.</returns>
    Task<List<TradingOrder>> GetOrderHistoryAsync(
        Guid connectorInstanceId,
        DateTime? startDate,
        DateTime? endDate,
        int pageSize,
        int page,
        CancellationToken ct = default);

    /// <summary>
    /// Finds a trading order by its external order ID (from exchange).
    /// </summary>
    /// <param name="externalOrderId">External order ID from exchange (e.g., Bybit order ID).</param>
    /// <param name="connectorInstanceId">Connector instance ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Trading order if found, null otherwise.</returns>
    Task<TradingOrder?> GetByExternalOrderIdAsync(
        string externalOrderId,
        Guid connectorInstanceId,
        CancellationToken ct = default);
}
