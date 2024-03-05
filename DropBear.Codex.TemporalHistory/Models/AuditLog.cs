namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a log of an audit operation, detailing the change made to an entity.
/// </summary>
public class AuditLog
{
    /// <summary>
    ///     Gets or sets the primary key of the audit log entry.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    ///     Gets or sets the identifier of the user who made the change.
    ///     Consider using Guid if the user system uses Guids for identifiers.
    /// </summary>
    public string UserId { get; init; } = Guid.Empty.ToString();

    /// <summary>
    ///     Gets or sets the timestamp when the change was made.
    /// </summary>
    public DateTime ChangeTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets or sets the operation code representing how the change was made.
    ///     Consider using an enum if there are predefined operation codes.
    /// </summary>
    public string OperationCode { get; init; } = string.Empty;

    /// <summary>
    ///     Gets or sets the unique identifier linking this log to a record in a temporal table.
    /// </summary>
    public Guid RecordNumber { get; init; }

    /// <summary>
    ///     Gets or sets the reason why the change was made.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
