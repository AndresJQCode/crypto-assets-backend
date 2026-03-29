namespace Api.Application.Dtos.TradingOrder;

/// <summary>
/// Trading order data transfer object.
/// Frontend-compatible format matching the TypeScript Order interface.
/// </summary>
public class TradingOrderDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Exchange/Connector Instance ID (frontend calls it exchangeId).
    /// </summary>
    public Guid ExchangeId { get; set; }
    
    /// <summary>
    /// External order ID from the exchange (e.g., Bybit order ID).
    /// </summary>
    public string? ExchangeOrderId { get; set; }
    
    /// <summary>
    /// Trading pair (e.g., BTC/USDT).
    /// </summary>
    public TradingPairDto Pair { get; set; } = default!;
    
    /// <summary>
    /// Order type: market, limit, stop_loss, take_profit.
    /// </summary>
    public string Type { get; set; } = default!;
    
    /// <summary>
    /// Order side: buy, sell.
    /// </summary>
    public string Side { get; set; } = default!;
    
    /// <summary>
    /// Order state: Created, Submitted, PartiallyFilled, Filled, Cancelled, Rejected.
    /// </summary>
    public string State { get; set; } = default!;
    
    /// <summary>
    /// Order status (derived from state): open, closed.
    /// </summary>
    public string Status { get; set; } = default!;
    
    public decimal Quantity { get; set; }
    public decimal FilledQuantity { get; set; }
    
    /// <summary>
    /// Remaining quantity (calculated: Quantity - FilledQuantity).
    /// </summary>
    public decimal RemainingQuantity { get; set; }
    
    public decimal? Price { get; set; }
    public decimal? AverageFilledPrice { get; set; }
    public decimal? TotalValue { get; set; }
    
    /// <summary>
    /// Current market price (for unrealized PnL calculation).
    /// </summary>
    public decimal? CurrentPrice { get; set; }
    
    /// <summary>
    /// Profit/Loss in quote currency.
    /// </summary>
    public decimal? Pnl { get; set; }
    
    /// <summary>
    /// Profit/Loss percentage.
    /// </summary>
    public decimal? PnlPercentage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
