using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Queries.Portfolio;

/// <summary>
/// Query to get the current user's portfolio.
/// </summary>
public record GetUserPortfolioQuery : IRequest<PortfolioDto>;
