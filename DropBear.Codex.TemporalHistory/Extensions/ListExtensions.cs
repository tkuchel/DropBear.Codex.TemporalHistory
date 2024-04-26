namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extension methods for lists that are commonly used
///     within the library to manipulate and query lists.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    ///     Safely adds an item to a list if the item is not already present.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to which the item will be added.</param>
    /// <param name="item">The item to add if it's not already in the list.</param>
    /// <returns>True if the item was added, false otherwise.</returns>
    public static bool AddIfNotExists<T>(this List<T> list, T item)
    {
        if (list.Contains(item)) return false;
        list.Add(item);
        return true;

    }
}
