using DropBear.Codex.TemporalHistory.ConsoleApp.Data;
using DropBear.Codex.TemporalHistory.ConsoleApp.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.TemporalHistory.ConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();

        // Retrieve services from DI container
        var app = serviceProvider.GetRequiredService<Application>();
        await app.RunAsync();
    }

    private static IServiceCollection ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        // Configure logging
        services.AddLogging(configure => configure.AddConsole())
            .AddTransient<Application>();

        // Add EF DbContext with options
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer("YourConnectionStringHere"));

        // Register other services
        services.AddScoped(typeof(AuditService));
        services.AddScoped(typeof(TemporalQueryService<>));
        services.AddScoped(typeof(RollbackService<>));

        return services;
    }
}

public class Application
{
    private readonly AuditService _auditService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<Application> _logger;
    private readonly RollbackService<Product> _rollbackService;
    private readonly TemporalQueryService<Product> _temporalQueryService;

    public Application(ILogger<Application> logger, AppDbContext dbContext,
        AuditService auditService, TemporalQueryService<Product> temporalQueryService,
        RollbackService<Product> rollbackService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _auditService = auditService;
        _temporalQueryService = temporalQueryService;
        _rollbackService = rollbackService;
    }

    public async Task RunAsync()
    {
        try
        {
            await SetupDatabaseAsync();
            await PerformDemoOperationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the application run.");
        }
    }

    private async Task SetupDatabaseAsync()
    {
        // Ensure database created
        await _dbContext.Database.EnsureCreatedAsync();
    }

    private async Task PerformDemoOperationsAsync()
    {
        // Demonstration of adding, editing, and querying for products
        await AddRandomProductAsync();
        await EditRandomProductAsync();

        // Demonstration of temporal queries
        await CompareEntityVersionsAsync();
        await GetChangeFrequencyAsync();

        // Demonstration of rollback functionality
        await DemonstrateRollbackOperationAsync();
    }

    private async Task AddRandomProductAsync()
    {
        try
        {
            var random = new Random();
            var newProduct = new Product
            {
                Name = $"Random Product {random.Next(1000, 9999)}",
                Price = (decimal)(random.Next(10, 100) + random.NextDouble())
            };

            await _dbContext.Products.AddAsync(newProduct);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Added new product: {newProduct.Name} with price {newProduct.Price}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding a new product asynchronously.");
        }
    }


    private async Task EditRandomProductAsync()
    {
        try
        {
            var products = await _dbContext.Products.ToListAsync();
            if (!products.Any())
            {
                _logger.LogInformation("No existing products to edit.");
                return;
            }

            var random = new Random();
            var productToEdit = products[random.Next(products.Count)];
            var oldPrice = productToEdit.Price;
            productToEdit.Price += (decimal)(random.Next(1, 5) + random.NextDouble());

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                $"Edited product: {productToEdit.Name}, old price: {oldPrice}, new price: {productToEdit.Price}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing a product asynchronously.");
        }
    }


    private async Task CompareEntityVersionsAsync()
    {
        try
        {
            var productId = await _dbContext.Products
                .OrderBy(p => Guid.NewGuid())
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (productId == default)
            {
                _logger.LogInformation("No products found in the database.");
                return;
            }

            // Assuming method exists to fetch change timestamps
            var timestamps = await FetchChangeTimestampsAsync(productId);

            if (timestamps.Count < 2)
            {
                _logger.LogInformation("Not enough history for comparison.");
                return;
            }

            var firstDate = timestamps.First();
            var secondDate = timestamps.Last();

            var changes = await _temporalQueryService.CompareEntityVersionsAsync(
                p => p.Id, productId, firstDate, secondDate);

            foreach (var change in changes)
                _logger.LogInformation(
                    $"Property: {change.PropertyName}, Original: {change.OriginalValue}, Current: {change.CurrentValue}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing entity versions.");
        }
    }


    private async Task GetChangeFrequencyAsync()
    {
        try
        {
            var productId = Guid.Empty;

            // Assuming your temporal table is configured and you're using EF Core 6 or later which supports temporal queries
            var historyCount = await _dbContext.Products
                .TemporalAll() // Or use TemporalFromTo(from, to) if filtering by a specific range
                .Where(p => p.Id == productId)
                .CountAsync();

            _logger.LogInformation($"Product {productId} underwent {historyCount} changes.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate change frequency for product.");
        }
    }

    private async Task DemonstrateRollbackOperationAsync()
    {
        try
        {
            var productId = await _dbContext.Products
                .OrderBy(p => Guid.NewGuid())
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (productId == default)
            {
                _logger.LogInformation("No products found for rollback demonstration.");
                return;
            }

            var rollbackDate = DateTime.UtcNow.AddDays(-new Random().Next(1, 30));
            await _rollbackService.RollbackToAsync(p => p.Id, productId, rollbackDate);

            _logger.LogInformation($"Product {productId} has been rolled back to {rollbackDate}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to demonstrate rollback operation.");
        }
    }


    private async Task<List<DateTime>> FetchChangeTimestampsAsync(Guid productId)
    {
        try
        {
            // Fetching change timestamps using PeriodStart from the temporal table
            var timestamps = await _dbContext.Products
                .TemporalAll()
                .Where(p => p.Id == productId)
                .Select(p => EF.Property<DateTime>(p, "PeriodStart")) // Directly accessing PeriodStart
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            return timestamps;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch change timestamps for product {ProductId}", productId);
            throw;
        }
    }
}