# EventConsumers.Core Library

A .NET library that provides entity event consumers for handling data changes using MassTransit and Amazon SQS.

## Features

- Generic entity event consumers for:
  - Entity Created events
  - Entity Updated events
  - Entity Deleted events
  - Entity Solid State Changed events
- Automatic message persistence to SQL Server
- AWS SQS integration via MassTransit
- Built-in error handling and logging
- Easy setup with dependency injection

## Installation

Add a reference to the EventConsumers.Core library in your project:

```xml
<ProjectReference Include="..\EventConsumers.Core\EventConsumers.Core.csproj" />
```

## Usage

The library provides a simple extension method to register all consumers:

```csharp
builder.Services.AddEventConsumers(options =>
{
    options.Region = "us-east-1";  // AWS region (optional, defaults to us-east-1)
    options.AccessKey = configuration["AWS:AccessKey"];
    options.SecretKey = configuration["AWS:SecretKey"];
});
```

This will automatically:
- Register all entity event consumers
- Configure MassTransit with Amazon SQS
- Set up message persistence
- Configure error handling and logging

### MassTransit Configuration Details

The library configures MassTransit with the following setup:

```csharp
services.AddMassTransit(x =>
{
    // Register generic entity event consumers
    x.AddConsumer(typeof(EntityCreatedConsumer<>));
    x.AddConsumer(typeof(EntityUpdatedConsumer<>));
    x.AddConsumer(typeof(EntityDeletedConsumer<>));
    x.AddConsumer(typeof(EntitySolidStateChangedConsumer<>));

    x.UsingAmazonSqs((context, cfg) =>
    {
        // Configure AWS credentials and region
        cfg.Host("us-east-1", h =>
        {
            h.AccessKey("your-access-key");
            h.SecretKey("your-secret-key");
        });

        // Configure endpoints using default naming conventions
        cfg.ConfigureEndpoints(context);
    });
});
```

Key Configuration Points:
- Uses default endpoint naming conventions
- Automatically registers generic consumers for all entity types
- Configures Amazon SQS as the transport
- Simple setup with minimal configuration required

## Event Types

The library handles the following events:

1. EntityCreatedEvent<T>
   - Triggered when a new entity is created
   - Stores full entity data
   - Captures namespace and table name from entity properties

2. EntityUpdatedEvent<T>
   - Triggered when an entity is modified
   - Stores updated entity data
   - Captures namespace and table name from entity properties

3. EntityDeletedEvent<T>
   - Triggered when an entity is deleted
   - Stores entity identifier and metadata
   - Includes namespace and table name information

4. EntitySolidStateChangedEvent<T>
   - Triggered when an entity's solid state changes
   - Stores state change information
   - Includes namespace and table name information

## Configuration

Required AWS configuration:

```json
{
  "AWS": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "us-east-1"  // Optional, defaults to us-east-1
  }
}
```

## Dependencies

- MassTransit
- MassTransit.AmazonSQS
- Microsoft.EntityFrameworkCore
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## Message Storage

All consumed events are automatically stored in the Messages table with the following information:
- EntityId: Unique identifier of the entity
- EntityType: The type name of the entity
- EventType: Type of event (Created/Updated/Deleted/SolidStateChanged)
- Namespace: Entity's namespace
- TableName: Entity's table name
- EntityData: JSON serialized entity data (for Create/Update events)
- SolidState: State information (for state change events)
- Timestamp: When the event occurred

## Error Handling and Logging

The library includes comprehensive error handling and logging:

- All database operations are wrapped in try-catch blocks
- Failed message persistence is logged with error details
- Successful message persistence is logged with entity information
- Uses ILogger<T> for structured logging
- Logs include correlation information for tracking events

Example log messages:
```
Information: Successfully saved message for {EntityType} with ID {EntityId}
Error: Error saving message for {EntityType} with ID {EntityId}
```

## Base Consumer

All event consumers inherit from BaseConsumer which provides:
- Common database context handling
- Centralized message persistence logic
- Standardized error handling
- Consistent logging implementation

This ensures uniform behavior across all event types while maintaining clean, maintainable code.
