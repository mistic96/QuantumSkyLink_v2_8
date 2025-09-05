using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiquidStorageCloud.Services.EventPublishing.Models
{
    /// <summary>
    /// Lightweight envelope used across publishers to normalize domain events before transport mapping.
    /// </summary>
    public sealed class EventEnvelope
    {
        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("eventId")]
        public Guid EventId { get; set; } = Guid.NewGuid();

        [JsonPropertyName("payload")]
        public object Payload { get; set; } = default!;

        [JsonPropertyName("metadata")]
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Validate the envelope according to plan rules. Throws ArgumentException on invalid data.
        /// </summary>
        public void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(EventType))
                throw new ArgumentException("EventType is required.", nameof(EventType));

            if (EventType.Length > 256)
                throw new ArgumentException("EventType length must be <= 256 characters.", nameof(EventType));

            if (!IsAscii(EventType))
                throw new ArgumentException("EventType must be ASCII-safe.", nameof(EventType));

            if (string.IsNullOrWhiteSpace(Source))
                throw new ArgumentException("Source is required.", nameof(Source));

            if (Source.Length > 256)
                throw new ArgumentException("Source length must be <= 256 characters.", nameof(Source));

            if (!IsAscii(Source))
                throw new ArgumentException("Source must be ASCII-safe.", nameof(Source));

            if (Payload is null)
                throw new ArgumentException("Payload is required and must be serializable.", nameof(Payload));

            // Ensure payload is serializable with System.Text.Json
            try
            {
                JsonSerializer.Serialize(Payload);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Payload must be serializable to JSON.", nameof(Payload), ex);
            }

            if (Metadata != null)
            {
                var keys = new List<string>(Metadata.Keys);
                foreach (var k in keys)
                {
                    if (string.IsNullOrEmpty(k) || !IsAscii(k))
                        throw new ArgumentException("Metadata keys must be ASCII-only and non-empty.", nameof(Metadata));

                    var value = Metadata[k] ?? string.Empty;
                    if (value.Length > 1024)
                    {
                        // Trim to 1024 characters to enforce constraint
                        Metadata[k] = value.Substring(0, 1024);
                    }
                }
            }
        }

        private static bool IsAscii(string s)
        {
            // Fast check: ASCII bytes count equals length
            return Encoding.ASCII.GetByteCount(s) == s.Length;
        }
    }
}
