using Api.Application.Dtos;
using Api.Application.Queries;
using Api.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Api.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically handles pagination for queries implementing IPaginatedQuery
/// </summary>
public class PaginationBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<PaginationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only process if request is a paginated query
        if (request is not IPaginatedQuery<TResponse> paginatedQuery)
        {
            return await next(cancellationToken);
        }

        // Extract pagination parameters from query string
        var paginationParams = PaginationHelper.GetPaginationParametersFromQueryString(httpContextAccessor);
        paginatedQuery.PaginationParameters = paginationParams;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Pagination parameters injected into {QueryType}: Page={Page}, Limit={Limit}",
                typeof(TRequest).Name,
                paginationParams.Page,
                paginationParams.Limit);
        }

        // Execute the query
        var response = await next(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Pagination query executed successfully for {QueryType}",
                typeof(TRequest).Name);
        }

        return response;
    }
}
