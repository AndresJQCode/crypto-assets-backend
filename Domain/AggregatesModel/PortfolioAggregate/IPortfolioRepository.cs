using Domain.SeedWork;

namespace Domain.AggregatesModel.PortfolioAggregate;

/// <summary>
/// Portfolio repository interface.
/// </summary>
public interface IPortfolioRepository : IRepository<Portfolio>
{
    /// <summary>
    /// Get portfolio by user ID.
    /// </summary>
    Task<Portfolio?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get portfolio with transactions.
    /// </summary>
    Task<Portfolio?> GetWithTransactionsAsync(Guid portfolioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user already has a portfolio.
    /// </summary>
    Task<bool> UserHasPortfolioAsync(Guid userId, CancellationToken cancellationToken = default);
}
