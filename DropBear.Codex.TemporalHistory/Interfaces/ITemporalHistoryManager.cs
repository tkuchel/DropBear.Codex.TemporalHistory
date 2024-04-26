using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Interfaces;

public interface ITemporalHistoryManager<TContext> where TContext : DbContext
{
    Task<IEnumerable<TemporalRecord<T>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class;

    Task<T?> GetEntitySnapshotAt<T>(DateTime pointInTime, object key,
        CancellationToken cancellationToken = default) where T : class;

    Task<T?> GetEntitySnapshotAt<T>(DateTime snapshotTime, CancellationToken cancellationToken = default)
        where T : class;

    Task<IEnumerable<TemporalRecord<T>>> GetLatestChanges<T>(int maxResults,
        CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<TemporalRecord<T>>> GetChangesByUser<T>(string userId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<PropertyChange>> CompareEntityStates<T>(DateTime fromTime, DateTime toTime,
        CancellationToken cancellationToken = default) where T : class;

    Task<IEnumerable<TemporalRecord<T>>> GetHistoryForPeriod<T>(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default) where T : class;
}
