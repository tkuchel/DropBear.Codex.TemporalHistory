using System.Collections.ObjectModel;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extension methods for working with <see cref="List{T}" /> instances.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Creates a read-only wrapper around the specified <see cref="List{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The source <see cref="List{T}" /> instance.</param>
    /// <returns>A <see cref="ReadOnlyCollection{T}" /> that wraps the source list.</returns>
    public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this List<T> list) => new(list);
}
