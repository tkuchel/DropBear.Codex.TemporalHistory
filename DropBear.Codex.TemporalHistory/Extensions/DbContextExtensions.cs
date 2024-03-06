using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
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

    /// <summary>
    ///     Generates a list of audit entries for all auditable entities in the DbContext that have changes.
    /// </summary>
    /// <param name="context">The DbContext containing the entities to be audited.</param>
    /// <returns>A list of <see cref="AuditEntry" /> objects representing the changes detected.</returns>
    /// <remarks>
    ///     This method iterates over all entities tracked by the DbContext that implement the IAuditableEntry interface
    ///     and have state changes other than EntityState.Unchanged. For each entity, it constructs an audit entry
    ///     that captures the entity's current state, the changes made to it, and its auditable properties,
    ///     such as the entity identifier, last modified by user ID, and the last modified timestamp.
    ///     The method uses the GetIdSelector expression defined in the IAuditableEntry interface to dynamically
    ///     extract the entity's identifier, supporting flexibility in how entities define their identifiers.
    /// </remarks>
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
    ///     Populates the audit entry with changes detected in the entity, excluding properties marked with [DoNotLog].
    /// </summary>
    /// <param name="entry">The EntityEntry object containing information about the entity's changes.</param>
    /// <param name="auditEntry">The AuditEntry object to populate with changes.</param>
    private static void PopulateChanges(EntityEntry entry, AuditEntry auditEntry)
    {
        var entityType = entry.Entity.GetType();

        foreach (var property in entry.Properties)
        {
            // Retrieve the corresponding PropertyInfo object to check for the [DoNotLog] attribute.
            var propertyInfo =
                entityType.GetProperty(property.Metadata.Name, BindingFlags.Public | BindingFlags.Instance);

            // Check if the property is marked with [DoNotLog]; if so, skip logging this property.
            if (propertyInfo != null)
            {
                var doNotLogAttribute = propertyInfo.GetCustomAttribute<DoNotLogAttribute>();
                if (doNotLogAttribute is not null) continue; // Skip this property, as it's marked with [DoNotLog].
            }

            if (property.IsModified)
                auditEntry.AddChange(
                    property.Metadata.Name,
                    property.OriginalValue,
                    property.CurrentValue);
        }
    }
}
