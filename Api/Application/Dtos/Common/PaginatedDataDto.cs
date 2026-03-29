namespace Api.Application.Dtos.Common;

/// <summary>
/// Generic paginated data transfer object.
/// </summary>
public class PaginatedDataDto<T>
{
    public List<T> Data { get; init; } = [];
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int Page { get; init; }
    public int Limit { get; init; }
}
