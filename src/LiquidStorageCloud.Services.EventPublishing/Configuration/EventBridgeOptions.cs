using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LiquidStorageCloud.Services.EventPublishing.Configuration
{
    /// <summary>
    /// Options used to configure AWS EventBridge connectivity and defaults.
    /// Bound from configuration path "AWS:EventBridge".
    /// </summary>
    public sealed class EventBridgeOptions
    {
        [JsonPropertyName("region")]
        public string Region { get; set; } = string.Empty;

        [JsonPropertyName("eventBusName")]
        public string EventBusName { get; set; } = string.Empty;

        /// <summary>
        /// Optional explicit access key (not recommended for prod; prefer IRSA or credential provider).
        /// </summary>
        [JsonPropertyName("accessKeyId")]
        public string? AccessKeyId { get; set; }

        /// <summary>
        /// Optional explicit secret key (not recommended for prod; prefer IRSA or credential provider).
        /// </summary>
        [JsonPropertyName("secretAccessKey")]
        public string? SecretAccessKey { get; set; }

        [JsonPropertyName("queuePrefix")]
        public string? QueuePrefix { get; set; }

        [JsonPropertyName("defaultShadowPercentage")]
        public int DefaultShadowPercentage { get; set; } = 0;

        /// <summary>
        /// When true, use IRSA (k8s ServiceAccount with IAM role) for credentials.
        /// If false, code will attempt standard SDK credential chain (env, shared credentials, etc).
        /// </summary>
        [JsonPropertyName("useIRSA")]
        public bool UseIRSA { get; set; } = true;
    }

    /// <summary>
    /// Options passed at publish time to control mode and per-call overrides.
    /// </summary>
    public sealed class EventPublishOptions
    {
        [JsonPropertyName("mode")]
        public PublisherMode Mode { get; set; } = PublisherMode.Default;

        /// <summary>
        /// When Mode == Shadow, percentage [0..100] of events forwarded to EventBridge.
        /// </summary>
        [JsonPropertyName("shadowPercentage")]
        public int ShadowPercentage { get; set; } = 0;

        [JsonPropertyName("partitionKey")]
        public string? PartitionKey { get; set; }

        [JsonPropertyName("additionalAttributes")]
        public IDictionary<string, string>? AdditionalAttributes { get; set; }
    }

    /// <summary>
    /// Publisher resolution and behavior modes used by factory and DualPublisher.
    /// </summary>
    public enum PublisherMode
    {
        /// <summary>
        /// Resolve by configuration/feature-flag defaults (backwards compatible).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Probabilistic shadow publishing.
        /// </summary>
        Shadow = 1,

        /// <summary>
        /// Send to both transports but rely on primary for business decisions.
        /// </summary>
        Dual = 2,

        /// <summary>
        /// Unconditional send to both transports.
        /// </summary>
        Parallel = 3,

        /// <summary>
        /// Only send to EventBridge.
        /// </summary>
        EventBridgeOnly = 4,

        /// <summary>
        /// Only send to existing RabbitMQ/SQS publisher.
        /// </summary>
        RabbitOnly = 5
    }
}
