using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.TradingOrderAggregate;

/// <summary>
/// Trading order aggregate root.
/// Represents a buy/sell order on a crypto exchange (e.g., Bybit).
/// Separate from e-commerce OrderAggregate - different domain rules.
/// </summary>
public class TradingOrder : Entity<Guid>, IAggregateRoot
{
    public Guid ConnectorInstanceId { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>
    /// External order ID from the exchange (e.g., Bybit order ID).
    /// </summary>
    public string ExternalOrderId { get; private set; } = default!;

    /// <summary>
    /// Trading pair symbol (e.g., BTCUSDT, ETHUSDT).
    /// </summary>
    public string Symbol { get; private set; } = default!;

    public OrderSide Side { get; private set; }
    public OrderType OrderType { get; private set; }
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Order quantity (crypto amount, high precision required).
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Order price. For market orders, this is the execution price.
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Stop price for stop-loss/take-profit orders.
    /// </summary>
    public decimal? StopPrice { get; private set; }

    /// <summary>
    /// Trigger price for conditional orders.
    /// </summary>
    public decimal? TriggerPrice { get; private set; }

    /// <summary>
    /// Quantity that has been filled/executed.
    /// </summary>
    public decimal FilledQuantity { get; private set; }

    /// <summary>
    /// Average execution price for filled quantity.
    /// </summary>
    public decimal? AveragePrice { get; private set; }

    /// <summary>
    /// Total trading fee paid.
    /// </summary>
    public decimal? Fee { get; private set; }

    /// <summary>
    /// Currency of the fee (e.g., USDT, BTC).
    /// </summary>
    public string? FeeCurrency { get; private set; }

    /// <summary>
    /// Timestamp when the order was created on the exchange.
    /// </summary>
    public DateTime CreatedTime { get; private set; }

    /// <summary>
    /// Timestamp when the order was last updated on the exchange.
    /// </summary>
    public DateTime? UpdatedTime { get; private set; }

    /// <summary>
    /// Last time this order was synchronized from the exchange API.
    /// </summary>
    public DateTime LastSyncedAt { get; private set; }

    /// <summary>
    /// Raw JSON response from exchange API (for debugging/auditing).
    /// </summary>
    public string? RawData { get; private set; }

    // EF Core constructor
    private TradingOrder() { }

    /// <summary>
    /// Factory method to create a TradingOrder from Bybit API response.
    /// </summary>
    public static TradingOrder CreateFromBybit(
        Guid connectorInstanceId,
        Guid userId,
        string externalOrderId,
        string symbol,
        string side,
        string orderType,
        string status,
        decimal quantity,
        decimal price,
        decimal filledQuantity,
        decimal? averagePrice,
        decimal? fee,
        string? feeCurrency,
        DateTime createdTime,
        DateTime? updatedTime,
        decimal? stopPrice = null,
        decimal? triggerPrice = null,
        string? rawData = null)
    {
        if (string.IsNullOrWhiteSpace(externalOrderId))
            throw new DomainException("External order ID is required");

        if (string.IsNullOrWhiteSpace(symbol))
            throw new DomainException("Symbol is required");

        var order = new TradingOrder
        {
            Id = Guid.CreateVersion7(),
            ConnectorInstanceId = connectorInstanceId,
            UserId = userId,
            ExternalOrderId = externalOrderId,
            Symbol = symbol.ToUpperInvariant(),
            Side = ParseOrderSide(side),
            OrderType = ParseOrderType(orderType),
            Status = ParseOrderStatus(status),
            Quantity = quantity,
            Price = price,
            StopPrice = stopPrice,
            TriggerPrice = triggerPrice,
            FilledQuantity = filledQuantity,
            AveragePrice = averagePrice,
            Fee = fee,
            FeeCurrency = feeCurrency,
            CreatedTime = createdTime,
            UpdatedTime = updatedTime,
            LastSyncedAt = DateTime.UtcNow,
            RawData = rawData,
            CreatedOn = DateTimeOffset.UtcNow
        };

        return order;
    }

    /// <summary>
    /// Updates the order with fresh data from Bybit API.
    /// </summary>
    public void UpdateFromBybit(
        string status,
        decimal filledQuantity,
        decimal? averagePrice,
        decimal? fee,
        string? feeCurrency,
        DateTime? updatedTime,
        string? rawData = null)
    {
        Status = ParseOrderStatus(status);
        FilledQuantity = filledQuantity;
        AveragePrice = averagePrice;
        Fee = fee;
        FeeCurrency = feeCurrency;
        UpdatedTime = updatedTime;
        LastSyncedAt = DateTime.UtcNow;
        RawData = rawData;
        LastModifiedOn = DateTimeOffset.UtcNow;

        // Domain event (infrastructure for future use)
        // this.AddDomainEvent(new TradingOrderUpdatedEvent(this.Id, this.Status));
    }

    /// <summary>
    /// Checks if the order is fully filled (executed).
    /// </summary>
    public bool IsFullyFilled() => FilledQuantity >= Quantity;

    /// <summary>
    /// Checks if the order is still active (open, not filled/cancelled).
    /// </summary>
    public bool IsActive() => Status is OrderStatus.New or OrderStatus.PartiallyFilled or OrderStatus.Untriggered;

    // Private parsing helpers

    private static OrderSide ParseOrderSide(string side)
    {
        return side?.ToUpperInvariant() switch
        {
            "BUY" => OrderSide.Buy,
            "SELL" => OrderSide.Sell,
            _ => throw new DomainException($"Unknown order side: {side}")
        };
    }

    private static OrderType ParseOrderType(string type)
    {
        return type?.ToUpperInvariant() switch
        {
            "MARKET" => OrderType.Market,
            "LIMIT" => OrderType.Limit,
            "STOPLOSS" or "STOP_LOSS" => OrderType.StopLoss,
            "TAKEPROFIT" or "TAKE_PROFIT" => OrderType.TakeProfit,
            "STOPLOSSLIMIT" or "STOP_LOSS_LIMIT" => OrderType.StopLossLimit,
            "TAKEPROFITLIMIT" or "TAKE_PROFIT_LIMIT" => OrderType.TakeProfitLimit,
            _ => throw new DomainException($"Unknown order type: {type}")
        };
    }

    private static OrderStatus ParseOrderStatus(string status)
    {
        return status?.ToUpperInvariant() switch
        {
            "NEW" => OrderStatus.New,
            "PARTIALLYFILLED" or "PARTIALLY_FILLED" => OrderStatus.PartiallyFilled,
            "FILLED" => OrderStatus.Filled,
            "CANCELLED" or "CANCELED" => OrderStatus.Cancelled,
            "REJECTED" => OrderStatus.Rejected,
            "UNTRIGGERED" => OrderStatus.Untriggered,
            "TRIGGERED" => OrderStatus.Triggered,
            "DEACTIVATED" => OrderStatus.Deactivated,
            _ => throw new DomainException($"Unknown order status: {status}")
        };
    }
}
