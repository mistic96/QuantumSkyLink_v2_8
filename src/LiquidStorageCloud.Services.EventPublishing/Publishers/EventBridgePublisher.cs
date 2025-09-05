using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LiquidStorageCloud.Services.EventPublishing.Configuration;
using LiquidStorageCloud.Services.EventPublishing.Models;
using LiquidStorageCloud.Services.EventPublishing.Telemetry;

namespace LiquidStorageCloud.Services.EventPublishing.Publishers
{
    /// <summary>
    /// Implements IEventPublisher using AWS EventBridge PutEvents API.
    /// Minimal, testable implementation with simple retry/backoff.
    /// </summary>
    public sealed class EventBridgePublisher : IEventPublisher
    {
        private readonly IAmazonEventBridge _client;
        private readonly EventBridgeOptions _options;
        private readonly ILogger<EventBridgePublisher> _logger;

        // Simple retry parameters (small and conservative)
        private const int MaxAttempts = 3;
        private static readonly TimeSpan InitialBackoff = TimeSpan.FromMilliseconds(200);

        public EventBridgePublisher(IAmazonEventBridge client, IOptions<EventBridgeOptions> options, ILogger<EventBridgePublisher> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publish a domain event to EventBridge.
        /// Uses PublisherMappings to build entries and sends via PutEvents API.
        /// Throws ArgumentException for validation errors; throws on unrecoverable transport errors.
        /// </summary>
        public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions? options = null, CancellationToken ct = default)
        {
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            // Accept either EventEnvelope or raw event; if raw, wrap into EventEnvelope with best-effort metadata.
            EventEnvelope envelope = @event as EventEnvelope ?? new EventEnvelope
            {
                EventType = @event?.GetType().Name ?? "Unknown",
                Source = _options.QueuePrefix ?? "quantumskylink.unknown",
                Payload = @event!
            };

            envelope.ValidateOrThrow();

            // Build DTO entries
            var publishOptions = options ?? new EventPublishOptions
            {
                Mode = PublisherMode.Default,
                ShadowPercentage = _options.DefaultShadowPercentage
            };

            var dtos = PublisherMappings.MapEnvelopeToPutEventEntries(envelope, _options, publishOptions);

            // Map DTOs -> AWS PutEventsRequestEntry
            var entries = dtos.Select(d => new PutEventsRequestEntry
            {
                Detail = d.Detail,
                DetailType = d.DetailType,
                EventBusName = d.EventBusName,
                Source = d.Source,
                // PutEventsRequestEntry doesn't have PartitionKey property in AWS SDK; use Trace/EventBus or Detail for grouping if needed.
            }).ToList();

            if (!entries.Any())
            {
                _logger.LogWarning("No PutEvents entries generated for envelope {EventId}", envelope.EventId);
                return;
            }

            // Send with simple retry/backoff
            int attempt = 0;
            var backoff = InitialBackoff;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                attempt++;

                try
                {
                    var request = new PutEventsRequest
                    {
                        Entries = entries
                    };

                    var response = await _client.PutEventsAsync(request, ct).ConfigureAwait(false);

                    if (response == null)
                    {
                        throw new Exception("PutEvents returned null response.");
                    }

                    // Check for failed entries
                    var failedCount = response.Entries?.Count(e => !string.IsNullOrEmpty(e.ErrorCode)) ?? 0;
                    if (failedCount > 0)
                    {
                        // Log errors and decide: retryable? For now, treat as transient and retry up to MaxAttempts.
                        _logger.LogWarning("PutEvents returned {Failed} failed entries on attempt {Attempt}. FirstError: {Error}", failedCount, attempt, response.Entries?.FirstOrDefault(e => !string.IsNullOrEmpty(e.ErrorCode))?.ErrorMessage);

                        if (attempt >= MaxAttempts)
                        {
                            // Construct aggregate exception with error details
                            var errors = string.Join(";", response.Entries?.Where(e => !string.IsNullOrEmpty(e.ErrorCode)).Select(e => $"{e.ErrorCode}:{e.ErrorMessage}") ?? Array.Empty<string>());
                            throw new Exception($"PutEvents failed for {failedCount} entries after {attempt} attempts. Errors: {errors}");
                        }

                        // backoff then retry
                        await Task.Delay(backoff, ct).ConfigureAwait(false);
                        backoff = TimeSpan.FromMilliseconds(backoff.TotalMilliseconds * 2);
                        continue;
                    }

                    // Success path
                    _logger.LogDebug("Successfully published {Count} entries to EventBridge (bus: {Bus}) for event {EventId}", entries.Count, _options.EventBusName, envelope.EventId);
                    return;
                }
                catch (AmazonEventBridgeException aex) when (attempt < MaxAttempts)
                {
                    _logger.LogWarning(aex, "AWS EventBridge exception on attempt {Attempt}, will retry.", attempt);
                    await Task.Delay(backoff, ct).ConfigureAwait(false);
                    backoff = TimeSpan.FromMilliseconds(backoff.TotalMilliseconds * 2);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Publish cancelled for event {EventId}", envelope.EventId);
                    throw;
                }
                catch (Exception ex)
                {
                    // If max attempts reached or unrecoverable, rethrow as transport error
                    _logger.LogError(ex, "Unrecoverable error publishing to EventBridge for event {EventId}", envelope.EventId);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Minimal IEventPublisher contract used by publishers.
    /// Kept internal to avoid touching other projects—if a public interface exists, align with that.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions? options = null, CancellationToken ct = default);
    }
}
