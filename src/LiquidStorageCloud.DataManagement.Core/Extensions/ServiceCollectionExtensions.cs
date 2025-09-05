using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.DataManagement.Core.CQRS;
using LiquidStorageCloud.DataManagement.Core.CQRS.Commands;
using LiquidStorageCloud.DataManagement.Core.CQRS.Factory;
using LiquidStorageCloud.DataManagement.Core.CQRS.Handlers;
using LiquidStorageCloud.DataManagement.Core.CQRS.Queries;
using LiquidStorageCloud.DataManagement.Core.Repository;
using LiquidStorageCloud.Services.EventPublishing.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LiquidStorageCloud.DataManagement.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SurrealDB and CQRS services to the service collection with optional event publishing
        /// </summary>
        public static IServiceCollection AddDataManagementCore(
            this IServiceCollection services, 
            IConfiguration configuration,
            Action<DataManagementOptions>? configureOptions = null)
        {
            var options = new DataManagementOptions();
            configureOptions?.Invoke(options);

            // Add SurrealDB client using environment variables set by AppHost
            var dbOptions = SurrealDbOptions
                .Create()
                .WithEndpoint(Environment.GetEnvironmentVariable("SURREALDB_URL") ?? configuration["SurrealDB:Url"])
                .WithNamespace(Environment.GetEnvironmentVariable("SURREALDB_NS") ?? configuration["SurrealDB:Namespace"])
                .WithDatabase(Environment.GetEnvironmentVariable("SURREALDB_DB") ?? configuration["SurrealDB:Database"])
                .WithUsername(Environment.GetEnvironmentVariable("SURREALDB_USER") ?? configuration["SurrealDB:Username"])
                .WithPassword(Environment.GetEnvironmentVariable("SURREALDB_PASS") ?? configuration["SurrealDB:Password"])
                .Build();

            // Register SurrealDB client
            services.AddSurreal(dbOptions);

            // Register repositories
            services.AddScoped(typeof(ISurrealRepository<>), typeof(SurrealRepository<>));

            // Add CQRS services
            services.AddCqrs(options.EnableEventPublishing);

            return services;
        }

        /// <summary>
        /// Adds automatic registration of SurrealDB entities and their handlers
        /// </summary>
        public static IServiceCollection AddAutoEntityRegistration(
            this IServiceCollection services,
            Assembly? assembly = null,
            ILogger? logger = null)
        {
            try
            {
                // Use provided assembly or get the calling assembly
                assembly ??= Assembly.GetCallingAssembly();

                // Find all types that implement ISurrealEntity
                var surrealEntityTypes = assembly.GetTypes()
                    .Where(type => typeof(ISurrealEntity).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

                foreach (var entityType in surrealEntityTypes)
                {
                    try
                    {
                        // Create an instance with default constructor
                        var entity = Activator.CreateInstance(entityType) as ISurrealEntity;
                        if (entity != null)
                        {
                            // Register the concrete type instance
                            services.AddSingleton(entityType, entity);

                            // Register entity handlers for this type
                            var addEntityHandlersMethod = typeof(ServiceCollectionExtensions)
                                .GetMethod(nameof(AddEntityHandlers), BindingFlags.NonPublic | BindingFlags.Static)
                                ?.MakeGenericMethod(entityType);

                            addEntityHandlersMethod?.Invoke(null, new object[] { services });
                            
                            logger?.LogInformation("Registered entity type: {EntityType}", entityType.Name);
                        }
                        else
                        {
                            logger?.LogWarning("Failed to create instance of entity type: {EntityType}", entityType.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "Error registering entity type: {EntityType}", entityType.Name);
                        throw; // Rethrow to maintain fail-fast behavior
                    }
                }

                return services;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error scanning for entity types");
                throw; // Rethrow to maintain fail-fast behavior
            }
        }

        /// <summary>
        /// Adds CQRS services to the service collection
        /// </summary>
        public static IServiceCollection AddCqrs(
            this IServiceCollection services,
            bool enableEventPublishing = false)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Register factory
            services.AddSingleton<ICqrsHandlerFactory, CqrsHandlerFactory>();

            // Register generic handlers
            services.AddScoped(typeof(CreateEntityHandler<>));
            services.AddScoped(typeof(UpdateEntityHandler<>));
            services.AddScoped(typeof(DeleteEntityHandler<>));
            services.AddScoped(typeof(SetSolidStateHandler<>));
            services.AddScoped(typeof(GetEntityByIdHandler<>));
            services.AddScoped(typeof(ListEntitiesHandler<>));

            if (enableEventPublishing)
            {
                services.AddScoped(typeof(EventPublishingDecorator<,>));
            }

            return services;
        }

        /// <summary>
        /// Adds entity-specific CQRS handlers for a given entity type with optional event publishing
        /// </summary>
        public static IServiceCollection AddEntityHandlers<T>(
            this IServiceCollection services,
            Action<EntityHandlerOptions>? configureOptions = null)
            where T : class, ISurrealEntity
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var options = new EntityHandlerOptions();
            configureOptions?.Invoke(options);

            void RegisterEntityHandler<TCommand, TResult>(
                IServiceCollection services,
                Type handlerType) 
                where TCommand : ICommand<TResult>
                where TResult : class, ISurrealEntity
            {
                if (options.EnableEventPublishing)
                {
                    // Register base handler
                    services.AddScoped(handlerType);
                    
                    // Register decorated handler
                    services.AddScoped<ICommandHandler<TCommand, TResult>>(sp =>
                    {
                        var baseHandler = (ICommandHandler<TCommand, TResult>)sp.GetRequiredService(handlerType);
                        var publisherFactory = sp.GetService<IEventPublisherFactory>();
                        return new EventPublishingDecorator<TCommand, TResult>(baseHandler, publisherFactory);
                    });
                }
                else
                {
                    // Register base handler directly
                    services.AddScoped(typeof(ICommandHandler<TCommand, TResult>), handlerType);
                }
            }

            void RegisterHandler<TCommand, TResult>(
                IServiceCollection services,
                Type handlerType) 
                where TCommand : ICommand<TResult>
            {
                // For non-entity commands, just register the handler directly without decoration
                services.AddScoped(typeof(ICommandHandler<TCommand, TResult>), handlerType);
            }

            // Register entity handlers with optional event publishing
            RegisterEntityHandler<CreateEntityCommand<T>, T>(services, typeof(CreateEntityHandler<T>));
            RegisterEntityHandler<UpdateEntityCommand<T>, T>(services, typeof(UpdateEntityHandler<T>));
            RegisterHandler<DeleteEntityCommand<T>, bool>(services, typeof(DeleteEntityHandler<T>));
            RegisterHandler<SetSolidStateCommand<T>, bool>(services, typeof(SetSolidStateHandler<T>));

            // Register query handlers (no event publishing needed)
            services.AddScoped<IQueryHandler<GetEntityByIdQuery<T>, T?>, GetEntityByIdHandler<T>>();
            services.AddScoped<IQueryHandler<ListEntitiesQuery<T>, IEnumerable<T>>, ListEntitiesHandler<T>>();

            return services;
        }
    }

    public class DataManagementOptions
    {
        public bool EnableEventPublishing { get; set; }
    }

    public class EntityHandlerOptions
    {
        public bool EnableEventPublishing { get; set; }
    }
}
