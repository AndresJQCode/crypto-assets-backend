using Domain.SeedWork;

namespace Domain.AggregatesModel.PortfolioAggregate;

/// <summary>
/// Portfolio transaction type enumeration.
/// </summary>
public class TransactionType : Enumeration
{
    public static readonly TransactionType Deposit = new(1, nameof(Deposit));
    public static readonly TransactionType Withdrawal = new(2, nameof(Withdrawal));
    public static readonly TransactionType TradingProfit = new(3, nameof(TradingProfit));
    public static readonly TransactionType TradingLoss = new(4, nameof(TradingLoss));
    public static readonly TransactionType Fee = new(5, nameof(Fee));

    public TransactionType(int id, string name)
        : base(id, name)
    {
    }
}
