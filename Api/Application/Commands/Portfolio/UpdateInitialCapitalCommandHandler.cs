using MediatR;
using System.Security.Claims;
using Api.Application.Dtos.Portfolio;
using Domain.AggregatesModel.PortfolioAggregate;
using Domain.Exceptions;

namespace Api.Application.Commands.Portfolio;

public class UpdateInitialCapitalCommandHandler(
    IPortfolioRepository portfolioRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<UpdateInitialCapitalCommand, PortfolioDto>
{
    public async Task<PortfolioDto> Handle(UpdateInitialCapitalCommand request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnAuthorizedException();

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnAuthorizedException();

        var portfolio = await portfolioRepository.GetWithTransactionsAsync(
            (await portfolioRepository.GetByUserIdAsync(userId, cancellationToken))?.Id ?? Guid.Empty,
            cancellationToken)
            ?? throw new NotFoundException("Portfolio not found for current user");

        portfolio.UpdateInitialCapital(request.NewInitialCapital);

        portfolioRepository.Update(portfolio);
        await portfolioRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return new PortfolioDto
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            InitialCapital = portfolio.InitialCapital,
            CurrentBalance = portfolio.CurrentBalance,
            TotalDeposits = portfolio.TotalDeposits,
            TotalWithdrawals = portfolio.TotalWithdrawals,
            TotalTradingProfit = portfolio.TotalTradingProfit,
            TotalTradingLoss = portfolio.TotalTradingLoss,
            TotalFees = portfolio.TotalFees,
            NetProfitLoss = portfolio.NetProfitLoss,
            ROI = portfolio.ROI,
            Currency = portfolio.Currency,
            IsActive = portfolio.IsActive,
            LastUpdatedAt = portfolio.LastUpdatedAt,
            CreatedOn = portfolio.CreatedOn
        };
    }
}
