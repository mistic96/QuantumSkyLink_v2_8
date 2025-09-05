using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LiquidStorageCloud.Services.EventPublishing.Telemetry;
using LiquidStorageCloud.Services.EventPublishing.Publishers;

namespace LiquidStorageCloud.Services.EventPublishing.Extensions
{
    /// <summary>
    /// Registers publisher infrastructure helpers and diagnostic telemetry used by the publishing factory.
    /// Keep registrations additive and safe for existing services.
    /// </summary>
    public static class EventPublisherFactoryExtensions
    {
        /// <summary>
        /// Registers telemetry collector and adapter publishers required by EventPublisherFactory.
        /// Call this after AddEventPublishing to ensure configuration is bound.
        /// </summary>
        public static IServiceCollection AddEventPublisherInfrastructure(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Lightweight in-process telemetry collector for reconciliation diagnostics
            services.TryAddSingleton<TelemetryCollector>();

            // Register SQS adapter publisher as scoped so it can get scoped IPublishEndpoint from MassTransit
            services.TryAddScoped<SqsAdapterPublisher>();

            // EventBridgePublisher and DualPublisher may already be registered by AddEventPublishing; we don't override.
            // Ensure IEventPublisherFactory is registered by existing AddEventPublishing - if not, keep existing factory behaviour.
            services.TryAddScoped<IEventPublisherFactory, EventPublisherFactory>();

            return services;
        }
    }
}
