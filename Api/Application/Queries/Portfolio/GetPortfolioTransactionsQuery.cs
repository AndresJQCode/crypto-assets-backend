using MediatR;
using Api.Application.Dtos.Portfolio;

namespace Api.Application.Queries.Portfolio;

/// <summary>
/// Query to get portfolio transaction history.
/// </summary>
public record GetPortfolioTransactionsQuery : IRequest<List<PortfolioTransactionDto>>;
