using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuantumSkyLink.Shared.Eventing
{
    /// <summary>
    /// EventBridge implementation of IEventBus
    /// </summary>
    public class EventBridgeEventBus : IEventBus
    {
        private readonly IAmazonEventBridge _eventBridgeClient;
        private readonly ILogger<EventBridgeEventBus> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _environment;
        private readonly JsonSerializerOptions _jsonOptions;

        public EventBridgeEventBus(
            IAmazonEventBridge eventBridgeClient,
            ILogger<EventBridgeEventBus> logger,
            IConfiguration configuration)
        {
            _eventBridgeClient = eventBridgeClient ?? throw new ArgumentNullException(nameof(eventBridgeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _environment = configuration["AWS:Deployment:Environment"] ?? "development";
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task PublishAsync<T>(T @event, string eventSource, string detailType, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                var eventBusName = DetermineEventBus(eventSource, detailType);
                var eventDetail = JsonSerializer.Serialize(@event, _jsonOptions);

                var putEventsRequest = new PutEventsRequest
                {
                    Entries = new List<PutEventsRequestEntry>
                    {
                        new PutEventsRequestEntry
                        {
                            EventBusName = eventBusName,
                            Source = eventSource,
                            DetailType = detailType,
                            Detail = eventDetail,
                            Time = DateTime.UtcNow,
                            Resources = new List<string>()
                        }
                    }
                };

                // Add correlation ID if event is DomainEvent
                if (@event is DomainEvent domainEvent)
                {
                    putEventsRequest.Entries[0].Resources.Add($"correlation:{domainEvent.CorrelationId}");
                    
                    // Add trace context for observability
                    if (domainEvent.Metadata != null && domainEvent.Metadata.Any())
                    {
                        putEventsRequest.Entries[0].Detail = JsonSerializer.Serialize(new
                        {
                            Event = @event,
                            Metadata = domainEvent.Metadata
                        }, _jsonOptions);
                    }
                }

                _logger.LogInformation("Publishing event {DetailType} to EventBridge bus {EventBus}", 
                    detailType, eventBusName);

                var response = await _eventBridgeClient.PutEventsAsync(putEventsRequest, cancellationToken);

                if (response.FailedEntryCount > 0)
                {
                    var failedEntry = response.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.ErrorCode));
                    _logger.LogError("Failed to publish event {DetailType}: {ErrorCode} - {ErrorMessage}",
                        detailType, failedEntry?.ErrorCode, failedEntry?.ErrorMessage);
                    
                    throw new EventPublishException($"Failed to publish event: {failedEntry?.ErrorMessage}");
                }

                _logger.LogDebug("Successfully published event {DetailType} with ID {EventId}",
                    detailType, response.Entries[0].EventId);
            }
            catch (Exception ex) when (!(ex is EventPublishException))
            {
                _logger.LogError(ex, "Error publishing event {DetailType} to EventBridge", detailType);
                throw new EventPublishException($"Error publishing event: {ex.Message}", ex);
            }
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
        {
            if (@event is DomainEvent domainEvent)
            {
                await PublishAsync(@event, domainEvent.GetEventSource(), domainEvent.GetDetailType(), cancellationToken);
            }
            else
            {
                // Default source and type based on class name
                var typeName = typeof(T).Name;
                var source = $"qsl.{ExtractServiceName(typeName)}";
                var detailType = typeName;
                
                await PublishAsync(@event, source, detailType, cancellationToken);
            }
        }

        public Task<IDisposable> SubscribeAsync<T>(Func<T, CancellationToken, Task> handler) where T : class
        {
            // EventBridge doesn't support direct subscriptions
            // Consumers should use SQS queues via SQSConsumerService
            _logger.LogWarning("Direct subscription not supported for EventBridge. Use SQS consumer instead.");
            return Task.FromResult<IDisposable>(new NoOpDisposable());
        }

        private string DetermineEventBus(string eventSource, string detailType)
        {
            var projectName = _configuration["AWS:ProjectName"] ?? "qsl";
            
            // Determine bus based on event source/type
            if (eventSource.Contains("payment") || eventSource.Contains("treasury") || 
                eventSource.Contains("ledger") || detailType.Contains("Payment") || 
                detailType.Contains("Treasury") || detailType.Contains("Ledger"))
            {
                return $"{projectName}-{_environment}-financial";
            }
            else if (eventSource.Contains("blockchain") || eventSource.Contains("token") || 
                     detailType.Contains("Blockchain") || detailType.Contains("Token"))
            {
                return $"{projectName}-{_environment}-blockchain";
            }
            else if (eventSource.Contains("system") || eventSource.Contains("audit") || 
                     detailType.Contains("System") || detailType.Contains("Audit"))
            {
                return $"{projectName}-{_environment}-system";
            }
            else if (eventSource.Contains("marketplace") || eventSource.Contains("order") || 
                     detailType.Contains("Marketplace") || detailType.Contains("Order"))
            {
                return $"{projectName}-{_environment}-business";
            }
            else
            {
                // Default to core bus
                return $"{projectName}-{_environment}-core";
            }
        }

        private string ExtractServiceName(string typeName)
        {
            // Extract service name from event type name
            // E.g., "UserCreatedEvent" -> "user"
            var serviceName = typeName
                .Replace("Event", "")
                .Replace("Command", "")
                .Replace("Query", "");
            
            // Convert to lowercase and take first word
            var words = System.Text.RegularExpressions.Regex.Split(serviceName, @"(?<!^)(?=[A-Z])");
            return words.Length > 0 ? words[0].ToLowerInvariant() : "unknown";
        }

        private class NoOpDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    /// <summary>
    /// Exception thrown when event publishing fails
    /// </summary>
    public class EventPublishException : Exception
    {
        public EventPublishException(string message) : base(message) { }
        public EventPublishException(string message, Exception innerException) : base(message, innerException) { }
    }
}