using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class DbContextExtensions
{
    public static void LogChanges(this DbContext context, AuditService auditService)
    {
        var auditEntries = context.GenerateAuditEntries();
        auditService.SaveAuditEntries(auditEntries);
    }

    private static List<AuditEntry> GenerateAuditEntries(this DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged && e.Entity is IAuditableEntry).ToList();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in entries)
        {
            var auditableEntity = entry.Entity as IAuditableEntry;
            if (auditableEntity == null) continue; // Skip if not implementing IAuditableEntry

            // Use the GetIdSelector method to dynamically access the EntityId
            var entityIdExpression = auditableEntity.GetIdSelector();
            var compiled = entityIdExpression.Compile();
            var entityId = (Guid)compiled.DynamicInvoke();

            var auditEntry = new AuditEntry(entry.Entity, entry.State, entityId, auditableEntity.LastModifiedBy,
                auditableEntity.LastModifiedAt);

            // Assuming a method to populate changes exists
            PopulateChanges(entry, auditEntry);

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

// Example method to populate property changes
    private static void PopulateChanges(EntityEntry entry, AuditEntry auditEntry)
    {
        foreach (var property in entry.Properties)
            if (property.IsModified)
                auditEntry.AddChange(property.Metadata.Name, property.OriginalValue, property.CurrentValue);
    }


    private static Guid EvaluateIdSelector(Expression<Func<object>> idSelectorExpr)
    {
        // Compile and invoke the expression to get the ID value
        var idValue = idSelectorExpr.Compile().Invoke();
        // Ensure the ID value is a Guid and return it
        return (Guid)idValue;
    }
}
