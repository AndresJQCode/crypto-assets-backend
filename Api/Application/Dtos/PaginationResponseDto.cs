namespace Api.Application.Dtos;

public sealed class PaginationResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int Limit { get; set; }
    public int Page { get; set; }
}
