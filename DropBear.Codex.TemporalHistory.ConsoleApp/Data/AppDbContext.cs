using DropBear.Codex.TemporalHistory.ConsoleApp.Models;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.ConsoleApp.Data;

public class AppDbContext : DbContext
{
    private readonly AuditContext _auditContext;
    private AuditService _auditService;

    public AppDbContext(DbContextOptions<AppDbContext> options, AuditContext auditContext)
        : base(options)
    {
        _auditContext = auditContext ?? throw new ArgumentNullException(nameof(auditContext));
    }

    // Use _auditService where needed, ensure it's set before using
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    public void SetAuditService(AuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    // Other DbContext setup...


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure the 'Price' property of 'Product' entity
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18, 4)"); // Example: sets precision to 18 and scale to 4

        modelBuilder.ApplyTemporalTableConfiguration();
        modelBuilder.AddAuditLog();
    }
}