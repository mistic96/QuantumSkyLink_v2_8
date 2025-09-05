using System;
using Amazon.EventBridge;
using Amazon;
using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.Services.EventPublishing.Consumers;
using LiquidStorageCloud.Services.EventPublishing.Publishers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LiquidStorageCloud.Services.EventPublishing.Configuration;

namespace LiquidStorageCloud.Services.EventPublishing.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds event publishing services to the service collection
    /// </summary>
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register publisher factory (existing factory preserved)
        services.AddScoped<IEventPublisherFactory, EventPublisherFactory>();

        // Bind EventBridge options from configuration path "AWS:EventBridge"
        services.Configure<EventBridgeOptions>(configuration.GetSection("AWS:EventBridge"));

        // Register Amazon EventBridge client (honors IRSA/default credential chain when possible)
        services.AddSingleton<IAmazonEventBridge>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<EventBridgeOptions>>().Value ?? new EventBridgeOptions();
            var config = new AmazonEventBridgeConfig();

            if (!string.IsNullOrWhiteSpace(options.Region))
            {
                try
                {
                    config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region);
                }
                catch
                {
                    // If region parsing fails, leave default and let SDK resolve
                }
            }

            // If explicit credentials provided and IRSA disabled, use them. Otherwise rely on SDK chain (IRSA, env, shared credentials, EC2/ECS).
            if (!options.UseIRSA && !string.IsNullOrWhiteSpace(options.AccessKeyId) && !string.IsNullOrWhiteSpace(options.SecretAccessKey))
            {
                return new AmazonEventBridgeClient(options.AccessKeyId, options.SecretAccessKey, config);
            }

            return new AmazonEventBridgeClient(config);
        });

        // Register concrete publishers (do not replace existing SQS/Rabbit publishers)
        services.AddScoped<EventBridgePublisher>();
        services.AddScoped<DualPublisher>();
        // Register auxiliary publisher infrastructure (telemetry, SQS adapter, factory helpers)
        services.AddEventPublisherInfrastructure();

        // Configure MassTransit with Amazon SNS/SQS (preserve existing behavior)
        services.AddMassTransit(x =>
        {
            // Register consumers with ISurrealEntity
            x.AddConsumer<EntityCreatedConsumer<ISurrealEntity>>();
            x.AddConsumer<EntityUpdatedConsumer<ISurrealEntity>>();
            x.AddConsumer<EntityDeletedConsumer<ISurrealEntity>>();
            x.AddConsumer<EntitySolidStateChangedConsumer<ISurrealEntity>>();

            x.UsingAmazonSqs((context, cfg) =>
            {
                var region = configuration["AWS:Region"] ?? throw new InvalidOperationException("AWS:Region configuration is required");
                var accessKey = configuration["AWS:AccessKey"] ?? throw new InvalidOperationException("AWS:AccessKey configuration is required");
                var secretKey = configuration["AWS:SecretKey"] ?? throw new InvalidOperationException("AWS:SecretKey configuration is required");

                cfg.Host(region, h =>
                {
                    h.AccessKey(accessKey);
                    h.SecretKey(secretKey);
                });

                // Get queue name prefix from configuration or use solution name
                var queuePrefix = configuration["AWS:QueuePrefix"] ?? "LiquidStorageCloud";
                
                // Configure consumer endpoints with prefix
                cfg.ReceiveEndpoint($"{queuePrefix}-entity-created-queue", e =>
                {
                    e.ConfigureConsumer<EntityCreatedConsumer<ISurrealEntity>>(context);
                    e.PrefetchCount = 16;
                    e.WaitTimeSeconds = 20;
                });

                cfg.ReceiveEndpoint($"{queuePrefix}-entity-updated-queue", e =>
                {
                    e.ConfigureConsumer<EntityUpdatedConsumer<ISurrealEntity>>(context);
                    e.PrefetchCount = 16;
                    e.WaitTimeSeconds = 20;
                });

                cfg.ReceiveEndpoint($"{queuePrefix}-entity-deleted-queue", e =>
                {
                    e.ConfigureConsumer<EntityDeletedConsumer<ISurrealEntity>>(context);
                    e.PrefetchCount = 16;
                    e.WaitTimeSeconds = 20;
                });

                cfg.ReceiveEndpoint($"{queuePrefix}-entity-solid-state-changed-queue", e =>
                {
                    e.ConfigureConsumer<EntitySolidStateChangedConsumer<ISurrealEntity>>(context);
                    e.PrefetchCount = 16;
                    e.WaitTimeSeconds = 20;
                });

                // Configure retry policy
                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)));

                // Configure circuit breaker
                cfg.UseCircuitBreaker(cb =>
                {
                    cb.TrackingPeriod = TimeSpan.FromMinutes(1);
                    cb.TripThreshold = 15;
                    cb.ActiveThreshold = 10;
                    cb.ResetInterval = TimeSpan.FromMinutes(5);
                });

                // Configure error handling
                cfg.UseMessageScope(context);
                cfg.UseInMemoryOutbox();

                // Configure endpoints using default naming
                cfg.ConfigureEndpoints(context);
            });
        });

        // Note: MassTransit hosted service is automatically registered in newer versions
        // services.AddMassTransitHostedService(); // This is obsolete

        return services;
    }

    /// <summary>
    /// Adds a specific entity event publisher to the service collection
    /// </summary>
    public static IServiceCollection AddEntityEventPublisher<T>(this IServiceCollection services)
        where T : class, ISurrealEntity
    {
        services.AddScoped<EntityEventPublisher<T>>();
        return services;
    }
}
