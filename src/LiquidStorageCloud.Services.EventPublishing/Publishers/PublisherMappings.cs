using System;
using System.Collections.Generic;
using System.Text.Json;
using LiquidStorageCloud.Services.EventPublishing.Models;
using LiquidStorageCloud.Services.EventPublishing.Configuration;

namespace LiquidStorageCloud.Services.EventPublishing.Publishers
{
    /// <summary>
    /// Mapping helpers that convert EventEnvelope -> EventBridgeDetail and create lightweight DTOs suitable for constructing AWS SDK PutEvents entries.
    /// Keeps mapping logic independent of AWSSDK types to simplify testing and avoid early package coupling.
    /// </summary>
    public static class PublisherMappings
    {
        /// <summary>
        /// Map an EventEnvelope to an EventBridgeDetail using the provided eventBus name and optional partition key.
        /// </summary>
        public static EventBridgeDetail MapToEventBridgeDetail(EventEnvelope envelope, string eventBusName, string? partitionKey = null)
        {
            if (envelope is null) throw new ArgumentNullException(nameof(envelope));
            envelope.ValidateOrThrow();

            var detail = new EventBridgeDetail
            {
                DetailType = envelope.EventType,
                EventBusName = eventBusName,
                Source = envelope.Source,
                PartitionKey = partitionKey ?? (envelope.Metadata?.ContainsKey("partitionKey") == true ? envelope.Metadata?["partitionKey"] : null)
            };

            // Serialize payload to JSON using System.Text.Json with default options.
            detail.Detail = JsonSerializer.Serialize(envelope.Payload);

            // Validate resulting DTO (will throw if JSON invalid)
            detail.ValidateOrThrow();

            return detail;
        }

        /// <summary>
        /// Lightweight DTO representing fields required to construct a PutEventsRequestEntry.
        /// This avoids a compile-time dependency on the AWSSDK types at mapping time.
        /// </summary>
        public sealed class PutEventEntryDto
        {
            public string Detail { get; init; } = "{}";
            public string DetailType { get; init; } = string.Empty;
            public string EventBusName { get; init; } = string.Empty;
            public string Source { get; init; } = string.Empty;
            public string? PartitionKey { get; init; }
            public IDictionary<string, string>? AdditionalAttributes { get; init; }
        }

        /// <summary>
        /// Convert an EventBridgeDetail into one or more PutEventEntryDto entries.
        /// Currently returns a single entry; preserved as a list to allow future splitting for large payloads.
        /// </summary>
        public static IReadOnlyList<PutEventEntryDto> BuildPutEventEntries(EventBridgeDetail detail, IDictionary<string, string>? additionalAttributes = null)
        {
            if (detail is null) throw new ArgumentNullException(nameof(detail));
            detail.ValidateOrThrow();

            var entry = new PutEventEntryDto
            {
                Detail = detail.Detail,
                DetailType = detail.DetailType,
                EventBusName = detail.EventBusName,
                Source = detail.Source,
                PartitionKey = detail.PartitionKey,
                AdditionalAttributes = additionalAttributes
            };

            return new[] { entry };
        }

        /// <summary>
        /// Convenience: map directly from EventEnvelope -> PutEventEntryDto list.
        /// </summary>
        public static IReadOnlyList<PutEventEntryDto> MapEnvelopeToPutEventEntries(EventEnvelope envelope, EventBridgeOptions options, EventPublishOptions? publishOptions = null)
        {
            if (envelope is null) throw new ArgumentNullException(nameof(envelope));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var eventBus = options.EventBusName ?? throw new ArgumentException("EventBridgeOptions.EventBusName must be set", nameof(options));
            var partitionKey = publishOptions?.PartitionKey;

            var detail = MapToEventBridgeDetail(envelope, eventBus, partitionKey);
            var attributes = publishOptions?.AdditionalAttributes;

            return BuildPutEventEntries(detail, attributes);
        }
    }
}
