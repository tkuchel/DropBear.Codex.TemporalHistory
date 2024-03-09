namespace DropBear.Codex.TemporalHistory.Exceptions;

/// <summary>
///     Custom exception class for unsupported temporal feature scenarios
/// </summary>
public class UnsupportedTemporalFeatureException : Exception
{
    public UnsupportedTemporalFeatureException(string message) : base(message) { }

    public UnsupportedTemporalFeatureException()
    {
    }

    public UnsupportedTemporalFeatureException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
