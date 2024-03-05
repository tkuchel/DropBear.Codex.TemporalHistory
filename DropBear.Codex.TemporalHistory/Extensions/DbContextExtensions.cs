using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class DbContextExtensions
{
    /// <summary>
    ///     Logs changes of the auditable entities within the DbContext.
    /// </summary>
    /// <param name="context">The DbContext containing the entities.</param>
    /// <param name="auditService">The service responsible for processing and storing audit entries.</param>
    public static void LogChanges(this DbContext context, AuditService auditService)
    {
        var auditEntries = context.GenerateAuditEntries();
        auditService.SaveAuditEntries(auditEntries);
    }

    private static List<AuditEntry> GenerateAuditEntries(this DbContext context) =>
        context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged && e.Entity is IAuditableEntry)
            .Select(entry =>
            {
                var auditableEntity = (IAuditableEntry)entry.Entity;

                // Directly evaluate the ID selector expression within the LINQ query
                var entityIdExpression = auditableEntity.GetIdSelector();
                var entityId = entityIdExpression.Compile().Invoke();

                var auditEntry = new AuditEntry(
                    entry.Entity,
                    entry.State,
                    (Guid)entityId, // Casting to Guid as we assume the ID is of this type
                    auditableEntity.LastModifiedBy,
                    auditableEntity.LastModifiedAt);

                PopulateChanges(entry, auditEntry);
                return auditEntry;
            }).ToList();

    /// <summary>
    ///     Populates the audit entry with changes detected in the entity.
    /// </summary>
    /// <param name="entry">The EntityEntry object containing information about the entity's changes.</param>
    /// <param name="auditEntry">The AuditEntry object to populate with changes.</param>
    private static void PopulateChanges(EntityEntry entry, AuditEntry auditEntry)
    {
        foreach (var property in entry.Properties)
            if (property.IsModified)
                auditEntry.AddChange(
                    property.Metadata.Name,
                    property.OriginalValue,
                    property.CurrentValue);
    }
}
