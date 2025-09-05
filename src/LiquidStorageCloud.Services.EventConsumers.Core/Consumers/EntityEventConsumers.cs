using System.Text.Json;
using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.Core.Models;
using LiquidStorageCloud.DataManagement.EntityFramework;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LiquidStorageCloud.Services.EventConsumers.Consumers;

public abstract class BaseConsumer
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly ILogger<BaseConsumer> _logger;

    protected BaseConsumer(ApplicationDbContext dbContext, ILogger<BaseConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected async Task SaveMessageAsync(Message message)
    {
        try
        {
            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully saved message for {EntityType} with ID {EntityId}", 
                message.EntityType, message.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving message for {EntityType} with ID {EntityId}", 
                message.EntityType, message.EntityId);
            throw;
        }
    }
}

public class EntityCreatedConsumer<T> : BaseConsumer, IConsumer<EntityCreatedEvent<T>> where T : class, ISurrealEntity
{
    public EntityCreatedConsumer(ApplicationDbContext dbContext, ILogger<EntityCreatedConsumer<T>> logger) 
        : base(dbContext, logger)
    {
    }

    public async Task Consume(ConsumeContext<EntityCreatedEvent<T>> context)
    {
        var entity = context.Message.Entity;
        var message = new Message
        {
            EntityId = entity.Id,
            EntityType = typeof(T).Name,
            EventType = "Created",
            Namespace = entity.GetType().GetProperty("Namespace")?.GetValue(entity)?.ToString() ?? string.Empty,
            TableName = entity.GetType().GetProperty("TableName")?.GetValue(entity)?.ToString() ?? string.Empty,
            EntityData = JsonSerializer.Serialize(entity),
            Timestamp = context.Message.Timestamp
        };

        await SaveMessageAsync(message);
    }
}

public class EntityUpdatedConsumer<T> : BaseConsumer, IConsumer<EntityUpdatedEvent<T>> where T : class, ISurrealEntity
{
    public EntityUpdatedConsumer(ApplicationDbContext dbContext, ILogger<EntityUpdatedConsumer<T>> logger) 
        : base(dbContext, logger)
    {
    }

    public async Task Consume(ConsumeContext<EntityUpdatedEvent<T>> context)
    {
        var entity = context.Message.Entity;
        var message = new Message
        {
            EntityId = entity.Id,
            EntityType = typeof(T).Name,
            EventType = "Updated",
            Namespace = entity.GetType().GetProperty("Namespace")?.GetValue(entity)?.ToString() ?? string.Empty,
            TableName = entity.GetType().GetProperty("TableName")?.GetValue(entity)?.ToString() ?? string.Empty,
            EntityData = JsonSerializer.Serialize(entity),
            Timestamp = context.Message.Timestamp
        };

        await SaveMessageAsync(message);
    }
}

public class EntityDeletedConsumer<T> : BaseConsumer, IConsumer<EntityDeletedEvent<T>> where T : class, ISurrealEntity
{
    public EntityDeletedConsumer(ApplicationDbContext dbContext, ILogger<EntityDeletedConsumer<T>> logger) 
        : base(dbContext, logger)
    {
    }

    public async Task Consume(ConsumeContext<EntityDeletedEvent<T>> context)
    {
        var message = new Message
        {
            EntityId = context.Message.Id,
            EntityType = typeof(T).Name,
            EventType = "Deleted",
            Namespace = context.Message.Namespace,
            TableName = context.Message.TableName,
            Timestamp = context.Message.Timestamp
        };

        await SaveMessageAsync(message);
    }
}

public class EntitySolidStateChangedConsumer<T> : BaseConsumer, IConsumer<EntitySolidStateChangedEvent<T>> where T : class, ISurrealEntity
{
    public EntitySolidStateChangedConsumer(ApplicationDbContext dbContext, ILogger<EntitySolidStateChangedConsumer<T>> logger) 
        : base(dbContext, logger)
    {
    }

    public async Task Consume(ConsumeContext<EntitySolidStateChangedEvent<T>> context)
    {
        var message = new Message
        {
            EntityId = context.Message.Id,
            EntityType = typeof(T).Name,
            EventType = "SolidStateChanged",
            Namespace = context.Message.Namespace,
            TableName = context.Message.TableName,
            SolidState = context.Message.SolidState,
            Timestamp = context.Message.Timestamp
        };

        await SaveMessageAsync(message);
    }
}
