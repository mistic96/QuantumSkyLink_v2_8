# DataManagement.Core

A .NET library that provides CQRS (Command Query Responsibility Segregation) functionality for SurrealDB operations with event publishing capabilities.

## Features

- CQRS pattern implementation
- Generic entity commands and queries
- Event publishing integration
- SurrealDB repository abstraction
- Factory pattern for handlers

## Installation

Add a reference to the DataManagement.Core library in your project:

```xml
<ProjectReference Include="..\DataManagement.Core\DataManagement.Core.csproj" />
```

## Setup

1. Register services in your `Program.cs` or `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add core CQRS services and SurrealDB
    services.AddDataManagementCore(Configuration);

    // Register handlers for your entity types
    services.AddEntityHandlers<YourEntity>();
    services.AddEntityHandlers<AnotherEntity>();
}
```

The `AddDataManagementCore` method will:
- Configure SurrealDB client
- Register repositories
- Register the CQRS handler factory
- Register generic handlers

The `AddEntityHandlers<T>` method will register all necessary handlers for your entity:
- CreateEntityHandler
- UpdateEntityHandler
- DeleteEntityHandler
- SetSolidStateHandler
- GetEntityByIdHandler
- ListEntitiesHandler

2. Configure SurrealDB settings in your appsettings.json:

```json
{
  "SurrealDB": {
    "Url": "your-surrealdb-url",
    "Namespace": "your-namespace",
    "Database": "your-database",
    "Username": "your-username",
    "Password": "your-password"
  }
}
```

Or use environment variables:
- SURREALDB_URL
- SURREALDB_NS
- SURREALDB_DB
- SURREALDB_USER
- SURREALDB_PASS

3. Implement ISurrealEntity for your entities:

```csharp
public class YourEntity : ISurrealEntity
{
    public string Id { get; set; } = string.Empty;
    public string TableName { get; } = "your_table";
    public string Namespace { get; } = "your_namespace";
    // Your entity properties
}
```

## Using CQRS

### Commands

1. **Create Entity**
```csharp
public async Task<YourEntity> CreateEntityAsync(YourEntity entity)
{
    var command = new CreateEntityCommand<YourEntity>(entity);
    var handler = _handlerFactory.GetCommandHandler<CreateEntityCommand<YourEntity>, YourEntity>();
    return await handler.HandleAsync(command);
}
```

2. **Update Entity**
```csharp
public async Task<YourEntity> UpdateEntityAsync(YourEntity entity)
{
    var command = new UpdateEntityCommand<YourEntity>(entity);
    var handler = _handlerFactory.GetCommandHandler<UpdateEntityCommand<YourEntity>, YourEntity>();
    return await handler.HandleAsync(command);
}
```

3. **Delete Entity**
```csharp
public async Task<bool> DeleteEntityAsync(string id, string tableName)
{
    var command = new DeleteEntityCommand<YourEntity>(id, tableName);
    var handler = _handlerFactory.GetCommandHandler<DeleteEntityCommand<YourEntity>, bool>();
    return await handler.HandleAsync(command);
}
```

4. **Set Solid State**
```csharp
public async Task<bool> SetSolidStateAsync(string id, string tableName, bool solidState)
{
    var command = new SetSolidStateCommand<YourEntity>(id, tableName, solidState);
    var handler = _handlerFactory.GetCommandHandler<SetSolidStateCommand<YourEntity>, bool>();
    return await handler.HandleAsync(command);
}
```

### Queries

1. **Get Entity by ID**
```csharp
public async Task<YourEntity?> GetByIdAsync(string id, string tableName)
{
    var query = new GetEntityByIdQuery<YourEntity>(id, tableName);
    var handler = _handlerFactory.GetQueryHandler<GetEntityByIdQuery<YourEntity>, YourEntity?>();
    return await handler.HandleAsync(query);
}
```

2. **List Entities**
```csharp
public async Task<IEnumerable<YourEntity>> ListAsync(
    string tableName,
    Expression<Func<YourEntity, bool>>? filter = null,
    Expression<Func<YourEntity, object>>? orderBy = null,
    bool ascending = true)
{
    var query = new ListEntitiesQuery<YourEntity>(
        tableName,
        filter,
        orderBy,
        ascending
    );
    var handler = _handlerFactory.GetQueryHandler<ListEntitiesQuery<YourEntity>, IEnumerable<YourEntity>>();
    return await handler.HandleAsync(query);
}
```

3. **Custom Query**
```csharp
public async Task<IEnumerable<TResult>> QueryAsync<TResult>(
    string query,
    Dictionary<string, object>? parameters = null)
{
    var queryObj = new QueryEntitiesQuery<YourEntity, TResult>(query, parameters);
    var handler = _handlerFactory.GetQueryHandler<QueryEntitiesQuery<YourEntity, TResult>, IEnumerable<TResult>>();
    return await handler.HandleAsync(queryObj);
}
```

