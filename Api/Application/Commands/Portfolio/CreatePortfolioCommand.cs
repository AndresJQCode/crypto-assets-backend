using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Commands.Portfolio;

/// <summary>
/// Command to create a new portfolio with initial capital.
/// </summary>
public record CreatePortfolioCommand(
    decimal InitialCapital,
    string Currency = "USDT"
) : IRequest<PortfolioDto>;
