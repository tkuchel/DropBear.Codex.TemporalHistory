using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Interfaces;

namespace DropBear.Codex.TemporalHistory.ConsoleApp.Models;

public class Product : IAuditableEntry
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public Guid LastModifiedBy { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public Expression<Func<object>> GetIdSelector()
    {
        return () => Guid.NewGuid();
    }
}