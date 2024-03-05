# DropBear Codex TemporalHistory Library

## Overview

The DropBear Codex TemporalHistory Library provides a robust set of tools and services for managing, querying, and rolling back the state of entities within an application using Entity Framework Core. Designed to support temporal data management, this library allows applications to track changes over time, audit entity modifications, and revert entities to their historical states.

## Features

- **Temporal Entity Base**: A base class for entities that should be audited and support temporal features.
- **Audit Logging**: Services and models to capture and persist audit logs, detailing who made changes, what those changes were, and when they were made.
- **Temporal Queries**: A service to query the historical states of entities, supporting complex temporal queries.
- **Rollback Capabilities**: A service to rollback entities to their previous states based on historical data.

## Getting Started

### Prerequisites

- .NET Core 3.1 or later
- Entity Framework Core 3.1 or later

### Installation

1. Clone the repository or download the source code.
2. Include the library in your .NET Core project.
3. Configure your `DbContext` to inherit from `AuditableDbContext` for audit logging support.

## Usage

### Configuring Temporal Entities

Implement the `TemporalEntityBase` for any entity that requires temporal features and audit logging. Ensure to configure the `DbContext` to apply temporal configurations using the `ModelBuilderExtensions`.

```csharp
public class MyEntity : TemporalEntityBase
{
    // Entity properties
}
```

### Working with Audit Logs

Inject `AuditService` into your services or controllers to work with audit logs. Use the `SaveAuditEntries` method to persist audit entries.

```csharp
var auditService = new AuditService(context, new AuditContext { UserId = userId, Reason = "Update operation", OperationCode = OperationCode.Update });
auditService.SaveAuditEntries(auditEntries);
```

### Querying Historical Data

Use `TemporalQueryService<T>` to fetch historical states of your entities. Specify the entity type, key selector, entity ID, and the date range for the query.

```csharp
var history = temporalQueryService.GetHistoryForKey(e => e.Id, entityId, startDate, endDate);
```

### Performing Rollbacks

The `RollbackService<T>` provides functionality to rollback entities to their historical states. Specify the entity type, key selector, entity ID, and the target rollback date.

```csharp
rollbackService.RollbackTo(e => e.Id, entityId, rollbackDate);
```

## Contributing

Contributions are welcome! Please submit pull requests or open issues to discuss proposed changes or report bugs.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- The Entity Framework Core team for providing a robust ORM for .NET
- Contributors who have helped shape this library