using DropBear.Codex.Core.ReturnTypes;
using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the contract for services managing and querying temporal history of entities.
/// </summary>
public interface ITemporalHistoryService<TContext> where TContext : class
{
    /// <summary>
    ///     Retrieves the history of changes for a specific entity type within a given time range, utilizing caching.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing a result with an enumeration of temporal records.</returns>
    Task<Result<IEnumerable<TemporalRecord<T>>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves the complete history of changes for a specific entity type, utilizing caching to optimize performance.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing all temporal records for the entity type.</returns>
    Task<Result<IEnumerable<TemporalRecord<T>>>> GetAllHistoryAsync<T>(CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    ///     Rolls back the state of entities to a specific point in time, applying the historical state as the current state.
    /// </summary>
    /// <typeparam name="T">The entity type to rollback.</typeparam>
    /// <param name="to">The point in time to rollback the entity states to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating success or failure.</returns>
    Task<Result> RollbackAsync<T>(DateTime to, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves the previous version of an entity before a specified point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the previous version of the entity.</returns>
    Task<Result<T>> GetPreviousVersionAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves the next version of an entity after a specified point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the next version of the entity.</returns>
    Task<Result<T>> GetNextVersionAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Reverts an entity to its state at a specific point in time and logs this reversion as a new version in the history.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity to revert.</param>
    /// <param name="to">The point in time to revert to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, indicating whether the reversion was successful.</returns>
    Task<Result> RevertToVersionAsync<T>(Guid entityId, DateTime to, CancellationToken cancellationToken = default)
        where T : class;
}
