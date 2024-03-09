using DropBear.Codex.TemporalHistory.Exceptions;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Data;

/// <summary>
///     Represents a database context with auditing capabilities for entities. This context intercepts save operations to
///     include audit logging, supporting both synchronous and asynchronous saves.
/// </summary>
public abstract class AuditableDbContext : DbContext
{
    private readonly AuditService _auditService;

    /// <summary>
    ///     Initializes a new instance of the AuditableDbContext class with the specified options and audit service.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    /// <param name="auditService">The audit service responsible for processing audit logs.</param>
    protected AuditableDbContext(DbContextOptions options, AuditService auditService)
        : base(options) => _auditService = auditService;

    /// <summary>
    ///     Saves all changes made in this context to the database with auditing.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     A value indicating whether all changes should be accepted upon successfully
    ///     saving to the database.
    /// </param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="AuditException">Thrown when an error occurs during the auditing process.</exception>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            var entries = _auditService.PrepareSaveChanges();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            _auditService.CompleteSaveChanges(entries);
            return result;
        }
        catch (Exception ex)
        {
            throw new AuditException("An error occurred during the audit process in SaveChanges.", ex);
        }
    }

    /// <summary>
    ///     Asynchronously saves all changes made in this context to the database with auditing.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">
    ///     A value indicating whether all changes should be accepted upon successfully
    ///     saving to the database.
    /// </param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    /// <exception cref="AuditException">Thrown when an error occurs during the auditing process.</exception>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await _auditService.PrepareSaveChangesAsync(cancellationToken).ConfigureAwait(false);
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                .ConfigureAwait(false);
            await _auditService.CompleteSaveChangesAsync(entries, cancellationToken).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            throw new AuditException("An error occurred during the audit process in SaveChangesAsync.", ex);
        }
    }
}
