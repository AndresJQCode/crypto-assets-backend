namespace Api.Application.Dtos.Portfolio;

/// <summary>
/// Portfolio data transfer object.
/// </summary>
public class PortfolioDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal InitialCapital { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public decimal TotalTradingProfit { get; set; }
    public decimal TotalTradingLoss { get; set; }
    public decimal TotalFees { get; set; }
    public decimal NetProfitLoss { get; set; }
    public decimal ROI { get; set; }
    public string Currency { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}
