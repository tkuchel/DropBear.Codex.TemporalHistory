namespace DropBear.Codex.TemporalHistory.Bases;

/// <summary>
///     Represents the base class for temporal entities, providing properties to track the validity period.
/// </summary>
public class TemporalEntityBase
{
    private DateTime _validFrom;
    private DateTime _validTo;

    /// <summary>
    ///     Gets or sets the start date and time of the validity period.
    /// </summary>
    public DateTime ValidFrom
    {
        get => _validFrom;
        set
        {
            // Example validation, could be extended based on requirements
            if (value > ValidTo)
                throw new ArgumentException("ValidFrom must be earlier than ValidTo.", paramName: nameof(ValidFrom));
            _validFrom = value;
        }
    }

    /// <summary>
    ///     Gets or sets the end date and time of the validity period.
    /// </summary>
    public DateTime ValidTo
    {
        get => _validTo;
        set
        {
            if (value < ValidFrom)
                throw new ArgumentException("ValidTo must be later than ValidFrom.", paramName: nameof(ValidTo));
            _validTo = value;
        }
    }
}
