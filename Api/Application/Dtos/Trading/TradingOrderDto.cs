namespace Api.Application.Dtos.Trading;

public class TradingOrderDto
{
    public string Id { get; set; } = string.Empty;
    public string ExternalOrderId { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal FilledQuantity { get; set; }
    public decimal? AveragePrice { get; set; }
    public decimal? Fee { get; set; }
    public string? FeeCurrency { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
}
