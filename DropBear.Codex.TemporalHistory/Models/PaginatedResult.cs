namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a paginated result, encapsulating the list of items and pagination details.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public class PaginatedResult<T>
{
    /// <summary>
    ///     Gets or sets the collection of items in the current page.
    /// </summary>
    public IEnumerable<T> Items { get; set; }   = Enumerable.Empty<T>();

    /// <summary>
    ///     Gets or sets the total number of records across all pages.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    ///     Gets or sets the current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    ///     Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Gets the total number of pages.
    /// </summary>
    public int TotalPages => (TotalRecords + PageSize - 1) / PageSize;
}
