namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
/// Represents a paginated result.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public class PaginatedResult<T>
{
    public IEnumerable<T>? Items { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
