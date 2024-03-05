using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Bases;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

public class TemporalQueryService<T> where T : TemporalEntityBase
{
    private readonly DbContext _context;

    public TemporalQueryService(DbContext context) => _context = context;

    public IEnumerable<T> GetHistoryForKey<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime from,
        DateTime to)
    {
        // Use the expression to create a Func<T, bool> to be used in the Where clause
        var predicate = PredicateBuilder(idSelector, entityId);

        return _context.Set<T>()
            .Where(predicate)
            .Where(e => e.ValidFrom >= from && e.ValidTo <= to)
            .ToList();
    }

    private Func<T, bool> PredicateBuilder<TKey>(Expression<Func<T, TKey>> idSelector, TKey key)
    {
        // Combine the idSelector expression with a comparison to the specified key
        var parameter = Expression.Parameter(typeof(T), "e");
        var body = Expression.Equal(
            Expression.Invoke(idSelector, parameter),
            Expression.Constant(key, typeof(TKey))
        );
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
        return lambda.Compile(); // Compile the expression into a Func<T, bool>
    }
}

// var service = new TemporalQueryService<MyEntity>(context);
// var history = service.GetHistoryForKey(e => e.Id, myEntityId, startDate, endDate);
