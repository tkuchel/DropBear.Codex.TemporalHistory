using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the contract for services managing and querying temporal history of entities.
/// </summary>
/// <typeparam name="TContext">The type of the database context.</typeparam>
public interface ITemporalHistoryService<TContext>
{
    /// <summary>
    ///     Retrieves the history of changes for a specific entity type within a given time range, utilizing caching.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumeration of temporal records.</returns>
    Task<IEnumerable<TemporalRecord<T>>?> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves the complete history of changes for a specific entity type, utilizing caching to optimize performance.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing all temporal records for the entity type.</returns>
    Task<IEnumerable<TemporalRecord<T>>?> GetAllHistoryAsync<T>(CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    ///     Rolls back the state of entities to a specific point in time, applying the historical state as the current state.
    /// </summary>
    /// <typeparam name="T">The entity type to rollback.</typeparam>
    /// <param name="to">The point in time to rollback the entity states to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating success or failure.</returns>
    Task<bool> RollbackAsync<T>(DateTime to, CancellationToken cancellationToken = default) where T : class;
}
