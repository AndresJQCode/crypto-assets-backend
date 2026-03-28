using System.Diagnostics;
using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Metrics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Trading;

/// <summary>
/// Bybit exchange API integration service using Bybit.Net SDK v6.4.0.
/// Provides methods to interact with Bybit V5 API for linear perpetual trading.
/// </summary>
public class BybitService(ILogger<BybitService> logger) : IBybitService
{
    private const string OpenOrdersEndpoint = "get_open_orders";
    private const string OrderHistoryEndpoint = "get_order_history";
    private const string ValidateCredentialsEndpoint = "validate_credentials";

    /// <summary>
    /// Validates Bybit API credentials by attempting to retrieve account balances.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string apiKey, string apiSecret, bool isTestnet, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Validating Bybit API credentials. Testnet: {IsTestnet}", isTestnet);

            using var client = CreateClient(apiKey, apiSecret, isTestnet);

            var result = await client.V5Api.Account.GetBalancesAsync(AccountType.Unified, ct: ct);

            if (result.Success)
            {
                logger.LogInformation("Bybit API credentials validated successfully");
                InfrastructureMetrics.BybitApiCallsTotal.WithLabels(ValidateCredentialsEndpoint, "success").Inc();
                InfrastructureMetrics.BybitApiCallDuration.WithLabels(ValidateCredentialsEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
                return true;
            }

            logger.LogWarning("Bybit API credential validation failed: {Error}", result.Error?.Message);
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(ValidateCredentialsEndpoint, "failed").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(ValidateCredentialsEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating Bybit API credentials");
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(ValidateCredentialsEndpoint, "error").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(ValidateCredentialsEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            return false;
        }
    }

    /// <summary>
    /// Retrieves open orders (New, PartiallyFilled, Untriggered) for linear perpetual contracts.
    /// </summary>
    public async Task<List<BybitOrderDto>> GetOpenOrdersAsync(string apiKey, string apiSecret, bool isTestnet, string? symbol = null, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Fetching open orders from Bybit. Symbol: {Symbol}, Testnet: {IsTestnet}",
                symbol ?? "all", isTestnet);

            using var client = CreateClient(apiKey, apiSecret, isTestnet);

            var result = await client.V5Api.Trading.GetOrdersAsync(
                category: Category.Linear,
                symbol: symbol,
                openOnly: 0, // 0 = open orders only
                limit: 50,
                ct: ct);

            if (!result.Success)
            {
                var errorMessage = $"Bybit API error: {result.Error?.Message ?? "Unknown error"}";
                logger.LogError("Failed to fetch open orders from Bybit: {Error}", errorMessage);
                InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OpenOrdersEndpoint, "failed").Inc();
                InfrastructureMetrics.BybitApiCallDuration.WithLabels(OpenOrdersEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);

                // Handle rate limiting
                if (result.Error?.Code == 10006) // Rate limit error code
                {
                    InfrastructureMetrics.BybitRateLimitHitsTotal.Inc();
                    throw new DomainException("Se ha excedido el límite de solicitudes a Bybit. Intente nuevamente en unos segundos.");
                }

                throw new DomainException($"Error al obtener órdenes de Bybit: {errorMessage}");
            }

            var orders = result.Data.List
                .Select(MapToBybitOrderDto)
                .ToList();

