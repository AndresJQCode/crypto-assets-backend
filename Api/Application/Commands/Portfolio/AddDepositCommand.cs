using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Commands.Portfolio;

/// <summary>
/// Command to add a deposit to the portfolio.
/// </summary>
public record AddDepositCommand(
    decimal Amount,
    string? Notes = null
) : IRequest<PortfolioDto>;
