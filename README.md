# DropBear.Codex.TemporalHistory
A library for managing and querying temporal history of entities using Entity Framework Core.

## Features

- **Temporal Queries**: Retrieve historical changes of your entities within a given time range.
- **Snapshot Retrieval**: Fetch the state of an entity at a specific point in time.
- **User-specific Changes**: Filter changes by user to track individual contributions.
- **Latest Changes**: Quickly get the most recent changes to entities.
- **Compare Changes**: Compare the state of an entity between two timestamps and get detailed property changes.

## Getting Started

### Prerequisites
- .NET Core 8 or higher
- Entity Framework Core 8 or higher

### Installation

To install DropBear.Codex.TemporalHistory, add the library to your project via NuGet:

```bash
dotnet add package DropBear.Codex.TemporalHistory
```

### Configuration

Add the library to your project using one of the following methods:

#### Using Dependency Injection

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddTemporalHistory();
}
```

#### Using the Builder Pattern

```csharp
var dbContext = new MyDbContext(options);
var manager = new TemporalManagerBuilder<MyDbContext>()
    .UseDbContext(dbContext)
    .Build();
```

## Usage

```csharp
// Retrieve entity history between two dates
var historyRecords = await temporalHistoryManager.GetHistoryAsync<MyEntity>(startDate, endDate);

// Get a snapshot of an entity at a specific point in time
var snapshot = await temporalHistoryManager.GetEntitySnapshotAt<MyEntity>(dateTime, id);
```

## Contributing
Contributions are welcome! Please feel free to submit pull requests or open issues to discuss proposed changes or enhancements.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Authors
- **DropBear (Terrence Kuchel)** - *Initial work* - [GitHub Profile](https://github.com/tkuchel)

See also the list of [contributors](https://github.com/yourusername/DropBear.Codex.TemporalHistory/contributors) who participated in this project.

## Acknowledgments
- Hat tip to anyone whose code was used
- Inspiration
- etc
