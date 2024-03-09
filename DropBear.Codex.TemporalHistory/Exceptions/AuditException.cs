namespace DropBear.Codex.TemporalHistory.Exceptions;

/// <summary>
///     Represents errors that occur during the auditing process.
/// </summary>
public class AuditException : Exception
{
    public AuditException()
    {
    }

    public AuditException(string message) : base(message) { }

    public AuditException(string message, Exception innerException) : base(message, innerException) { }
}
