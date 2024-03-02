namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Represents an entity that supports SQL Server's temporal table features, enabling tracking of historical data
///     changes.
/// </summary>
public interface ITemporal
{
    /// <summary>
    ///     Gets or sets the start of the period for which the entity's state is valid. Managed automatically by SQL Server.
    /// </summary>
    DateTime ValidFrom { get; set; }

    /// <summary>
    ///     Gets or sets the end of the period for which the entity's state is valid. Managed automatically by SQL Server.
    /// </summary>
    DateTime ValidTo { get; set; }
}
