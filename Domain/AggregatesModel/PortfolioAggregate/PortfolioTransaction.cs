using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.PortfolioAggregate;

/// <summary>
/// Portfolio transaction entity.
/// Tracks deposits, withdrawals, and trading P&L.
/// </summary>
public class PortfolioTransaction : Entity<Guid>
{
    public Guid PortfolioId { get; private set; }
    public TransactionType Type { get; private set; } = default!;

    /// <summary>
    /// Transaction amount (positive for credits, negative for debits).
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Balance after this transaction.
    /// </summary>
    public decimal BalanceAfter { get; private set; }

    /// <summary>
    /// Currency (e.g., USDT, USD).
    /// </summary>
    public string Currency { get; private set; } = default!;

    /// <summary>
    /// Optional reference to related trading order.
    /// </summary>
    public Guid? TradingOrderId { get; private set; }

    /// <summary>
    /// Transaction notes/description.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Transaction timestamp.
    /// </summary>
    public DateTime TransactionDate { get; private set; }

    // Navigation property
    public Portfolio Portfolio { get; private set; } = default!;

    // EF Core constructor
    private PortfolioTransaction() { }

    /// <summary>
    /// Factory method to create a portfolio transaction.
    /// </summary>
    public static PortfolioTransaction Create(
        Guid portfolioId,
        TransactionType type,
        decimal amount,
        decimal balanceAfter,
        string currency,
        Guid? tradingOrderId = null,
        string? notes = null)
    {
        if (amount == 0)
            throw new DomainException("Transaction amount cannot be zero");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");

        var transaction = new PortfolioTransaction
        {
            Id = Guid.CreateVersion7(),
            PortfolioId = portfolioId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Currency = currency.ToUpperInvariant(),
            TradingOrderId = tradingOrderId,
            Notes = notes,
            TransactionDate = DateTime.UtcNow,
            CreatedOn = DateTimeOffset.UtcNow
        };

        return transaction;
    }
}
