# DropBear.Codex.TemporalHistory Library

A comprehensive library for managing temporal data and audit logging in .NET applications using Entity Framework Core.

## Features

- Temporal Data Tracking: Automatically track historical changes to entities in your database.
- Audit Logging: Capture audit logs for entity changes, including who made the change and why.
- Flexible Configuration: Configure global and entity-specific auditing settings to suit your application's needs.
- Extension Methods: Easily apply configurations to your DbContext without inheritance.

## Getting Started

### Installation

First, add the TemporalHistory library to your project using NuGet:

```csharp
dotnet add package DropBear.Codex.TemporalHistory 
```

### Configuration

1. Service Registration: Register the necessary services and configurations in your Startup.cs or wherever you configure services.

```csharp
public void ConfigureServices(IServiceCollection services) {  services.AddTemporalHistory(config =>  {  config.GetCurrentUserIdFunc = () => "Implement logic to get current user ID";  config.GetChangeReasonFunc = (entity) => "Implement logic to determine change reason";  }); } 
```
2. DbContext Setup: If creating a new DbContext, inherit from TemporalDbContext. For existing DbContexts, use the provided extension method.

- Inheriting from TemporalDbContext:

```csharp
public class MyDbContext : TemporalDbContext {  public MyDbContext(AuditableConfig auditableConfig, ILogger<TemporalDbContext> logger)  : base(auditableConfig, logger)  {  }   protected override void OnModelCreating(ModelBuilder modelBuilder)  {  base.OnModelCreating(modelBuilder);  modelBuilder.ApplyTemporalHistoryConfigurations();  } } 
``` 
- Using Extension Method:

```csharp
 public class MyExistingDbContext : DbContext {  protected override void OnModelCreating(ModelBuilder modelBuilder)  {  base.OnModelCreating(modelBuilder);  modelBuilder.ApplyTemporalHistoryConfigurations();  } } 
```


### Generating Migrations

After setting up your DbContext, generate migrations as usual:

```csharp
 dotnet ef migrations add InitialCreate --context MyDbContext 
```

## Advanced Configuration

The library offers flexibility for advanced configurations such as entity-specific auditing settings. Check the AuditableConfig class for more details.

## Contribution

Contributions to the TemporalHistory library are welcome. Please refer to the contributing guidelines for more information.

## License

This project is licensed under the LGPL v3 License - see the [LICENSE](https://www.gnu.org/licenses/lgpl-3.0.en.html) for details.

---

Disclaimer: This library is still in development and may not be suitable for production use. Use at your own risk.
