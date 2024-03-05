using System.Linq.Expressions;

namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the required properties and methods for an auditable entity.
/// </summary>
public interface IAuditableEntry
{
    /// <summary>
    ///     Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    Guid LastModifiedBy { get; set; }

    /// <summary>
    ///     Gets or sets the timestamp when the entity was last modified.
    /// </summary>
    DateTime LastModifiedAt { get; set; }

    /// <summary>
    ///     Returns an expression that selects the entity's identifier.
    /// </summary>
    /// <returns>An expression selecting the entity's ID.</returns>
    Expression<Func<object>> GetIdSelector();
}

// public class MyEntity : TemporalEntityBase, IAuditableEntity
// {
//     public Guid Id { get; set; }
//
//     public Expression<Func<object>> GetIdSelector()
//     {
//         return () => this.Id;
//     }
// }