## Handler Factory

The CqrsHandlerFactory is responsible for resolving command and query handlers from the dependency injection container:

```csharp
public interface ICqrsHandlerFactory
{
    ICommandHandler<TCommand, TResult> GetCommandHandler<TCommand, TResult>() 
        where TCommand : ICommand<TResult>;
    
    ICommandHandler<TCommand> GetCommandHandler<TCommand>() 
        where TCommand : ICommand;
    
    IQueryHandler<TQuery, TResult> GetQueryHandler<TQuery, TResult>() 
        where TQuery : IQuery<TResult>;
}
```

The factory is automatically registered when you call `AddDataManagementCore()`. It uses the service provider to resolve the appropriate handler for each command or query.

## Event Integration

Commands automatically publish events through the EventPublishing library:

- EntityCreatedEvent
- EntityUpdatedEvent
- EntityDeletedEvent
- EntitySolidStateChangedEvent

## Best Practices

1. **Handler Registration**
   - Use AddEntityHandlers<T> for each entity type
   - Register handlers early in your application startup
   - Handlers are scoped by default

2. **Entity Design**
   - Keep entities simple and focused
   - Implement ISurrealEntity properly
   - Use meaningful table names and namespaces

3. **Query Optimization**
   - Use filters in ListEntitiesQuery when possible
   - Consider pagination for large datasets
   - Use custom queries for complex operations

4. **Error Handling**
```csharp
try
{
    var handler = _handlerFactory.GetCommandHandler<CreateEntityCommand<YourEntity>, YourEntity>();
    var result = await handler.HandleAsync(command);
    return result;
}
catch (InvalidOperationException ex)
{
    // Handle missing handler registration
    _logger.LogError(ex, "Handler not registered for command");
    throw;
}
catch (Exception ex)
{
    // Handle other errors
    _logger.LogError(ex, "Operation failed");
    throw;
}
```

## Example Service Implementation

```csharp
public class EntityService<T> where T : class, ISurrealEntity
{
    private readonly ICqrsHandlerFactory _handlerFactory;
    private readonly ILogger<EntityService<T>> _logger;

    public EntityService(ICqrsHandlerFactory handlerFactory, ILogger<EntityService<T>> logger)
    {
        _handlerFactory = handlerFactory;
        _logger = logger;
    }

    public async Task<T> CreateAsync(T entity)
    {
        try
        {
            var command = new CreateEntityCommand<T>(entity);
            var handler = _handlerFactory.GetCommandHandler<CreateEntityCommand<T>, T>();
            return await handler.HandleAsync(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create entity");
            throw;
        }
    }

    public async Task<IEnumerable<T>> ListActiveAsync(string tableName)
    {
        try
        {
            var query = new ListEntitiesQuery<T>(
                tableName,
                onlySolidState: true
            );
            var handler = _handlerFactory.GetQueryHandler<ListEntitiesQuery<T>, IEnumerable<T>>();
            return await handler.HandleAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active entities");
            throw;
        }
    }
}
```

## Testing

1. **Unit Testing**
```csharp
public class EntityServiceTests
{
    private readonly Mock<ICqrsHandlerFactory> _mockHandlerFactory;
    private readonly Mock<ILogger<EntityService<TestEntity>>> _mockLogger;
    private readonly EntityService<TestEntity> _service;

    public EntityServiceTests()
    {
        _mockHandlerFactory = new Mock<ICqrsHandlerFactory>();
        _mockLogger = new Mock<ILogger<EntityService<TestEntity>>>();
        _service = new EntityService<TestEntity>(_mockHandlerFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidEntity_ReturnsCreatedEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = "test" };
        var mockHandler = new Mock<ICommandHandler<CreateEntityCommand<TestEntity>, TestEntity>>();
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<CreateEntityCommand<TestEntity>>()))
            .ReturnsAsync(entity);

        _mockHandlerFactory
            .Setup(f => f.GetCommandHandler<CreateEntityCommand<TestEntity>, TestEntity>())
            .Returns(mockHandler.Object);

        // Act
        var result = await _service.CreateAsync(entity);

        // Assert
        Assert.Equal(entity.Id, result.Id);
    }
}
```

2. **Integration Testing**
```csharp
public class EntityServiceIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly EntityService<TestEntity> _service;

    public EntityServiceIntegrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _service = new EntityService<TestEntity>(
            _fixture.HandlerFactory,
            new Logger<EntityService<TestEntity>>(new LoggerFactory())
        );
    }

    [Fact]
    public async Task CreateAndList_ValidEntity_Success()
    {
        // Arrange
        var entity = new TestEntity { Id = Guid.NewGuid().ToString() };

        // Act
        var created = await _service.CreateAsync(entity);
        var list = await _service.ListActiveAsync("test_entities");

        // Assert
        Assert.Contains(list, e => e.Id == created.Id);
    }
}
