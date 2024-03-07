using DropBear.Codex.TemporalHistory.ConsoleApp.Data;
using DropBear.Codex.TemporalHistory.ConsoleApp.Models;
using DropBear.Codex.TemporalHistory.Enums;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.ConsoleApp;

internal class Program
{
    private static void Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer("Server=TDOG-DEV-VM\\TDOGSQLSERVER;Initial Catalog=TemporalHistory;Integrated Security=true;Encrypt=True;TrustServerCertificate=True;");

        var auditContext = new AuditContext
        {
            UserId = Guid.NewGuid(),
            Reason = "Console App Test",
            OperationCode = OperationCode.Update
        };

        using var dbContext = new AppDbContext(optionsBuilder.Options, auditContext);

        // Create AuditService after DbContext instantiation
        var auditService = new AuditService(dbContext, auditContext);

        // Set AuditService into DbContext
        dbContext.SetAuditService(auditService);

        // Now you can use dbContext and auditService as needed

        // Ensure database is created, for demonstration purposes
        dbContext.Database.EnsureCreated();


        // Execute an example operation
        var product = new Product { Name = "Example Product", Price = 9.99M };
        dbContext.Products.Add(product);
        dbContext.SaveChanges();

        // Log changes
        dbContext.LogChanges(auditService);

        Console.WriteLine("Operation completed and audited.");
    }
}