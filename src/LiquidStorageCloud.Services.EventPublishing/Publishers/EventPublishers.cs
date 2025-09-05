using System;
using Microsoft.Extensions.DependencyInjection;
using LiquidStorageCloud.Core.Database;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LiquidStorageCloud.Services.EventPublishing.Configuration;

namespace LiquidStorageCloud.Services.EventPublishing.Publishers;

/// <summary>
/// Base publisher class with common functionality
/// </summary>
public abstract class BaseEventPublisher
{
    protected readonly IPublishEndpoint _publishEndpoint;
    protected readonly ILogger _logger;

    protected BaseEventPublisher(IPublishEndpoint publishEndpoint, ILogger logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
}

/// <summary>
/// Publisher for entity events
/// </summary>
/// <typeparam name="T">The type of entity</typeparam>
public class EntityEventPublisher<T> : BaseEventPublisher where T : class, ISurrealEntity
{
    public EntityEventPublisher(IPublishEndpoint publishEndpoint, ILogger<EntityEventPublisher<T>> logger)
        : base(publishEndpoint, logger)
    {
    }

    /// <summary>
    /// Publishes an entity created event
    /// </summary>
    public async Task PublishCreatedAsync(T entity)
    {
        _logger.LogInformation("Publishing created event for entity: {EntityType} with ID {Id}",
            typeof(T).Name, entity.Id);

        await _publishEndpoint.Publish(new EntityCreatedEvent<T>
        {
            Entity = entity,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Publishes an entity updated event
    /// </summary>
    public async Task PublishUpdatedAsync(T entity)
    {
        _logger.LogInformation("Publishing updated event for entity: {EntityType} with ID {Id}",
            typeof(T).Name, entity.Id);

        await _publishEndpoint.Publish(new EntityUpdatedEvent<T>
        {
            Entity = entity,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Publishes an entity deleted event
    /// </summary>
    public async Task PublishDeletedAsync(string id, string tableName, string nameSpace)
    {
        _logger.LogInformation("Publishing deleted event for entity: {EntityType} with ID {Id}",
            typeof(T).Name, id);

        await _publishEndpoint.Publish(new EntityDeletedEvent<T>
        {
            Id = id,
            TableName = tableName,
            Namespace = nameSpace,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Publishes an entity solid state changed event
    /// </summary>
    public async Task PublishSolidStateChangedAsync(string id, string tableName, string nameSpace, bool solidState)
    {
        _logger.LogInformation("Publishing solid state changed event for entity: {EntityType} with ID {Id}",
            typeof(T).Name, id);

        await _publishEndpoint.Publish(new EntitySolidStateChangedEvent<T>
        {
            Id = id,
            TableName = tableName,
            Namespace = nameSpace,
            SolidState = solidState,
            Timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Factory for creating entity event publishers. Extended to resolve transport publishers by name/mode.
/// </summary>
public interface IEventPublisherFactory
{
    EntityEventPublisher<T> CreatePublisher<T>() where T : class, ISurrealEntity;

    /// <summary>
    /// Resolve a transport-level IEventPublisher for the given service and mode.
    /// </summary>
    IEventPublisher Resolve(string serviceName, PublisherMode mode = PublisherMode.Default);
}

public class EventPublisherFactory : IEventPublisherFactory
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _serviceProvider;

    public EventPublisherFactory(IPublishEndpoint publishEndpoint, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        _publishEndpoint = publishEndpoint;
        _loggerFactory = loggerFactory;
        _serviceProvider = serviceProvider;
    }

    public EntityEventPublisher<T> CreatePublisher<T>() where T : class, ISurrealEntity
    {
        var logger = _loggerFactory.CreateLogger<EntityEventPublisher<T>>();
        return new EntityEventPublisher<T>(_publishEndpoint, logger);
    }

    /// <summary>
    /// Resolve an IEventPublisher instance according to the requested PublisherMode.
    /// Falls back to SQS adapter when EventBridge publisher is not available.
    /// </summary>
    public IEventPublisher Resolve(string serviceName, PublisherMode mode = PublisherMode.Default)
    {
        // Primary = existing SQS/Rabbit adapter
        var sqsLogger = _loggerFactory.CreateLogger<SqsAdapterPublisher>();
        var primary = new SqsAdapterPublisher(_serviceProvider.GetRequiredService<IPublishEndpoint>(), sqsLogger);

        // Secondary = EventBridge publisher if registered
        var secondary = _serviceProvider.GetService<EventBridgePublisher>();

        // If secondary unavailable, fallback to primary for safety
        if (secondary == null)
        {
            // Return primary for all non-explicit EventBridgeOnly modes to avoid surprises
            if (mode == PublisherMode.EventBridgeOnly)
            {
                // No EventBridge client available; log a warning and return primary
                var warn = _loggerFactory.CreateLogger<EventPublisherFactory>();
                warn.LogWarning("EventBridgePublisher is not registered; falling back to primary SQS publisher for service {ServiceName}", serviceName);
                return primary;
            }

            // For dual/parallel/shadow modes, return primary (no-op secondary)
            return primary;
        }

        // If mode is Default, preserve existing behavior (primary)
        if (mode == PublisherMode.Default || mode == PublisherMode.RabbitOnly)
            return primary;

        if (mode == PublisherMode.EventBridgeOnly)
            return secondary;

        // For Dual/Parallel/Shadow, wrap in DualPublisher
        var dualLogger = _loggerFactory.CreateLogger<DualPublisher>();
        return new DualPublisher(primary, secondary, dualLogger);
    }
}
