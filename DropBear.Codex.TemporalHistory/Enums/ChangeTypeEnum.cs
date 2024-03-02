namespace DropBear.Codex.TemporalHistory.Enums;

/// <summary>
///     Represents the type of changes made to an entity.
/// </summary>
public enum ChangeTypeEnum
{
    /// <summary>
    ///     Entity was added.
    /// </summary>
    Added,

    /// <summary>
    ///     Entity was updated.
    /// </summary>
    Updated,

    /// <summary>
    ///     Entity was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    ///     Entity state was not available.
    /// </summary>
    NotAvailable

    // Include additional options as necessary, depending on the requirements.
}
