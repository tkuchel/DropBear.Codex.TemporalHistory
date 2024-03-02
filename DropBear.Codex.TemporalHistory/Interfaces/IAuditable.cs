namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Represents an entity that can be audited with creation and modification timestamps and identifiers.
/// </summary>
public interface IAuditable
{
    /// <summary>
    ///     Gets or sets the timestamp when the entity was created.
    ///     Implementers should ensure this uses UTC to avoid timezone issues.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp when the entity was last modified.
    ///     Implementers should ensure this uses UTC to avoid timezone issues.
    /// </summary>
    DateTime ModifiedAt { get; set; }

    /// <summary>
    ///     Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string ModifiedBy { get; set; }
}
