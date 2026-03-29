namespace Api.Application.Dtos.TradingOrder;

/// <summary>
/// Profit and Loss metrics data transfer object.
/// </summary>
public class PnLMetricsDto
{
    /// <summary>
    /// Total PnL (realized + unrealized).
    /// </summary>
    public decimal TotalPnL { get; set; }
    
    /// <summary>
    /// Realized PnL from closed/filled orders.
    /// </summary>
    public decimal RealizedPnL { get; set; }
    
    /// <summary>
    /// Unrealized PnL from open orders.
    /// </summary>
    public decimal UnrealizedPnL { get; set; }
    
    /// <summary>
    /// Win rate percentage (0-100).
    /// </summary>
    public decimal WinRate { get; set; }
    
    /// <summary>
    /// Total number of closed trades.
    /// </summary>
    public int TotalTrades { get; set; }
}
