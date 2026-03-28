namespace Domain.Interfaces;

/// <summary>
/// Service interface for Bybit exchange API operations.
/// Implementation in Infrastructure layer using bybit.net.api SDK.
/// </summary>
public interface IBybitService
{
    /// <summary>
    /// Validates Bybit API credentials by making a test request to the exchange.
    /// Used during connector setup to ensure credentials are valid.
    /// </summary>
    /// <param name="apiKey">Bybit API key.</param>
    /// <param name="apiSecret">Bybit API secret.</param>
    /// <param name="isTestnet">True for testnet environment, false for production.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if credentials are valid and API is reachable.</returns>
    Task<bool> ValidateCredentialsAsync(string apiKey, string apiSecret, bool isTestnet, CancellationToken ct = default);

    /// <summary>
    /// Gets open orders (active + recently filled/cancelled) from Bybit.
    /// Bybit V5 API endpoint: GET /v5/order/realtime
    /// Category: Linear (USDT perpetuals)
    /// </summary>
    /// <param name="apiKey">Bybit API key.</param>
    /// <param name="apiSecret">Bybit API secret.</param>
    /// <param name="isTestnet">True for testnet environment.</param>
    /// <param name="symbol">Optional symbol filter (e.g., BTCUSDT). Null for all symbols.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of orders from Bybit API.</returns>
    Task<List<BybitOrderDto>> GetOpenOrdersAsync(
        string apiKey,
        string apiSecret,
        bool isTestnet,
        string? symbol = null,
        CancellationToken ct = default);

    /// <summary>
    /// Gets order history with pagination from Bybit.
    /// Bybit V5 API endpoint: GET /v5/order/history
    /// Note: Bybit stores up to 2 years of data, max 7-day window per request.
    /// </summary>
    /// <param name="apiKey">Bybit API key.</param>
    /// <param name="apiSecret">Bybit API secret.</param>
    /// <param name="isTestnet">True for testnet environment.</param>
    /// <param name="startTime">Start date for history query.</param>
    /// <param name="endTime">End date for history query.</param>
    /// <param name="symbol">Optional symbol filter.</param>
    /// <param name="limit">Number of records to return (1-50).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of historical orders.</returns>
    Task<List<BybitOrderDto>> GetOrderHistoryAsync(
        string apiKey,
        string apiSecret,
        bool isTestnet,
        DateTime startTime,
        DateTime endTime,
        string? symbol = null,
        int limit = 50,
        CancellationToken ct = default);
}

/// <summary>
/// Data transfer object for Bybit orders.
/// Maps from Bybit SDK response to domain-agnostic DTO.
/// </summary>
public record BybitOrderDto(
    string OrderId,
    string Symbol,
    string Side,
    string OrderType,
    string Status,
    decimal Qty,
    decimal Price,
    decimal CumExecQty,
    decimal? AvgPrice,
    decimal? Fee,
    string? FeeCurrency,
    DateTime CreatedTime,
    DateTime? UpdatedTime,
    decimal? StopPrice = null,
    decimal? TriggerPrice = null);
