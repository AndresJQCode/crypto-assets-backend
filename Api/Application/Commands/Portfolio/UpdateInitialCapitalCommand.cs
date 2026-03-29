using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Commands.Portfolio;

/// <summary>
/// Command to update initial capital (only allowed before any transactions).
/// </summary>
public record UpdateInitialCapitalCommand(
    decimal NewInitialCapital
) : IRequest<PortfolioDto>;
