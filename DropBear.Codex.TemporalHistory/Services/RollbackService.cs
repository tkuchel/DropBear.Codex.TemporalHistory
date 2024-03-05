using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Bases;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

public class RollbackService<T> where T : TemporalEntityBase, new()
{
    private readonly DbContext _context;
    private readonly TemporalQueryService<T> _temporalQueryService;

    public RollbackService(DbContext context, TemporalQueryService<T> temporalQueryService)
    {
        _context = context;
        _temporalQueryService = temporalQueryService;
    }

    public void RollbackTo<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime rollbackDate)
    {
        // Assuming RollbackTo uses the same TKey for the entity's ID
        var historicalStates = _temporalQueryService
            .GetHistoryForKey(idSelector, entityId, rollbackDate, rollbackDate.AddDays(1)).ToList();
        var historicalState = historicalStates.FirstOrDefault();

        if (historicalState != null)
        {
            // Assuming there's a method to update the current entity with the historical state
            // This part of the implementation would depend on the specifics of how entities are updated in your application
            UpdateEntityWithHistoricalState(entityId, historicalState);
            _context.SaveChanges();
        }
    }

    private void UpdateEntityWithHistoricalState<TKey>(TKey entityId, T historicalState)
    {
        // Example update logic, this would need to be implemented based on your application's requirements
        var entity = _context.Set<T>().Find(entityId);
        if (entity != null)
        {
            // Apply historical state to the current entity
            // This might involve setting properties individually or using a more automated mapping approach
        }
    }
}
