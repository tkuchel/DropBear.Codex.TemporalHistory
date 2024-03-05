using DropBear.Codex.TemporalHistory.Enums;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents the context for an audit operation, including details about the user, reason, and operation type.
/// </summary>
public class AuditContext
{
    /// <summary>
    ///     Gets or sets the identifier of the user associated with the audit operation.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Gets or sets the reason for the audit operation. This property can be null if no reason is specified.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    ///     Gets or sets the code representing the type of operation being audited.
    /// </summary>
    public OperationCode OperationCode { get; set; }
}
