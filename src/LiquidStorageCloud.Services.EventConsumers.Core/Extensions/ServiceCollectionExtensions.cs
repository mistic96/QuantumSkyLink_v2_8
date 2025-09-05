using LiquidStorageCloud.Core.Database;
using LiquidStorageCloud.Services.EventConsumers.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace LiquidStorageCloud.Services.EventConsumers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventConsumers<T>(
        this IServiceCollection services,
        Action<EventConsumerOptions>? configureOptions = null) 
        where T : class, ISurrealEntity
    {
        var options = new EventConsumerOptions();
        configureOptions?.Invoke(options);

        services.AddMassTransit(x =>
        {
            // Register entity event consumers for specific type
            x.AddConsumer<EntityCreatedConsumer<T>>();
            x.AddConsumer<EntityUpdatedConsumer<T>>();
            x.AddConsumer<EntityDeletedConsumer<T>>();
            x.AddConsumer<EntitySolidStateChangedConsumer<T>>();

            x.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(options.Region ?? "us-east-1", h =>
                {
                    h.AccessKey(options.AccessKey);
                    h.SecretKey(options.SecretKey);
                });

                // Configure consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}

public class EventConsumerOptions
{
    public string? Region { get; set; }
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
