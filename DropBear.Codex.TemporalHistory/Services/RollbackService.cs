using System.Linq.Expressions;
using System.Reflection;
using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to rollback an entity to its historical state using temporal tables.
/// </summary>
/// <typeparam name="T">The type of the entity to rollback.</typeparam>
public class RollbackService<T>(
    DbContext context,
    TemporalQueryService<T> temporalQueryService,
    IAppLogger<RollbackService<T>> logger)
    where T : class
{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private readonly IAppLogger<RollbackService<T>>
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Initialize logger
    // Logger instance

    private readonly TemporalQueryService<T> _temporalQueryService =
        temporalQueryService ?? throw new ArgumentNullException(nameof(temporalQueryService));

    /// <summary>
    ///     Asynchronously rollbacks the entity identified by <paramref name="entityId" /> to its state at
    ///     <paramref name="rollbackDate" />.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The identifier of the entity to rollback.</param>
    /// <param name="rollbackDate">The date to rollback the entity to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RollbackToAsync<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime rollbackDate)
    {
        try
        {
            var historicalStates = await _temporalQueryService
                .GetHistoryForKeyAsync(idSelector, entityId, rollbackDate, rollbackDate.AddDays(1))
                .ConfigureAwait(false);

            var historicalState = historicalStates.FirstOrDefault();

            if (historicalState is not null)
            {
                await UpdateEntityWithHistoricalStateAsync(entityId, historicalState).ConfigureAwait(false);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                ZString.Format("Failed attempting to rollback entity: {0} to {1}", entityId, rollbackDate));
            throw; // Re-throw to allow further handling up the stack if necessary.
        }
    }

    /// <summary>
    ///     Asynchronously updates the current entity with its historical state.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <param name="entityId">The identifier of the entity to update.</param>
    /// <param name="historicalState">The historical state to apply to the entity.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateEntityWithHistoricalStateAsync<TKey>(TKey entityId, T historicalState)
    {
        var entity = await _context.Set<T>().FindAsync(entityId).ConfigureAwait(false);
        if (entity is null)
        {
            _logger.LogWarning(ZString.Format("Entity with ID: {0} not found for rollback.", entityId));
            return;
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod != null && p.CanWrite && p.SetMethod.IsPublic);

        foreach (var property in properties)
        {
            var historicalValue = property.GetValue(historicalState);
            if (historicalValue is not null)
                property.SetValue(entity, historicalValue);
        }

        _context.Update(entity);
        _logger.LogInformation(ZString.Format("Entity with ID: {0} has been updated with historical state from {1}",
            entityId, historicalState));
    }


    /// <summary>
    ///     Synchronously rollbacks the entity identified by <paramref name="entityId" /> to its state at
    ///     <paramref name="rollbackDate" />.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The identifier of the entity to rollback.</param>
    /// <param name="rollbackDate">The date to rollback the entity to.</param>
    public void RollbackTo<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime rollbackDate)
    {
        try
        {
            var historicalStates = _temporalQueryService
                .GetHistoryForKey(idSelector, entityId, rollbackDate, rollbackDate.AddDays(1)).ToList();
            var historicalState = historicalStates.FirstOrDefault();

            if (historicalState is null)
            {
                _logger.LogInformation($"No historical state found for entity ID: {entityId} at {rollbackDate}");
                return;
            }

            UpdateEntityWithHistoricalState(entityId, historicalState);
            _context.SaveChanges();
            _logger.LogInformation(ZString.Format("Entity with ID: {0} has been updated with historical state from {1}",
                entityId, historicalState));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                ZString.Format("Failed attempting to rollback entity: {0} to {1}", entityId, rollbackDate));
            throw; // Consider rethrowing to allow further handling up the call stack or handle gracefully here
        }
    }

    private void UpdateEntityWithHistoricalState<TKey>(TKey entityId, T historicalState)
    {
        var entity = _context.Set<T>().Find(entityId);
        if (entity is null)
        {
            _logger.LogWarning(ZString.Format("Entity with ID: {0} not found for rollback.", entityId));
            return;
        }

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod != null && p.CanWrite && p.SetMethod.IsPublic);

        foreach (var property in properties)
        {
            var historicalValue = property.GetValue(historicalState);
            property.SetValue(entity, historicalValue);
        }

        _context.Update(entity);
    }
}
