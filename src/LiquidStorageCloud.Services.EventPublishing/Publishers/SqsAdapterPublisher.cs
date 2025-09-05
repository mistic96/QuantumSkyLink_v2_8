using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using LiquidStorageCloud.Services.EventPublishing.Models;
using LiquidStorageCloud.Services.EventPublishing.Configuration;

namespace LiquidStorageCloud.Services.EventPublishing.Publishers
{
    /// <summary>
    /// Adapter that exposes an IPublishEndpoint (MassTransit -> SQS/Rabbit) as an IEventPublisher.
    /// Keeps existing publishing behavior unchanged while allowing the factory to return a uniform interface.
    /// </summary>
    public sealed class SqsAdapterPublisher : IEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<SqsAdapterPublisher> _logger;

        public SqsAdapterPublisher(IPublishEndpoint publishEndpoint, ILogger<SqsAdapterPublisher> logger)
        {
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions? options = null, CancellationToken ct = default)
        {
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            try
            {
                // If caller passed an EventEnvelope, publish the inner payload to preserve existing MassTransit contracts.
                if (@event is EventEnvelope envelope)
                {
                    _logger.LogDebug("SqsAdapterPublisher publishing envelope event {EventType} to MassTransit", envelope.EventType);
                    await _publishEndpoint.Publish(envelope.Payload, ct).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogDebug("SqsAdapterPublisher publishing event of type {Type} to MassTransit", typeof(TEvent).Name);
                    await _publishEndpoint.Publish(@event, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SqsAdapterPublisher publish canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SqsAdapterPublisher failed to publish message of type {Type}", typeof(TEvent).Name);
                throw;
            }
        }
    }
}
