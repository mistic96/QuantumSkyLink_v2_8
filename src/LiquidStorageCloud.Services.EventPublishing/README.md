# EventPublishing Library

A .NET library that provides event publishing and consuming capabilities using MassTransit and Amazon SNS/SQS.

## Features

### Event Publishers
- Generic entity event publishers for:
  - Entity Created events
  - Entity Updated events
  - Entity Deleted events
  - Entity Solid State Changed events
- Factory pattern for creating publishers
- Automatic AWS SNS/SQS integration

### Event Consumers
- Corresponding consumers for all event types
- Automatic message routing
- Error handling and retry policies
- Circuit breaker pattern implementation

## Installation

Add a reference to the EventPublishing library in your project:

```xml
<ProjectReference Include="..\EventPublishing\Services.EventPublishing.csproj" />
```

## Usage

### Basic Setup

Register event publishing services in your `Program.cs` or `Startup.cs`:

```csharp
builder.Services.AddEventPublishing(builder.Configuration);
```

### Using Event Publishers

1. Using the Factory Pattern:
```csharp
public class YourService
{
    private readonly IEventPublisherFactory _publisherFactory;

    public YourService(IEventPublisherFactory publisherFactory)
    {
        _publisherFactory = publisherFactory;
    }

    public async Task HandleEntityChange(YourEntity entity)
    {
        var publisher = _publisherFactory.CreatePublisher<YourEntity>();
        await publisher.PublishCreatedAsync(entity);
    }
}
```

2. Using Direct Injection:
```csharp
// Register specific entity publisher
services.AddEntityEventPublisher<YourEntity>();

// Use in your service
public class YourService
{
    private readonly EntityEventPublisher<YourEntity> _publisher;

    public YourService(EntityEventPublisher<YourEntity> publisher)
    {
        _publisher = publisher;
    }

    public async Task HandleEntityChange(YourEntity entity)
    {
        await _publisher.PublishCreatedAsync(entity);
        // Or other event types:
        await _publisher.PublishUpdatedAsync(entity);
        await _publisher.PublishDeletedAsync(entity.Id, entity.TableName, entity.Namespace);
        await _publisher.PublishSolidStateChangedAsync(entity.Id, entity.TableName, entity.Namespace, true);
    }
}
```

## Configuration

Required AWS configuration in appsettings.json:

```json
{
  "AWS": {
    "Region": "your-region",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "QueuePrefix": "YourSolutionName" // Optional: Defaults to "LiquidStorageCloud"
  }
}
```

The `QueuePrefix` setting allows you to namespace your queues with a custom prefix. If not specified, it defaults to "LiquidStorageCloud". This helps avoid naming conflicts and maintains organization in your AWS SQS resources.

## MassTransit Configuration Details

The library provides a comprehensive MassTransit configuration with Amazon SNS/SQS integration. Here's the detailed setup:

```csharp
services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<EntityCreatedConsumer<ISurrealEntity>>();
    x.AddConsumer<EntityUpdatedConsumer<ISurrealEntity>>();
    x.AddConsumer<EntityDeletedConsumer<ISurrealEntity>>();
    x.AddConsumer<EntitySolidStateChangedConsumer<ISurrealEntity>>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        // AWS Configuration
        cfg.Host("your-region", h =>
        {
            h.AccessKey("your-access-key");
            h.SecretKey("your-secret-key");
        });

        // Get queue prefix from configuration
        var queuePrefix = configuration["AWS:QueuePrefix"] ?? "LiquidStorageCloud";

        // Queue Configuration with prefix
        cfg.ReceiveEndpoint($"{queuePrefix}-entity-created-queue", e =>
        {
            e.ConfigureConsumer<EntityCreatedConsumer<ISurrealEntity>>(context);
            e.PrefetchCount = 16;
            e.WaitTimeSeconds = 20;
        });

        // Similar configurations for other queues with prefix...

        // Retry Policy
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)
        ));

        // Circuit Breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });

        // Error Handling
        cfg.UseMessageScope(context);
        cfg.UseInMemoryOutbox();
    });
});

// Add MassTransit hosted service
services.AddMassTransitHostedService();
```

### Queue Configuration

The library configures the following queues with specific settings (where {prefix} is your AWS:QueuePrefix or "LiquidStorageCloud" by default):
- {prefix}-entity-created-queue
- {prefix}-entity-updated-queue
- {prefix}-entity-deleted-queue
- {prefix}-entity-solid-state-changed-queue

Each queue is configured with:
- **Prefetch Count**: 16 (optimizes message processing throughput)
- **Wait Time**: 20 seconds (long polling for efficient message retrieval)
- **Retry Policy**: Progressive intervals (1s, 5s, 15s)
- **Circuit Breaker Protection**:
  - Tracking Period: 1 minute
  - Trip Threshold: 15 exceptions
  - Active Threshold: 10 concurrent messages
  - Reset Interval: 5 minutes

### Error Handling Features
- Message scope for tracking message context
- In-memory outbox for guaranteed message processing
- Progressive retry policy for transient failures
- Circuit breaker to prevent system overload

## Dependencies

- MassTransit
- MassTransit.AmazonSQS
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting.Abstractions
- Microsoft.Extensions.Logging.Abstractions
