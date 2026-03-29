namespace Api.Application.Dtos.Portfolio;

/// <summary>
/// Portfolio transaction data transfer object.
/// </summary>
public class PortfolioTransactionDto
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Type { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Currency { get; set; } = default!;
    public Guid? TradingOrderId { get; set; }
    public string? Notes { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}
