using DropBear.Codex.TemporalHistory.Bases;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Configures entities inheriting from TemporalEntityBase to use temporal tables with system-managed columns.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder to apply configurations to.</param>
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            if (typeof(TemporalEntityBase).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).ToTable(tb => tb.IsTemporal(ttb =>
                {
                    ttb.UseHistoryTable(
                        $"{entityType.ClrType.Name}History"); // Customizes the name of the history table
                }));
    }

    /// <summary>
    ///     Ensures the DbSet for AuditLog is included in the DbContext, allowing it to be part of the model.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder to add the AuditLog entity to.</param>
    /// <returns>The ModelBuilder for chaining.</returns>
    public static ModelBuilder AddAuditLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>();
        return modelBuilder;
    }
}
