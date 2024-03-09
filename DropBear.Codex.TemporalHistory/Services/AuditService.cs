using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to create and save audit entries for entity changes to the database asynchronously and
///     synchronously.
/// </summary>
public class AuditService
{
    private readonly AuditContext _auditContext;
    private readonly DbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditService" /> class.
    /// </summary>
    /// <param name="context">The database context to track changes and save audit entries.</param>
    /// <param name="auditContext">The audit context containing information about the current audit operation.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if either <paramref name="context" /> or
    ///     <paramref name="auditContext" /> is null.
    /// </exception>
    public AuditService(DbContext context, AuditContext auditContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _auditContext = auditContext ?? throw new ArgumentNullException(nameof(auditContext));
    }

    /// <summary>
    ///     Prepares audit entries for changes detected in the DbContext asynchronously.
    /// </summary>
    /// <returns>A list of <see cref="AuditEntry" /> objects representing the changes to be audited.</returns>
    public IEnumerable<AuditEntry> PrepareSaveChanges()
    {
        var auditEntries = new List<AuditEntry>();
        _context.ChangeTracker.DetectChanges();

        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditableEntry auditableEntity || entry.State is EntityState.Detached ||
                entry.State is EntityState.Unchanged)
                continue;

            // Directly evaluate the ID selector expression within the LINQ query
            var entityIdExpression = auditableEntity.GetIdSelector();
            var entityId = entityIdExpression.Compile().Invoke();
            var auditEntry = new AuditEntry(entry.Entity.GetType(), entry.State, (Guid)entityId);

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

            auditEntries.Add(auditEntry);
        }

        return auditEntries.ToReadOnlyCollection();
    }

    /// <summary>
    ///     Prepares audit entries for changes detected in the DbContext asynchronously.
    /// </summary>
    /// <returns>A list of <see cref="AuditEntry" /> objects representing the changes to be audited.</returns>
    /// <summary>
    ///     Prepares audit entries for changes detected in the DbContext asynchronously.
    /// </summary>
    /// <returns>A list of <see cref="AuditEntry" /> objects representing the changes to be audited.</returns>
    public async Task<List<AuditEntry>> PrepareSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = await Task.Run(() =>
        {
            var entries = new List<AuditEntry>();
            _context.ChangeTracker.DetectChanges();

            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.Entity is not IAuditableEntry auditableEntity || entry.State is EntityState.Detached ||
                    entry.State is EntityState.Unchanged)
                    continue;

                var entityIdExpression = auditableEntity.GetIdSelector();
                var entityId =
                    entityIdExpression.Compile().Invoke(); // Consider caching compiled expressions if possible
                var auditEntry = new AuditEntry(entry.Entity.GetType(), entry.State, (Guid)entityId);

                foreach (var property in entry.Properties)
                {
                    var propertyInfo = property.Metadata.PropertyInfo;
                    if (propertyInfo?.GetCustomAttribute<DoNotLogAttribute>() is not null)
                        continue;

                    if (property.IsModified)
                        auditEntry.AddChange(property.Metadata.Name, property.OriginalValue, property.CurrentValue);
                }

                entries.Add(auditEntry);
            }

            return entries;
        }, cancellationToken).ConfigureAwait(false);

        return auditEntries;
    }

    /// <summary>
    ///     Completes the auditing process by saving the prepared audit entries to the database asynchronously.
    /// </summary>
    /// <param name="auditEntries">The list of audit entries to be saved.</param>
    /// <param name="cancellationToken"> A cancellation token to be used.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CompleteSaveChangesAsync(IEnumerable<AuditEntry> auditEntries,
        CancellationToken cancellationToken = default)
    {
        var auditLogs = auditEntries.Select(entry => new AuditLog
        {
            UserId = _auditContext.UserId,
            ChangeTime = DateTime.UtcNow,
            OperationCode = _auditContext.OperationCode,
            Reason = _auditContext.Reason ?? string.Empty,
            RecordNumber = entry.EntityId,
        }).ToList();

        await _context.Set<AuditLog>().AddRangeAsync(auditLogs, cancellationToken).ConfigureAwait(false);
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Completes the auditing process by saving the prepared audit entries to the database synchronously.
    /// </summary>
    /// <param name="auditEntries">The list of audit entries to be saved.</param>
    public void CompleteSaveChanges(IEnumerable<AuditEntry> auditEntries)
    {
        var auditLogs = auditEntries.Select(entry => new AuditLog
        {
            UserId = _auditContext.UserId,
            ChangeTime = DateTime.UtcNow,
            OperationCode = _auditContext.OperationCode,
            Reason = _auditContext.Reason ?? string.Empty,
            RecordNumber = entry.EntityId,
        }).ToList();

        _context.Set<AuditLog>().AddRange(auditLogs);
        _context.SaveChanges();
    }
}
