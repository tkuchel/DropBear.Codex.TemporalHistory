using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Bases;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides services for querying the historical states of temporal entities.
/// </summary>
/// <typeparam name="T">The type of temporal entity.</typeparam>
public class TemporalQueryService<T> where T : TemporalEntityBase
{
    private readonly DbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporalQueryService{T}" />.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TemporalQueryService(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    ///     Retrieves the historical states for an entity identified by a key within a specified date range.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
    /// <param name="idSelector">An expression to select the entity key.</param>
    /// <param name="entityId">The value of the key for the entity.</param>
    /// <param name="from">The start date of the historical period.</param>
    /// <param name="to">The end date of the historical period.</param>
    /// <returns>An enumerable collection of entity states within the specified period.</returns>
    public IEnumerable<T> GetHistoryForKey<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime from,
        DateTime to)
    {
        // Dynamically build a LINQ query to filter by the ID and the valid period
        var entityParam = idSelector.Parameters.First();
        var keyComparison = Expression.Equal(idSelector.Body, Expression.Constant(entityId));
        var dateFilter = Expression.AndAlso(
            Expression.GreaterThanOrEqual(Expression.Property(entityParam, nameof(TemporalEntityBase.ValidFrom)),
                Expression.Constant(from)),
            Expression.LessThanOrEqual(Expression.Property(entityParam, nameof(TemporalEntityBase.ValidTo)),
                Expression.Constant(to))
        );

        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(keyComparison, dateFilter),
            entityParam
        );

        return _context.Set<T>().Where(predicate).ToList();
    }

    /// <summary>
    ///     Compares two versions of an entity based on timestamps and identifies property changes.
    /// </summary>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The entity's identifier value.</param>
    /// <param name="firstDate">The timestamp for the first version to compare.</param>
    /// <param name="secondDate">The timestamp for the second version to compare.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>A collection of PropertyChange detailing differences between the two versions.</returns>
    public IEnumerable<PropertyChange> CompareEntityVersions<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId,
        DateTime firstDate, DateTime secondDate)
    {
        if (firstDate >= secondDate) throw new ArgumentException("firstDate must be less than {secondDate}.",
            nameof(firstDate));

        var firstState = GetHistoryForKey(idSelector, entityId, firstDate, firstDate.AddDays(1)).FirstOrDefault();
        var secondState = GetHistoryForKey(idSelector, entityId, secondDate, secondDate.AddDays(1)).FirstOrDefault();

        if (firstState is null || secondState is null) return Enumerable.Empty<PropertyChange>();

        var differences = typeof(T).GetProperties()
            .Where(prop => prop.CanRead)
            .Select(prop => new PropertyChange(prop.Name,
                prop.GetValue(firstState),
                prop.GetValue(secondState)))
    .Where(change => !Equals(change.OriginalValue, change.CurrentValue))
            .ToList();

        return differences;
    }

    /// <summary>
    ///     Retrieves entities that have remained in a specified state for at least the given duration.
    /// </summary>
    /// <param name="stateExpression">An expression defining the state to check.</param>
    /// <param name="minimumDuration">The minimum duration an entity must have remained in the state.</param>
    /// <returns>An enumerable of entities matching the criteria.</returns>
    public IEnumerable<T> GetEntitiesByStateDuration(Expression<Func<T, bool>> stateExpression,
        TimeSpan minimumDuration)
    {
        if (minimumDuration <= TimeSpan.Zero)
            throw new ArgumentException("minimumDuration must be greater than zero.", nameof(minimumDuration));

        return _context.Set<T>()
            .AsEnumerable() // Potentially heavy operation, consider optimizing
            .Where(entity =>
                stateExpression.Compile().Invoke(entity) &&
                entity.ValidTo - entity.ValidFrom >= minimumDuration)
            .ToList();
    }

    /// <summary>
    ///     Counts the number of changes for an entity within a specified date range.
    /// </summary>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The entity's identifier value.</param>
    /// <param name="from">The start date of the range to count changes.</param>
    /// <param name="to">The end date of the range to count changes.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>The number of changes made to the entity within the date range.</returns>
    public int GetChangeFrequency<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime from, DateTime to)
    {
        if (from >= to) throw new ArgumentException($"The start date must be before the end date.", nameof(from));


        return GetHistoryForKey(idSelector, entityId, from, to).Count();
    }

    /// <summary>
    ///     Identifies entities that match a specified sequence of patterns within a date range.
    /// </summary>
    /// <param name="patterns">A list of expressions defining the patterns to match against entity states.</param>
    /// <param name="from">The start date for matching patterns.</param>
    /// <param name="to">The end date for matching patterns.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>An enumerable of entities matching the specified patterns within the date range.</returns>
    public IEnumerable<T> GetEntitiesMatchingPattern(List<Expression<Func<T, bool>>> patterns, DateTime from,
        DateTime to)
    {
        if (patterns is null || patterns.Count is 0)
            throw new ArgumentException("Patterns list cannot be null or empty.", nameof(patterns));
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        var matchingEntities = new List<T>();

        foreach (var matched in patterns.Select(pattern => _context.Set<T>()
                     .AsEnumerable() // Consider optimization
                     .Where(entity => pattern.Compile().Invoke(entity))
                     .ToList()))
        {
            matchingEntities.AddRange(matched);
        }

        return matchingEntities.Distinct().ToList();
    }
}
