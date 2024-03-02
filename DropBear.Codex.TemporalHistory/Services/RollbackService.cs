using DropBear.Codex.TemporalHistory.DataAccess;
using DropBear.Codex.TemporalHistory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to rollback entity states to their historical versions.
/// </summary>
public class RollbackService(TemporalDbContext context) : IRollbackService
{
    private readonly TemporalDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly Dictionary<Type, HashSet<string>> _exclusions = new();

    /// <summary>
    ///     Asynchronously rolls back the state of an entity to a specified point in time.
    /// </summary>
    /// <typeparam name="T">The entity type implementing ITemporal.</typeparam>
    /// <param name="recordId">The record ID to rollback.</param>
    /// <param name="toDateTime">The point in time to rollback to.</param>
    /// <returns>A task representing the asynchronous operation, with a result indicating success.</returns>
    public async Task<bool> RollbackRecordAsync<T>(int recordId, DateTime toDateTime) where T : class, ITemporal
    {
        var historicalEntity = await _context.Set<T>()
            .TemporalAsOf(toDateTime)
            .SingleOrDefaultAsync(e => EF.Property<int>(e, "Id") == recordId)
            .ConfigureAwait(false);

        if (historicalEntity == null) return false; // Historical state not found.

        var currentEntity = await _context.Set<T>().FindAsync(recordId).ConfigureAwait(false);
        if (currentEntity == null) return false; // Current state not found.

        var properties = typeof(T).GetProperties().Where(p => p.CanWrite && !IsExcluded<T>(p.Name));
        foreach (var prop in properties)
        {
            var historicalValue = prop.GetValue(historicalEntity);
            prop.SetValue(currentEntity, historicalValue);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    /// <summary>
    ///     Adds a property name to the exclusion list for a given entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The property name to exclude from rollback operations.</param>
    /// <returns>True if the exclusion was successfully added; otherwise, false.</returns>
    public bool AddExclusion<T>(string propertyName) where T : class, ITemporal
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be null or whitespace.", nameof(propertyName));

        var type = typeof(T);
        if (!_exclusions.ContainsKey(type)) _exclusions[type] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return _exclusions[type].Add(propertyName);
    }

    /// <summary>
    ///     Checks if a property is excluded for a given entity type.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propertyName">The property name to check.</param>
    /// <returns>True if the property is excluded; otherwise, false.</returns>
    private bool IsExcluded<T>(string propertyName) where T : class, ITemporal =>
        _exclusions.TryGetValue(typeof(T), out var exclusions) && exclusions.Contains(propertyName);
}
