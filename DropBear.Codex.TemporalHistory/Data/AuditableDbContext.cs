using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Data;

public abstract class AuditableDbContext : DbContext
{
    private readonly AuditService _auditService;

    protected AuditableDbContext(DbContextOptions options, AuditService auditService)
        : base(options) =>
        _auditService = auditService;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (!(entry.Entity is IAuditableEntry auditableEntity) || entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged)
                continue;

            // Retrieve the ID using the entity's GetIdSelector method
            var entityIdExpression = auditableEntity.GetIdSelector();
            var compiledExpression = entityIdExpression.Compile();
            var entityId = (Guid)compiledExpression.DynamicInvoke();

            // Instantiate AuditEntry with all required parameters
            var auditEntry = new AuditEntry(
                entry.Entity,
                entry.State,
                entityId,
                auditableEntity.LastModifiedBy,
                auditableEntity.LastModifiedAt);

            // Populate changes
            foreach (var property in entry.Properties)
                if (property.IsModified)
                {
                    var originalValue = property.OriginalValue;
                    var currentValue = property.CurrentValue;
                    auditEntry.AddChange(property.Metadata.Name, originalValue, currentValue);
                }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }


    private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            // Logic to save audit entries
            // You may need to adjust AuditService to accommodate batch saving of audit entries
        }

        // This might be a place to call AuditService, or directly save audit entries if AuditService is adjusted accordingly
    }


    // Similar override for SaveChangesAsync

    private List<AuditEntry> GenerateAuditEntries() =>
        // Logic to generate AuditEntry objects
        new();
}

// public class MyDbContext : AuditableDbContext
// {
//     public MyDbContext(DbContextOptions<MyDbContext> options, AuditService auditService)
//         : base(options, auditService)
//     {
//     }
//
//     // DbSets and other DbContext configurations
// }