            logger.LogInformation("Successfully fetched {Count} open orders from Bybit", orders.Count);
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OpenOrdersEndpoint, "success").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OpenOrdersEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);

            return orders;
        }
        catch (DomainException)
        {
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OpenOrdersEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            throw; // Re-throw domain exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching open orders from Bybit");
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OpenOrdersEndpoint, "error").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OpenOrdersEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            throw new DomainException("Error inesperado al comunicarse con Bybit. Verifique sus credenciales e intente nuevamente.", ex);
        }
    }

    /// <summary>
    /// Retrieves order history for linear perpetual contracts within a date range.
    /// </summary>
    public async Task<List<BybitOrderDto>> GetOrderHistoryAsync(
        string apiKey,
        string apiSecret,
        bool isTestnet,
        DateTime startTime,
        DateTime endTime,
        string? symbol = null,
        int limit = 50,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation(
                "Fetching order history from Bybit. Symbol: {Symbol}, StartTime: {StartTime}, EndTime: {EndTime}, Testnet: {IsTestnet}",
                symbol ?? "all", startTime, endTime, isTestnet);

            using var client = CreateClient(apiKey, apiSecret, isTestnet);

            var result = await client.V5Api.Trading.GetOrderHistoryAsync(
                category: Category.Linear,
                symbol: symbol,
                startTime: startTime,
                endTime: endTime,
                limit: Math.Min(limit, 50), // Bybit max is 50
                ct: ct);

            if (!result.Success)
            {
                var errorMessage = $"Bybit API error: {result.Error?.Message ?? "Unknown error"}";
                logger.LogError("Failed to fetch order history from Bybit: {Error}", errorMessage);
                InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OrderHistoryEndpoint, "failed").Inc();
                InfrastructureMetrics.BybitApiCallDuration.WithLabels(OrderHistoryEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);

                if (result.Error?.Code == 10006) // Rate limit
                {
                    InfrastructureMetrics.BybitRateLimitHitsTotal.Inc();
                    throw new DomainException("Se ha excedido el límite de solicitudes a Bybit. Intente nuevamente en unos segundos.");
                }

                throw new DomainException($"Error al obtener historial de órdenes de Bybit: {errorMessage}");
            }

            var orders = result.Data.List
                .Select(MapToBybitOrderDto)
                .ToList();

            logger.LogInformation("Successfully fetched {Count} historical orders from Bybit", orders.Count);
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OrderHistoryEndpoint, "success").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OrderHistoryEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);

            return orders;
        }
        catch (DomainException)
        {
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OrderHistoryEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching order history from Bybit");
            InfrastructureMetrics.BybitApiCallsTotal.WithLabels(OrderHistoryEndpoint, "error").Inc();
            InfrastructureMetrics.BybitApiCallDuration.WithLabels(OrderHistoryEndpoint).Observe(stopwatch.Elapsed.TotalSeconds);
            throw new DomainException("Error inesperado al obtener historial de Bybit.", ex);
        }
    }

    /// <summary>
    /// Creates a configured Bybit REST client with API credentials and environment setting.
    /// </summary>
    private static BybitRestClient CreateClient(string apiKey, string apiSecret, bool isTestnet)
    {
        return new BybitRestClient(options =>
        {
            options.ApiCredentials = new ApiCredentials(apiKey, apiSecret);
            options.Environment = isTestnet ? BybitEnvironment.Testnet : BybitEnvironment.Live;
        });
    }

    /// <summary>
    /// Maps Bybit SDK order object to domain DTO.
    /// </summary>
    private static BybitOrderDto MapToBybitOrderDto(Bybit.Net.Objects.Models.V5.BybitOrder order)
    {
        return new BybitOrderDto(
            OrderId: order.OrderId,
            Symbol: order.Symbol,
            Side: order.Side.ToString(),
            OrderType: order.OrderType.ToString(),
            Status: MapOrderStatus(order.Status),
            Qty: order.Quantity,
            Price: order.Price ?? 0m,
            CumExecQty: order.QuantityFilled ?? 0m,
            AvgPrice: order.AveragePrice,
            Fee: order.ExecutedFee ?? 0m,
            FeeCurrency: !string.IsNullOrEmpty(order.FeeAsset) ? order.FeeAsset : null,
            CreatedTime: order.CreateTime,
            UpdatedTime: order.UpdateTime,
            StopPrice: order.StopLoss,
            TriggerPrice: order.TriggerPrice
        );
    }

    /// <summary>
    /// Maps Bybit SDK order status to standardized string format.
    /// </summary>
    private static string MapOrderStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.New => "New",
            OrderStatus.PartiallyFilled => "PartiallyFilled",
            OrderStatus.Filled => "Filled",
            OrderStatus.Cancelled => "Cancelled",
            OrderStatus.Rejected => "Rejected",
            OrderStatus.Untriggered => "Untriggered",
            OrderStatus.Triggered => "Triggered",
            OrderStatus.Deactivated => "Deactivated",
            _ => status.ToString()
        };
    }
}
