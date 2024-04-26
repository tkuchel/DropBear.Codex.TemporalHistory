using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory;

/// <summary>
///     Builder for configuring and creating an instance of TemporalHistoryManager.
/// </summary>
public class TemporalManagerBuilder<TContext> where TContext : DbContext
{
    private TContext? _context;

    /// <summary>
    ///     Sets the database context for the TemporalHistoryManager.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TemporalManagerBuilder<TContext> UseDbContext(TContext? context)
    {
        _context = context;
        return this;
    }

    /// <summary>
    ///     Builds and returns a configured instance of TemporalHistoryManager.
    /// </summary>
    /// <returns>A fully configured instance of TemporalHistoryManager.</returns>
    public ITemporalHistoryManager<TContext> Build()
    {
        ValidateConfiguration();
        return new TemporalHistoryManager<TContext>(_context ??
                                                    throw new InvalidOperationException(
                                                        "Database context must be configured."));
    }

    /// <summary>
    ///     Validates the current configuration before building the manager.
    /// </summary>
    private void ValidateConfiguration()
    {
        if (_context is null) throw new InvalidOperationException("Database context must be configured.");
    }
}
