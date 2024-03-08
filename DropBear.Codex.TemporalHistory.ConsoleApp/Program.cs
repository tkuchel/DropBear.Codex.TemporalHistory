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
    private const string ConnectionString = "YourConnectionStringHere";
    private const string LaptopConnectionString =
        "Server=TDOG-STRIX-15\\SQLEXPRESS;Initial Catalog=TemporalHistory;Integrated Security=true;Encrypt=True;TrustServerCertificate=True;";

    private const string TDogDevVmConnectionString =
        "Server=TDOG-DEV-VM\\TDOGSQLSERVER;Initial Catalog=TemporalHistory;Integrated Security=true;Encrypt=True;TrustServerCertificate=True;";

    private static void Main(string[] args)
    {
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(TDogDevVmConnectionString);

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

        // Ensure database is created, for demonstration purposes
        dbContext.Database.EnsureCreated();

        // Add a random new record
        AddRandomProduct(dbContext);

        // Edit a random existing record
        EditRandomProduct(dbContext);

        // Log changes
        dbContext.LogChanges(auditService);

        // Save changes
        dbContext.SaveChanges();

        Console.WriteLine("Operation completed and audited.");
    }

    private static void AddRandomProduct(AppDbContext dbContext)
    {
        var random = new Random();
        var newProduct = new Product
        {
            Name = $"Random Product {random.Next(1000, 9999)}",
            Price = (decimal)(random.Next(10, 100) + random.NextDouble())
        };
        dbContext.Products.Add(newProduct);
        Console.WriteLine($"Added new product: {newProduct.Name} with price {newProduct.Price}");
    }

    private static void EditRandomProduct(AppDbContext dbContext)
    {
        var products = dbContext.Products.ToList();
        if (products.Any())
        {
            var random = new Random();
            var productToEdit = products[random.Next(products.Count)];
            var oldPrice = productToEdit.Price;
            productToEdit.Price += (decimal)(random.Next(1, 5) + random.NextDouble());
            Console.WriteLine(
                $"Edited product: {productToEdit.Name}, old price: {oldPrice}, new price: {productToEdit.Price}");
        }
        else
        {
            Console.WriteLine("No existing products to edit.");
        }
    }
}