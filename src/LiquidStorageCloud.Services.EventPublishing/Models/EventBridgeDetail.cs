using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiquidStorageCloud.Services.EventPublishing.Models
{
    /// <summary>
    /// EventBridge-specific payload wrapper used to build PutEvents entries.
    /// </summary>
    public sealed class EventBridgeDetail
    {
        [JsonPropertyName("detailType")]
        public string DetailType { get; set; } = string.Empty;

        /// <summary>
        /// JSON string representation of the payload/detail.
        /// </summary>
        [JsonPropertyName("detail")]
        public string Detail { get; set; } = "{}";

        [JsonPropertyName("eventBusName")]
        public string EventBusName { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("partitionKey")]
        public string? PartitionKey { get; set; }

        /// <summary>
        /// Construct from explicit values.
        /// </summary>
        public EventBridgeDetail() { }

        /// <summary>
        /// Validate fields and ensure Detail is well-formed JSON.
        /// Throws ArgumentException on invalid input.
        /// </summary>
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(DetailType))
                throw new ArgumentException("DetailType is required.", nameof(DetailType));

            if (DetailType.Length > 256)
                throw new ArgumentException("DetailType length must be <= 256 characters.", nameof(DetailType));

            if (string.IsNullOrWhiteSpace(EventBusName))
                throw new ArgumentException("EventBusName is required.", nameof(EventBusName));

            if (EventBusName.Length > 256)
                throw new ArgumentException("EventBusName length must be <= 256 characters.", nameof(EventBusName));

            if (string.IsNullOrWhiteSpace(Source))
                throw new ArgumentException("Source is required.", nameof(Source));

            if (Source.Length > 256)
                throw new ArgumentException("Source length must be <= 256 characters.", nameof(Source));

            // Ensure Detail is valid JSON
            try
            {
                using var doc = JsonDocument.Parse(Detail);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Detail must be a valid JSON string.", nameof(Detail), ex);
            }
        }
    }
}
