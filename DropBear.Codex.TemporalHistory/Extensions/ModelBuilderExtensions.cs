using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using DropBear.Codex.TemporalHistory.Exceptions;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extension methods for <see cref="ModelBuilder" /> for custom configurations, including temporal table
///     setup and audit log inclusion.
/// </summary>
public static class ModelBuilderExtensions
{
    private static readonly HashSet<Type> TemporalEntitiesCache = [];

    /// <summary>
    ///     Applies configuration to entities marked as temporal, setting them up with temporal table features in the database.
    ///     Checks for provider support and uses caching for improved performance.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder" /> to apply configurations to.</param>
    /// <exception cref="UnsupportedTemporalFeatureException">
    ///     Thrown if the database provider does not support temporal
    ///     features.
    /// </exception>
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder)
    {
        // Example check for provider support; implement based on actual project's data access strategy
        if (!DatabaseProviderSupportsTemporalFeatures())
            throw new UnsupportedTemporalFeatureException(
                "The current database provider does not support temporal features.");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!TemporalEntitiesCache.Contains(clrType) &&
                !Attribute.IsDefined(clrType, typeof(TemporalEntityAttribute))) continue;
            TemporalEntitiesCache.Add(clrType); // Cache for future use

            modelBuilder.Entity(clrType).ToTable(tb => tb.IsTemporal(ttb =>
            {
                // Example of enhanced naming convention
                var historyTableName = GetHistoryTableName(clrType);
                ttb.UseHistoryTable(historyTableName);
                ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
            }));
        }
    }

    /// <summary>
    ///     Ensures the <see cref="AuditLog" /> entity is included in the model, allowing it to be part of the DbContext and
    ///     used for audit logging.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder" /> to add the AuditLog entity to.</param>
    /// <returns>The updated <see cref="ModelBuilder" /> for chaining further configuration calls.</returns>
    public static ModelBuilder AddAuditLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>();
        return modelBuilder;
    }

    private static bool DatabaseProviderSupportsTemporalFeatures() =>
        // Implement custom logic for checking provider support
        // This could be based on the provider name, version, or other factors
        true;


    private static string GetHistoryTableName(MemberInfo entityType) =>
        // Implement custom logic for determining history table names
        // This could be based on conventions, attributes, or external configuration
        $"{entityType.Name}History";
}
