using MediatR;
using System.Security.Claims;
using Api.Application.Dtos.Portfolio;
using Domain.AggregatesModel.PortfolioAggregate;
using Domain.Exceptions;

namespace Api.Application.Commands.Portfolio;

public class CreatePortfolioCommandHandler(
    IPortfolioRepository portfolioRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreatePortfolioCommand, PortfolioDto>
{
    public async Task<PortfolioDto> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new UnAuthorizedException();

        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnAuthorizedException();

        // Check if user already has a portfolio
        var existingPortfolio = await portfolioRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingPortfolio is not null)
            throw new DomainException("User already has a portfolio. Use update endpoint to modify initial capital.");

        // Create new portfolio
        var portfolio = Domain.AggregatesModel.PortfolioAggregate.Portfolio.Create(
            userId,
            request.InitialCapital,
            request.Currency);

        await portfolioRepository.Create(portfolio, cancellationToken);
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
