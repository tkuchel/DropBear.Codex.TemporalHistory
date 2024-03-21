using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.TemporalHistory.ConsoleApp.Data;
using DropBear.Codex.TemporalHistory.ConsoleApp.Models;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddTransient<Application>();

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
    private readonly IAppLogger<Application> _logger;
    private readonly RollbackService<Product> _rollbackService;
    private readonly TemporalQueryService<Product> _temporalQueryService;

    public Application(IAppLogger<Application> logger, AppDbContext dbContext,
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

            _logger.LogInformation(ZString.Format("Added new product: {0}", newProduct.Name));
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

            _logger.LogInformation(ZString.Format("Edited product: {0}, Old Price: {1}, New Price: {2}",
                productToEdit.Name, oldPrice, productToEdit.Price));
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
                 productId, firstDate, secondDate);

            if (changes != null)
                foreach (var change in changes)
                    _logger.LogInformation(ZString.Format("Property: {0}, Old Value: {1}, New Value: {2}",
                        change.PropertyName, change.OriginalValue, change.CurrentValue));
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

            _logger.LogInformation(ZString.Format("Product {0} has been changed {1} times.", productId, historyCount));
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

            _logger.LogInformation(ZString.Format("Product {0} has been rolled back to {1}.", productId, rollbackDate));
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
            _logger.LogError(ex, ZString.Format("Error fetching change timestamps for product: {0}", productId));
            throw;
        }
    }
}