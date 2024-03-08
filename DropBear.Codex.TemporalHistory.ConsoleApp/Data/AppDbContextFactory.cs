using DropBear.Codex.TemporalHistory.Enums;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DropBear.Codex.TemporalHistory.ConsoleApp.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string LaptopConnectionString =
        "Server=TDOG-STRIX-15\\SQLEXPRESS;Initial Catalog=TemporalHistory;Integrated Security=true;Encrypt=True;TrustServerCertificate=True;";

    private const string TDogDevVmConnectionString =
        "Server=TDOG-DEV-VM\\TDOGSQLSERVER;Initial Catalog=TemporalHistory;Integrated Security=true;Encrypt=True;TrustServerCertificate=True;";
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(LaptopConnectionString);

        // Create and configure an AuditContext as needed
        var auditContext = new AuditContext
        {
            UserId = Guid.NewGuid(), // Example setup, adjust as necessary
            Reason = "Design-time operations",
            OperationCode = OperationCode.Update // Example operation code
        };

        // Instantiate and return AppDbContext with required parameters
        return new AppDbContext(optionsBuilder.Options, auditContext);
    }
}