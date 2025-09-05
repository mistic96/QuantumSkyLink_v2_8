using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuantumSkyLink.Shared.Eventing
{
    /// <summary>
    /// Abstraction for event bus to support both RabbitMQ and EventBridge
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to the event bus
        /// </summary>
        /// <typeparam name="T">Type of the event</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="eventSource">Source of the event (e.g., "qsl.payment")</param>
        /// <param name="detailType">Type of event detail (e.g., "Payment.Initiated")</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task PublishAsync<T>(T @event, string eventSource, string detailType, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publishes an event with automatic source and type detection
        /// </summary>
        Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Subscribe to events (for local/test scenarios)
        /// Note: In production, use SQS consumers instead
        /// </summary>
        Task<IDisposable> SubscribeAsync<T>(Func<T, CancellationToken, Task> handler) where T : class;
    }

    /// <summary>
    /// Base class for all domain events
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Gets the event source for EventBridge (e.g., "qsl.payment")
        /// </summary>
        public abstract string GetEventSource();

        /// <summary>
        /// Gets the detail type for EventBridge (e.g., "Payment.Initiated")
        /// </summary>
        public abstract string GetDetailType();
    }

    /// <summary>
    /// Event envelope for EventBridge compatibility
    /// </summary>
    public class EventEnvelope<T> where T : class
    {
        public string Version { get; set; } = "1.0";
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DetailType { get; set; }
        public string Source { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public T Detail { get; set; }
        public Dictionary<string, string> Resources { get; set; } = new();
    }
}