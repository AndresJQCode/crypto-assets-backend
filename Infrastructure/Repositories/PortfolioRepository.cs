using Domain.AggregatesModel.PortfolioAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

public class PortfolioRepository(
    ApiContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PortfolioRepository> logger)
    : Repository<Portfolio>(context, httpContextAccessor, logger), IPortfolioRepository
{
    private readonly ApiContext _context = context;

    public async Task<Portfolio?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Portfolios
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<Portfolio?> GetWithTransactionsAsync(Guid portfolioId, CancellationToken cancellationToken = default)
    {
        return await _context.Portfolios
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == portfolioId, cancellationToken);
    }

    public async Task<bool> UserHasPortfolioAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Portfolios
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId, cancellationToken);
    }
}
