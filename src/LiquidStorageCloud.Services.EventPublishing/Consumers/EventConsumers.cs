using LiquidStorageCloud.Core.Database;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LiquidStorageCloud.Services.EventPublishing.Consumers
{
    /// <summary>
    /// Base consumer class with common functionality
    /// </summary>
    /// <typeparam name="T">The type of entity in the event</typeparam>
    public abstract class BaseEventConsumer<T> where T : ISurrealEntity
    {
        protected readonly ILogger _logger;

        protected BaseEventConsumer(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual async Task PublishToSns(string topicArn, object message)
        {
            _logger.LogInformation("Publishing to SNS topic {TopicArn}: {Message}", topicArn, message);
            // SNS publishing will be handled by MassTransit configuration
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Consumer for entity created events
    /// </summary>
    /// <typeparam name="T">The type of entity that was created</typeparam>
    public class EntityCreatedConsumer<T> : BaseEventConsumer<T>, IConsumer<EntityCreatedEvent<T>> where T : ISurrealEntity
    {
        public EntityCreatedConsumer(ILogger<EntityCreatedConsumer<T>> logger) : base(logger)
        {
        }

        public async Task Consume(ConsumeContext<EntityCreatedEvent<T>> context)
        {
            var entity = context.Message.Entity;
            _logger.LogInformation("Processing created entity: {EntityType} with ID {Id}", typeof(T).Name, entity.Id);

            // Publish to SNS for downstream processing
            await PublishToSns($"arn:aws:sns:{context.Message.Entity.TableName}:entity-created", new
            {
                EntityType = typeof(T).Name,
                Entity = entity,
                Timestamp = context.Message.Timestamp
            });
        }
    }

    /// <summary>
    /// Consumer for entity updated events
    /// </summary>
    /// <typeparam name="T">The type of entity that was updated</typeparam>
    public class EntityUpdatedConsumer<T> : BaseEventConsumer<T>, IConsumer<EntityUpdatedEvent<T>> where T : ISurrealEntity
    {
        public EntityUpdatedConsumer(ILogger<EntityUpdatedConsumer<T>> logger) : base(logger)
        {
        }

        public async Task Consume(ConsumeContext<EntityUpdatedEvent<T>> context)
        {
            var entity = context.Message.Entity;
            _logger.LogInformation("Processing updated entity: {EntityType} with ID {Id}", typeof(T).Name, entity.Id);

            // Publish to SNS for downstream processing
            await PublishToSns($"arn:aws:sns:{context.Message.Entity.TableName}:entity-updated", new
            {
                EntityType = typeof(T).Name,
                Entity = entity,
                Timestamp = context.Message.Timestamp
            });
        }
    }

    /// <summary>
    /// Consumer for entity deleted events
    /// </summary>
    /// <typeparam name="T">The type of entity that was deleted</typeparam>
    public class EntityDeletedConsumer<T> : BaseEventConsumer<T>, IConsumer<EntityDeletedEvent<T>> where T : ISurrealEntity
    {
        public EntityDeletedConsumer(ILogger<EntityDeletedConsumer<T>> logger) : base(logger)
        {
        }

        public async Task Consume(ConsumeContext<EntityDeletedEvent<T>> context)
        {
            _logger.LogInformation("Processing deleted entity: {EntityType} with ID {Id}", typeof(T).Name, context.Message.Id);

            // Publish to SNS for downstream processing
            await PublishToSns($"arn:aws:sns:{context.Message.Namespace}:entity-deleted", new
            {
                EntityType = typeof(T).Name,
                Id = context.Message.Id,
                Namespace = context.Message.Namespace,
                TableName = context.Message.TableName,
                Timestamp = context.Message.Timestamp
            });
        }
    }

    /// <summary>
    /// Consumer for entity solid state changed events
    /// </summary>
    /// <typeparam name="T">The type of entity whose solid state changed</typeparam>
    public class EntitySolidStateChangedConsumer<T> : BaseEventConsumer<T>, IConsumer<EntitySolidStateChangedEvent<T>> where T : ISurrealEntity
    {
        public EntitySolidStateChangedConsumer(ILogger<EntitySolidStateChangedConsumer<T>> logger) : base(logger)
        {
        }

        public async Task Consume(ConsumeContext<EntitySolidStateChangedEvent<T>> context)
        {
            _logger.LogInformation("Processing solid state change for entity: {EntityType} with ID {Id}", typeof(T).Name, context.Message.Id);

            // Publish to SNS for downstream processing
            await PublishToSns($"arn:aws:sns:{context.Message.Namespace}:entity-solid-state-changed", new
            {
                EntityType = typeof(T).Name,
                Id = context.Message.Id,
                Namespace = context.Message.Namespace,
                TableName = context.Message.TableName,
                SolidState = context.Message.SolidState,
                Timestamp = context.Message.Timestamp
            });
        }
    }
}
