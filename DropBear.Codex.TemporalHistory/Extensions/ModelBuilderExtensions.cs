using DropBear.Codex.TemporalHistory.Bases;
using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Extensions;

// Path/Filename: ModelBuilderExtensions.cs
using Microsoft.EntityFrameworkCore;
using System;

public static class ModelBuilderExtensions
{
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TemporalEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("ValidFrom")
                    .HasColumnName("SysStartTime")
                    .IsRequired();

                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTime>("ValidTo")
                    .HasColumnName("SysEndTime")
                    .IsRequired();
            }
        }
    }


    // Example method to ensure DbSet for AuditLog is added to the DbContext
    public static ModelBuilder AddAuditLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>();
        return modelBuilder;
    }
}
