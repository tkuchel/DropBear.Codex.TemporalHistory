using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory;

/// <summary>
/// Builder for configuring and creating an instance of TemporalHistoryManager.
/// </summary>
public class TemporalManagerBuilder<TContext> where TContext : DbContext
{
    private IDbContextFactory<TContext>? _contextFactory;

    /// <summary>
    /// Sets the database context factory for the TemporalHistoryManager.
    /// </summary>
    /// <param name="contextFactory">The database context factory.</param>
    /// <returns>The builder instance for chaining.</returns>
    public TemporalManagerBuilder<TContext> UseDbContextFactory(IDbContextFactory<TContext> contextFactory)
    {
        _contextFactory = contextFactory;
        return this;
    }

    /// <summary>
    /// Builds and returns a configured instance of TemporalHistoryManager.
    /// </summary>
    /// <returns>A fully configured instance of TemporalHistoryManager.</returns>
    public ITemporalHistoryManager<TContext> Build()
    {
        ValidateConfiguration();
        return new TemporalHistoryManager<TContext>(_contextFactory ??
                                                    throw new InvalidOperationException(
                                                        "DbContextFactory must be configured."));
    }

    /// <summary>
    /// Validates the current configuration before building the manager.
    /// </summary>
    private void ValidateConfiguration()
    {
        if (_contextFactory is null)
        {
            throw new InvalidOperationException("DbContextFactory must be configured.");
        }
    }
}
