using MediatR;
using System.Security.Claims;
using Api.Application.Dtos.Portfolio;
using Domain.AggregatesModel.PortfolioAggregate;
using Domain.Exceptions;

namespace Api.Application.Queries.Portfolio;

public class GetPortfolioTransactionsQueryHandler(
    IPortfolioRepository portfolioRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetPortfolioTransactionsQuery, List<PortfolioTransactionDto>>
{
    public async Task<List<PortfolioTransactionDto>> Handle(GetPortfolioTransactionsQuery request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnAuthorizedException();

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnAuthorizedException();

        var userPortfolio = await portfolioRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Portfolio not found for current user");

        var portfolio = await portfolioRepository.GetWithTransactionsAsync(userPortfolio.Id, cancellationToken)
            ?? throw new NotFoundException("Portfolio not found");

        return portfolio.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new PortfolioTransactionDto
            {
                Id = t.Id,
                PortfolioId = t.PortfolioId,
                Type = t.Type.Name,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Currency = t.Currency,
                TradingOrderId = t.TradingOrderId,
                Notes = t.Notes,
                TransactionDate = t.TransactionDate,
                CreatedOn = t.CreatedOn
            })
            .ToList();
    }
}
