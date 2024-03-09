using System.Linq.Expressions;

namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the required properties and methods for an auditable entity.
/// </summary>
public interface IAuditableEntry
{
    /// <summary>
    ///     Returns an expression that selects the entity's identifier.
    /// </summary>
    /// <returns>An expression selecting the entity's ID.</returns>
    Expression<Func<object>> GetIdSelector();
}
