namespace Domain.SeedWork;

public record PagedResult<T>
{
    public int TotalCount { get; init; }
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult(int totalCount, IReadOnlyList<T> items, int page, int pageSize)
    {
        TotalCount = totalCount;
        Items = items;
        Page = page;
        PageSize = pageSize;
    }
}
