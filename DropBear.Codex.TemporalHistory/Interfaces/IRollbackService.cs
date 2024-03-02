namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the contract for services that support rolling back records to a previous state.
/// </summary>
public interface IRollbackService
{
    /// <summary>
    ///     Asynchronously rolls back a record of a specific type to its state at a given point in time.
    /// </summary>
    /// <typeparam name="T">The type of the record, which must implement ITemporal.</typeparam>
    /// <param name="recordId">The identifier of the record.</param>
    /// <param name="toDateTime">The point in time to which the record should be rolled back.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result indicates whether the rollback was
    ///     successful.
    /// </returns>
    Task<bool> RollbackRecordAsync<T>(int recordId, DateTime toDateTime) where T : class, ITemporal;
}
