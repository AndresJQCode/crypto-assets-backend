using Api.Application.Dtos;
using Api.Application.Queries;
using MediatR;

namespace Api.Extensions;

/// <summary>
/// Extension methods for paginated endpoints
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Maps a GET endpoint that returns paginated results
    /// Automatically extracts pagination parameters from query string and adds pagination headers
    /// </summary>
    /// <typeparam name="TQuery">The query type that implements IPaginatedQuery</typeparam>
    /// <typeparam name="TDto">The DTO type returned in the paginated response</typeparam>
    /// <param name="group">The route group builder</param>
    /// <param name="pattern">The route pattern</param>
    /// <param name="queryFactory">Factory function to create the query instance</param>
    /// <returns>RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapGetPaginated<TQuery, TDto>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TQuery> queryFactory)
        where TQuery : IPaginatedQuery<PaginationResponseDto<TDto>>
    {
        return group.MapGet(pattern, async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = queryFactory();
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });
    }

    /// <summary>
    /// Maps a GET endpoint that returns paginated results with additional route parameters
    /// </summary>
    public static RouteHandlerBuilder MapGetPaginated<TQuery, TDto, TParam>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TParam, TQuery> queryFactory)
        where TQuery : IPaginatedQuery<PaginationResponseDto<TDto>>
    {
        return group.MapGet(pattern, async (
            TParam param,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = queryFactory(param);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });
    }

    /// <summary>
    /// Maps a GET endpoint that returns paginated results with two route parameters
    /// </summary>
    public static RouteHandlerBuilder MapGetPaginated<TQuery, TDto, TParam1, TParam2>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TParam1, TParam2, TQuery> queryFactory)
        where TQuery : IPaginatedQuery<PaginationResponseDto<TDto>>
    {
        return group.MapGet(pattern, async (
            TParam1 param1,
            TParam2 param2,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = queryFactory(param1, param2);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });
    }
}
