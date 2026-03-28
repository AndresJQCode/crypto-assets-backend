using Api.Application.Dtos;
using Api.Utilities;

namespace Api.Application.Queries;

/// <summary>
/// Marker interface for paginated queries
/// Queries implementing this interface will automatically have pagination parameters injected
/// and pagination headers added to the response
/// </summary>
public interface IPaginatedQuery<TResponse>
{
    PaginationParameters PaginationParameters { get; set; }
}
