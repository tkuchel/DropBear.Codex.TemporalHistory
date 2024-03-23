using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides services for managing and querying temporal history of entities in a database context.
/// </summary>
/// <typeparam name="TContext">The database context type, derived from DbContext.</typeparam>
public class TemporalHistoryService<TContext> : ITemporalHistoryService<TContext> where TContext : DbContext
{
    private readonly IMemoryCache _cache;
    private readonly TContext _context;

    /// <summary>
    ///     Initializes a new instance of the TemporalHistoryService with the specified DbContext and MemoryCache.
    /// </summary>
    /// <param name="context">The DbContext to use for data operations.</param>
    /// <param name="cache">The MemoryCache for caching query results.</param>
    public TemporalHistoryService(TContext context, IMemoryCache cache)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    ///     Retrieves the history of changes for a given entity type within a specified time range, utilizing caching.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumeration of temporal records.</returns>
    public async Task<IEnumerable<TemporalRecord<T>>?> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = $"GetHistoryAsync_{typeof(T).Name}_{from}_{to}";
        return await GetOrSetCacheAsync(
            cacheKey,
            () => FetchHistoryFromDatabase<T>(from, to, cancellationToken),
            TimeSpan.FromMinutes(5),
            cancellationToken
        ).ConfigureAwait(false);
    }


    /// <summary>
    ///     Retrieves the complete history of changes for a given entity type, utilizing caching to optimize performance.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing all temporal records for the entity type.</returns>
    public async Task<IEnumerable<TemporalRecord<T>>?> GetAllHistoryAsync<T>(
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheKey = $"GetAllHistoryAsync_{typeof(T).Name}";
        return await GetOrSetCacheAsync(cacheKey, () => FetchAllHistoryFromDatabase<T>(cancellationToken),
            TimeSpan.FromMinutes(10), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Rolls back the state of entities to a specific point in time, applying the historical state as the current state.
    /// </summary>
    /// <typeparam name="T">The entity type to rollback.</typeparam>
    /// <param name="to">The point in time to rollback the entity states to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating success or failure.</returns>
    public async Task<bool> RollbackAsync<T>(DateTime to, CancellationToken cancellationToken = default) where T : class
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (transaction.ConfigureAwait(false))
        {
            var entities = await _context.Set<T>().TemporalAsOf(to).ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (entities.Count is 0) return false;

            foreach (var entity in entities) _context.Entry(entity).State = EntityState.Modified; // Mark for update

            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return true;
        }
    }

    // Other methods (GetPreviousVersionAsync, GetNextVersionAsync, RevertToVersionAsync) remain the same,
    // just ensure to include CancellationToken in their signatures and pass it to async calls.

    /// <summary>
    ///     Abstracts caching logic to get or set cache entries.
    /// </summary>
    /// <typeparam name="T">The type of data being cached.</typeparam>
    /// <param name="cacheKey">The key used for caching.</param>
    /// <param name="fetchFunction">The function to fetch data if it's not in the cache.</param>
    /// <param name="expiration">The expiration timespan for the cache entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cached or freshly fetched data.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>?> GetOrSetCacheAsync<T>(string cacheKey,
        Func<Task<IEnumerable<TemporalRecord<T>>>> fetchFunction, TimeSpan expiration,
        CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(cacheKey, out IEnumerable<TemporalRecord<T>>? cachedRecords)) return cachedRecords;
        cachedRecords = await fetchFunction().ConfigureAwait(false);
        _cache.Set(cacheKey, cachedRecords, new MemoryCacheEntryOptions().SetSlidingExpiration(expiration));

        return cachedRecords;
    }

    /// <summary>
    ///     Fetches all history from the database for a given entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All historical records for the entity type.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>> FetchAllHistoryFromDatabase<T>(
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalAll()
            .OrderBy(e => EF.Property<DateTime>(e, "ValidFrom"))
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "ValidFrom"),
                ValidTo = EF.Property<DateTime>(e, "ValidTo")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);


    /// <summary>
    ///     Fetches history from the database for a given entity type within the specified time range.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing temporal records within the specified time range.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>> FetchHistoryFromDatabase<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalBetween(from, to)
            .OrderBy(e => EF.Property<DateTime>(e, "ValidFrom"))
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "ValidFrom"),
                ValidTo = EF.Property<DateTime>(e, "ValidTo")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);
}
