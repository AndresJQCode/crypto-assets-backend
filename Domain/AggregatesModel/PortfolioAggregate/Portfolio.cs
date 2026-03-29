using Domain.Exceptions;
using Domain.SeedWork;

namespace Domain.AggregatesModel.PortfolioAggregate;

/// <summary>
/// Portfolio aggregate root.
/// Tracks user's capital, balance, and transaction history.
/// </summary>
public class Portfolio : Entity<Guid>, IAggregateRoot
{
    public Guid UserId { get; private set; }

    /// <summary>
    /// Initial capital when the portfolio was created.
    /// </summary>
    public decimal InitialCapital { get; private set; }

    /// <summary>
    /// Current balance (capital + deposits - withdrawals +/- trading P&L).
    /// </summary>
    public decimal CurrentBalance { get; private set; }

    /// <summary>
    /// Total deposited amount (excluding initial capital).
    /// </summary>
    public decimal TotalDeposits { get; private set; }

    /// <summary>
    /// Total withdrawn amount.
    /// </summary>
    public decimal TotalWithdrawals { get; private set; }

    /// <summary>
    /// Total profit from trading.
    /// </summary>
    public decimal TotalTradingProfit { get; private set; }

    /// <summary>
    /// Total loss from trading.
    /// </summary>
    public decimal TotalTradingLoss { get; private set; }

    /// <summary>
    /// Total fees paid.
    /// </summary>
    public decimal TotalFees { get; private set; }

    /// <summary>
    /// Portfolio currency (e.g., USDT, USD).
    /// </summary>
    public string Currency { get; private set; } = default!;

    /// <summary>
    /// Portfolio status.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Last time the balance was updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; private set; }

    // Navigation properties
    private readonly List<PortfolioTransaction> _transactions = [];
    public IReadOnlyCollection<PortfolioTransaction> Transactions => _transactions.AsReadOnly();

    // EF Core constructor
    private Portfolio() { }

    /// <summary>
    /// Factory method to create a new portfolio.
    /// </summary>
    public static Portfolio Create(
        Guid userId,
        decimal initialCapital,
        string currency)
    {
        if (initialCapital < 0)
            throw new DomainException("Initial capital cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");

        var portfolio = new Portfolio
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            InitialCapital = initialCapital,
            CurrentBalance = initialCapital,
            TotalDeposits = 0,
            TotalWithdrawals = 0,
            TotalTradingProfit = 0,
            TotalTradingLoss = 0,
            TotalFees = 0,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            LastUpdatedAt = DateTime.UtcNow,
            CreatedOn = DateTimeOffset.UtcNow
        };

        // Create initial transaction record
        var initialTransaction = PortfolioTransaction.Create(
            portfolio.Id,
            TransactionType.Deposit,
            initialCapital,
            initialCapital,
            currency,
            notes: "Initial capital");

        portfolio._transactions.Add(initialTransaction);

        // Domain event (for future use)
        // portfolio.AddDomainEvent(new PortfolioCreatedEvent(portfolio.Id, userId, initialCapital));

        return portfolio;
    }

    /// <summary>
    /// Add a deposit to the portfolio.
    /// </summary>
    public void AddDeposit(decimal amount, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot add deposit to inactive portfolio");

        if (amount <= 0)
            throw new DomainException("Deposit amount must be positive");

        TotalDeposits += amount;
        CurrentBalance += amount;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        var transaction = PortfolioTransaction.Create(
            Id,
            TransactionType.Deposit,
            amount,
            CurrentBalance,
            Currency,
            notes: notes);

        _transactions.Add(transaction);

        // Domain event
        // this.AddDomainEvent(new DepositAddedEvent(Id, amount));
    }

    /// <summary>
    /// Add a withdrawal from the portfolio.
    /// </summary>
    public void AddWithdrawal(decimal amount, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot withdraw from inactive portfolio");

        if (amount <= 0)
            throw new DomainException("Withdrawal amount must be positive");

        if (CurrentBalance < amount)
            throw new DomainException("Insufficient balance for withdrawal");

        TotalWithdrawals += amount;
        CurrentBalance -= amount;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        var transaction = PortfolioTransaction.Create(
            Id,
            TransactionType.Withdrawal,
            -amount,
            CurrentBalance,
            Currency,
            notes: notes);

        _transactions.Add(transaction);

        // Domain event
        // this.AddDomainEvent(new WithdrawalAddedEvent(Id, amount));
    }

    /// <summary>
    /// Record trading profit.
    /// </summary>
    public void RecordTradingProfit(decimal profit, Guid tradingOrderId, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot record profit on inactive portfolio");

        if (profit <= 0)
            throw new DomainException("Profit amount must be positive");

        TotalTradingProfit += profit;
        CurrentBalance += profit;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        var transaction = PortfolioTransaction.Create(
            Id,
            TransactionType.TradingProfit,
            profit,
            CurrentBalance,
            Currency,
            tradingOrderId,
            notes);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Record trading loss.
    /// </summary>
    public void RecordTradingLoss(decimal loss, Guid tradingOrderId, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot record loss on inactive portfolio");

        if (loss <= 0)
            throw new DomainException("Loss amount must be positive");

        TotalTradingLoss += loss;
        CurrentBalance -= loss;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        var transaction = PortfolioTransaction.Create(
            Id,
            TransactionType.TradingLoss,
            -loss,
            CurrentBalance,
            Currency,
            tradingOrderId,
            notes);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Record trading fee.
    /// </summary>
    public void RecordFee(decimal fee, Guid? tradingOrderId = null, string? notes = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot record fee on inactive portfolio");

        if (fee <= 0)
            throw new DomainException("Fee amount must be positive");

        TotalFees += fee;
        CurrentBalance -= fee;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        var transaction = PortfolioTransaction.Create(
            Id,
            TransactionType.Fee,
            -fee,
            CurrentBalance,
            Currency,
            tradingOrderId,
            notes);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Update initial capital (only if no transactions other than initial deposit).
    /// </summary>
    public void UpdateInitialCapital(decimal newInitialCapital)
    {
        if (newInitialCapital < 0)
            throw new DomainException("Initial capital cannot be negative");

        if (_transactions.Count > 1)
            throw new DomainException("Cannot update initial capital after transactions have been made");

        var difference = newInitialCapital - InitialCapital;

        InitialCapital = newInitialCapital;
        CurrentBalance += difference;
        LastUpdatedAt = DateTime.UtcNow;
        LastModifiedOn = DateTimeOffset.UtcNow;

        // Update the initial transaction
        if (_transactions.Count == 1)
        {
            _transactions.Clear();
            var initialTransaction = PortfolioTransaction.Create(
                Id,
                TransactionType.Deposit,
                newInitialCapital,
                newInitialCapital,
                Currency,
                notes: "Initial capital (updated)");
            _transactions.Add(initialTransaction);
        }
    }

    /// <summary>
    /// Deactivate the portfolio.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reactivate the portfolio.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Net profit/loss (profit - loss - fees).
    /// </summary>
    public decimal NetProfitLoss =>
        TotalTradingProfit - TotalTradingLoss - TotalFees;

    /// <summary>
    /// Return on investment percentage.
    /// </summary>
    public decimal ROI
    {
        get
        {
            if (InitialCapital == 0)
                return 0;

            var totalInvested = InitialCapital + TotalDeposits - TotalWithdrawals;
            if (totalInvested == 0)
                return 0;

            return ((CurrentBalance - totalInvested) / totalInvested) * 100;
        }
    }

    /// <summary>
    /// Check if portfolio can be deleted.
    /// </summary>
    public bool CanBeDeleted() => !IsActive && CurrentBalance == 0;
}
