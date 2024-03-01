namespace DropBear.Codex.TemporalHistory.Interfaces
{
    /// <summary>
    /// Defines the structure and requirements for entities that support SQL Server's temporal table features.
    /// Entities implementing this interface should have ValidFrom and ValidTo properties 
    /// to track the historical data changes over time.
    /// </summary>
    public interface ITemporal
    {
        /// <summary>
        /// Gets or sets the start of the period for which the entity's state is valid.
        /// This property is automatically managed by SQL Server.
        /// </summary>
        DateTime ValidFrom { get; set; }

        /// <summary>
        /// Gets or sets the end of the period for which the entity's state is valid.
        /// This property is automatically managed by SQL Server.
        /// </summary>
        DateTime ValidTo { get; set; }
    }
}
