using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Data;

/// <summary>
///     Represents a database context with auditing capabilities for entities that implement IAuditableEntry.
/// </summary>
public abstract class AuditableDbContext : DbContext
{
    private readonly AuditService _auditService;

    /// <summary>
    ///     Initializes a new instance of the AuditableDbContext class with the specified options and audit service.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="auditService">The audit service for processing audit entries.</param>
    protected AuditableDbContext(DbContextOptions options, AuditService auditService)
        : base(options) =>
        _auditService = auditService;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // Consider adding try-catch blocks around this logic for improved error handling
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        OnAfterSaveChanges(auditEntries);
        return result;
    }

    /// <summary>
    ///     Processes entities about to be saved to generate audit entries.
    /// </summary>
    /// <returns>A list of <see cref="AuditEntry" /> objects representing the audit history.</returns>
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
            var entityId = (Guid)(compiledExpression.DynamicInvoke() ?? Guid.Empty);

            var auditEntry = new AuditEntry(
                entry.Entity,
                entry.State,
                entityId,
                auditableEntity.LastModifiedBy,
                auditableEntity.LastModifiedAt);

            foreach (var property in entry.Properties)
                if (property.IsModified)
                    auditEntry.AddChange(property.Metadata.Name, property.OriginalValue, property.CurrentValue);

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    /// <summary>
    ///     Processes the generated audit entries after changes have been saved to the database.
    /// </summary>
    /// <param name="auditEntries">The list of audit entries to process.</param>
    private static void OnAfterSaveChanges(List<AuditEntry>? auditEntries)
    {
        if (auditEntries is null || auditEntries.Count is 0)
            return;

        foreach (var auditEntry in auditEntries)
        {
            // Placeholder for logic to save audit entries
            // _auditService.Save(auditEntry); // Example call to a potentially modified AuditService to accommodate batch saving
        }
        // Consider logging successful audit saving
        // Log.Information("Successfully saved {AuditEntryCount} audit entries.", auditEntries.Count);
    }

// Note: Implementation of SaveChangesAsync should follow a similar pattern to SaveChanges, including error handling and logging.
}
