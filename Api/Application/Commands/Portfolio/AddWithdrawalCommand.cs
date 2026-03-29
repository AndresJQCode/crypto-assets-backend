using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Commands.Portfolio;

/// <summary>
/// Command to add a withdrawal from the portfolio.
/// </summary>
public record AddWithdrawalCommand(
    decimal Amount,
    string? Notes = null
) : IRequest<PortfolioDto>;
