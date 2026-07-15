// Defines a reusable pagination result for query use cases.
namespace WebApi.Core.Pagination;

/// <summary>
/// Generic paged result used by query use cases and HTTP responses.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}