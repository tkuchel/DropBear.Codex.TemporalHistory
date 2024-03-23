using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Core.ReturnTypes;
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
    private readonly IAppLogger<TemporalHistoryService<TContext>> _logger;

    /// <summary>
    ///     Initializes a new instance of the TemporalHistoryService with the specified DbContext and MemoryCache.
    /// </summary>
    /// <param name="context">The DbContext to use for data operations.</param>
    /// <param name="cache">The MemoryCache for caching query results.</param>
    /// <param name="logger">An App Logger implementation</param>
    public TemporalHistoryService(TContext context, IMemoryCache cache,
        IAppLogger<TemporalHistoryService<TContext>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                TimeSpan.FromMinutes(5),
                cancellationToken
            ).ConfigureAwait(false);

            return getResult is not null
                ? Result<IEnumerable<TemporalRecord<T>>>.Success(getResult)
                : Result<IEnumerable<TemporalRecord<T>>>.Failure("No history found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                ZString.Format("An error occurred while fetching history for entity type {EntityType}.",
                    typeof(T).Name));
            return Result<IEnumerable<TemporalRecord<T>>>.Failure("An error occurred while fetching history.");
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
                TimeSpan.FromMinutes(10), cancellationToken).ConfigureAwait(false);

            return getResult is not null
                ? Result<IEnumerable<TemporalRecord<T>>>.Success(getResult)
                : Result<IEnumerable<TemporalRecord<T>>>.Failure("No history found");
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                ZString.Format("An error occurred while fetching all history for entity type {EntityType}.",
                    typeof(T).Name));
            return Result<IEnumerable<TemporalRecord<T>>>.Failure("An error occurred while fetching history.");
        }
    }

    /// <summary>
    ///     Rolls back the state of entities to a specific point in time, applying the historical state as the current state.
    /// </summary>
    /// <typeparam name="T">The entity type to rollback.</typeparam>
    /// <param name="to">The point in time to rollback the entity states to.</param>
    /// <param name="cancellationToken">A token for canceling the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating success or failure.</returns>
    public async Task<Result> RollbackAsync<T>(DateTime to, CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            await using (transaction.ConfigureAwait(false))
            {
                var entities = await _context.Set<T>().TemporalAsOf(to).ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (entities.Count is 0) return Result.Failure("No entities found to rollback.");

                foreach (var entity in entities) _context.Entry(entity).State = EntityState.Modified; // Mark for update

                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                ZString.Format("An error occurred while rolling back entity type {EntityType}.", typeof(T).Name));
            return Result.Failure("An error occurred while rolling back entities.");
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
            _logger.LogError(ex,
                ZString.Format("An error occurred while fetching previous version for entity with ID {EntityId}.",
                    entityId));
            return Result<T>.Failure("An error occurred while fetching previous version.");
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
        catch (Exception e)
        {
            _logger.LogError(e,
                ZString.Format("An error occurred while fetching next version for entity with ID {EntityId}.",
                    entityId));
            return Result<T>.Failure("An error occurred while fetching next version.");
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
        catch (Exception e)
        {
            _logger.LogError(e,
                ZString.Format("An error occurred while reverting entity with ID {EntityId} to version at {To}.",
                    entityId, to));
            return Result.Failure("An error occurred while reverting entity.");
        }
    }


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
        try
        {
            if (_cache.TryGetValue(cacheKey, out IEnumerable<TemporalRecord<T>>? cachedRecords)) return cachedRecords;
            cachedRecords = await fetchFunction().ConfigureAwait(false);
            _cache.Set(cacheKey, cachedRecords, new MemoryCacheEntryOptions().SetSlidingExpiration(expiration));

            return cachedRecords;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while fetching or setting cache.");
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
