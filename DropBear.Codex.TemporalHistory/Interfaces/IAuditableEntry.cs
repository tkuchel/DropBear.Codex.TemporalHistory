using System.Linq.Expressions;

namespace DropBear.Codex.TemporalHistory.Interfaces;

public interface IAuditableEntry
{
    Guid LastModifiedBy { get; set; }
    DateTime LastModifiedAt { get; set; }
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
