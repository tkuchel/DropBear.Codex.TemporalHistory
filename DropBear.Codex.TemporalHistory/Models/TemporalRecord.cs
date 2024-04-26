namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a temporal record of an entity, encapsulating the entity and its temporal validity range.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class TemporalRecord<T> where T : class
{
    /// <summary>
    ///     Gets or sets the entity associated with this temporal record.
    /// </summary>
    public T Entity { get; set; }

    /// <summary>
    ///     Gets or sets the starting timestamp of the record's validity.
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    ///     Gets or sets the ending timestamp of the record's validity.
    /// </summary>
    public DateTime ValidTo { get; set; }
}
