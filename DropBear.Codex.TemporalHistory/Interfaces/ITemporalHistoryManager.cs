using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Interfaces;

public interface ITemporalHistoryManager<TContext> where TContext : DbContext
{
    /// <summary>
    ///     Retrieves historical records for a given entity type within the specified date range.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start date of the period.</param>
    /// <param name="to">The end date of the period.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A list of temporal records for the specified entity.</returns>
    Task<IEnumerable<TemporalRecord<T>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves a snapshot of an entity at a specific point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="pointInTime">The specific point in time for the snapshot.</param>
    /// <param name="key">The key value of the entity.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A snapshot of the entity if found; otherwise, null.</returns>
    Task<T?> GetEntitySnapshotAt<T>(DateTime pointInTime, object key,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    ///     Retrieves the latest changes for a given entity type, limited by a maximum number of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="maxResults">The maximum number of records to retrieve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A list of the latest temporal records.</returns>
    Task<IEnumerable<TemporalRecord<T>>> GetLatestChanges<T>(int maxResults,
        CancellationToken cancellationToken = default) where T : class;
}
