using DropBear.Codex.Core;
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
    public async Task<Result<IEnumerable<TemporalRecord<T>>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cacheKey = $"GetHistoryAsync_{typeof(T).Name}_{from}_{to}";
            var getResult = await GetOrSetCacheAsync(
                cacheKey,
                () => FetchHistoryFromDatabase<T>(from, to, cancellationToken),
                TimeSpan.FromMinutes(5)
            ).ConfigureAwait(false);

            return getResult is not null
                ? Result<IEnumerable<TemporalRecord<T>>>.Success(getResult)
                : Result<IEnumerable<TemporalRecord<T>>>.Failure("No history found");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TemporalRecord<T>>>.Failure(ex);
        }
    }


    /// <summary>
    ///     Retrieves the complete history of changes for a given entity type, utilizing caching to optimize performance.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing all temporal records for the entity type.</returns>
    public async Task<Result<IEnumerable<TemporalRecord<T>>>> GetAllHistoryAsync<T>(
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cacheKey = $"GetAllHistoryAsync_{typeof(T).Name}";
            var getResult = await GetOrSetCacheAsync(cacheKey, () => FetchAllHistoryFromDatabase<T>(cancellationToken),
                TimeSpan.FromMinutes(10)).ConfigureAwait(false);

            return getResult is not null
                ? Result<IEnumerable<TemporalRecord<T>>>.Success(getResult)
                : Result<IEnumerable<TemporalRecord<T>>>.Failure("No history found");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TemporalRecord<T>>>.Failure(ex);
        }
    }


    /// <summary>
    ///     Retrieves the previous version of an entity before a specified point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the previous version of the entity.</returns>
    public async Task<Result<T>> GetPreviousVersionAsync<T>(Guid entityId,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var currentEntity = await _context.Set<T>().FindAsync(new object[] { entityId }, cancellationToken)
                .ConfigureAwait(false);
            if (currentEntity is null) return Result<T>.Failure("Entity not found.");

            var validFromProperty = _context.Entry(currentEntity).Property<DateTime>("ValidFrom").CurrentValue;
            var previousVersion = await _context.Set<T>()
                .TemporalAsOf(validFromProperty.AddMilliseconds(-1))
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken)
                .ConfigureAwait(false);

            return previousVersion is not null
                ? Result<T>.Success(previousVersion)
                : Result<T>.Failure("No previous version found.");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }


    /// <summary>
    ///     Retrieves the next version of an entity after a specified point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the next version of the entity.</returns>
    public async Task<Result<T>> GetNextVersionAsync<T>(Guid entityId, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var currentEntity = await _context.Set<T>().FindAsync(new object[] { entityId }, cancellationToken)
                .ConfigureAwait(false);
            if (currentEntity is null) return Result<T>.Failure("Entity not found.");

            var validToProperty = _context.Entry(currentEntity).Property<DateTime>("ValidTo").CurrentValue;
            var nextVersion = await _context.Set<T>()
                .TemporalFromTo(validToProperty, DateTime.UtcNow)
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken)
                .ConfigureAwait(false);

            return nextVersion is not null
                ? Result<T>.Success(nextVersion)
                : Result<T>.Failure("No next version found.");
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex);
        }
    }

    /// <summary>
    ///     Reverts an entity to its state at a specific point in time and logs this reversion as a new version in the history.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="entityId">The unique identifier of the entity to revert.</param>
    /// <param name="to">The point in time to revert to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, indicating whether the reversion was successful.</returns>
    public async Task<Result> RevertToVersionAsync<T>(Guid entityId, DateTime to,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var targetVersion = await _context.Set<T>()
                .TemporalAsOf(to)
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken)
                .ConfigureAwait(false);

            if (targetVersion is null) return Result.Failure("No entity found to revert to.");

            var currentEntity = await _context.Set<T>().FindAsync(new object[] { entityId }, cancellationToken)
                .ConfigureAwait(false);
            if (currentEntity is null) return Result.Failure("Entity not found.");

            _context.Entry(currentEntity).CurrentValues.SetValues(targetVersion);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message,ex);
        }
    }


    /// <summary>
    ///     Abstracts caching logic to get or set cache entries.
    /// </summary>
    /// <typeparam name="T">The type of data being cached.</typeparam>
    /// <param name="cacheKey">The key used for caching.</param>
    /// <param name="fetchFunction">The function to fetch data if it's not in the cache.</param>
    /// <param name="expiration">The expiration timespan for the cache entry.</param>
    /// <returns>Cached or freshly fetched data.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>?> GetOrSetCacheAsync<T>(string cacheKey,
        Func<Task<IEnumerable<TemporalRecord<T>>>> fetchFunction, TimeSpan expiration) where T : class
    {
        try
        {
            if (_cache.TryGetValue(cacheKey, out IEnumerable<TemporalRecord<T>>? cachedRecords)) return cachedRecords;
            cachedRecords = await fetchFunction().ConfigureAwait(false);
            var orSetCacheAsync = cachedRecords.ToList();
            _cache.Set(cacheKey, orSetCacheAsync, new MemoryCacheEntryOptions().SetSlidingExpiration(expiration));

            return orSetCacheAsync;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Fetches all history from the database for a given entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All historical records for the entity type.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>> FetchAllHistoryFromDatabase<T>(
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var getResult = await _context.Set<T>()
                .TemporalAll()
                .OrderBy(e => EF.Property<DateTime>(e, "ValidFrom"))
                .Select(e => new TemporalRecord<T>
                {
                    Entity = e,
                    ValidFrom = EF.Property<DateTime>(e, "ValidFrom"),
                    ValidTo = EF.Property<DateTime>(e, "ValidTo")
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return getResult;
        }
        catch (Exception)
        {
            return Array.Empty<TemporalRecord<T>>();
        }
    }


    /// <summary>
    ///     Fetches history from the database for a given entity type within the specified time range.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start of the time range.</param>
    /// <param name="to">The end of the time range.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task representing the asynchronous operation, containing temporal records within the specified time range.</returns>
    private async Task<IEnumerable<TemporalRecord<T>>> FetchHistoryFromDatabase<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var getResult = await _context.Set<T>()
                .TemporalBetween(from, to)
                .OrderBy(e => EF.Property<DateTime>(e, "ValidFrom"))
                .Select(e => new TemporalRecord<T>
                {
                    Entity = e,
                    ValidFrom = EF.Property<DateTime>(e, "ValidFrom"),
                    ValidTo = EF.Property<DateTime>(e, "ValidTo")
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return getResult;
        }
        catch (Exception)
        {
            return Array.Empty<TemporalRecord<T>>();
        }
    }
}
