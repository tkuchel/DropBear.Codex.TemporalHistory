using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Interfaces;

namespace DropBear.Codex.TemporalHistory.ConsoleApp.Models;

public class Order : IAuditableEntry
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public Expression<Func<object>> GetIdSelector()
    {
        return () => Guid.NewGuid();
    }
}