using System.Linq.Expressions;
using System.Reflection;
using DropBear.Codex.TemporalHistory.Bases;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to rollback an entity to its historical state.
/// </summary>
/// <typeparam name="T">The type of the entity to rollback, constrained to TemporalEntityBase.</typeparam>
public class RollbackService<T> where T : TemporalEntityBase, new()
{
    private readonly DbContext _context;
    private readonly TemporalQueryService<T> _temporalQueryService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RollbackService{T}" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="temporalQueryService">The service used for temporal queries.</param>
    public RollbackService(DbContext context, TemporalQueryService<T> temporalQueryService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _temporalQueryService = temporalQueryService ?? throw new ArgumentNullException(nameof(temporalQueryService));
    }

    /// <summary>
    ///     Rollbacks the entity identified by <paramref name="entityId" /> to its state at <paramref name="rollbackDate" />.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The identifier of the entity to rollback.</param>
    /// <param name="rollbackDate">The date to rollback the entity to.</param>
    public void RollbackTo<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime rollbackDate)
    {
        var historicalStates = _temporalQueryService
            .GetHistoryForKey(idSelector, entityId, rollbackDate, rollbackDate.AddDays(1)).ToList();
        var historicalState = historicalStates.FirstOrDefault();

        if (historicalState != null)
        {
            UpdateEntityWithHistoricalState(entityId, historicalState);
            _context.SaveChanges();
        }
    }

    /// <summary>
    ///     Updates the current entity with its historical state.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <param name="entityId">The identifier of the entity to update.</param>
    /// <param name="historicalState">The historical state to apply to the entity.</param>
    private void UpdateEntityWithHistoricalState<TKey>(TKey entityId, T historicalState)
    {
        var entity = _context.Set<T>().Find(entityId);
        if (entity is null) return;

        // Get properties of the entity that can be written to
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod != null && p.CanWrite && p.SetMethod.IsPublic);

        foreach (var property in properties)
        {
            // Check if the historicalState has a property with the same name and it can be read
            var historicalValue = property.GetValue(historicalState);
            if (historicalValue is not null)
                // Set the value of the current entity's property to the historical value
                property.SetValue(entity, historicalValue);
        }

        // Mark the entity as modified to ensure changes are saved
        _context.Update(entity);
    }
}
