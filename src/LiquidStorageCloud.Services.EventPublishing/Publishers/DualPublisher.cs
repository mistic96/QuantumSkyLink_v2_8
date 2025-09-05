using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LiquidStorageCloud.Services.EventPublishing.Configuration;
using LiquidStorageCloud.Services.EventPublishing.Models;
using LiquidStorageCloud.Services.EventPublishing.Telemetry;

namespace LiquidStorageCloud.Services.EventPublishing.Publishers
{
    /// <summary>
    /// Wraps a primary and secondary IEventPublisher and routes events according to PublisherMode.
    /// Minimal, safe behavior: never drop primary events; secondary failures are logged and do not block primary unless configured.
    /// </summary>
    public sealed class DualPublisher : IEventPublisher
    {
        private readonly IEventPublisher _primary;
        private readonly IEventPublisher _secondary;
        private readonly ILogger<DualPublisher> _logger;
        private readonly Random _rng = new Random();

        public DualPublisher(IEventPublisher primary, IEventPublisher secondary, ILogger<DualPublisher> logger)
        {
            _primary = primary ?? throw new ArgumentNullException(nameof(primary));
            _secondary = secondary ?? throw new ArgumentNullException(nameof(secondary));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync<TEvent>(TEvent @event, EventPublishOptions? options = null, CancellationToken ct = default)
        {
            if (@event is null) throw new ArgumentNullException(nameof(@event));

            var publishOptions = options ?? new EventPublishOptions();

            // Resolve effective mode
            var mode = publishOptions.Mode;

            // Ensure default semantics: if Default, treat as RabbitOnly to preserve existing behavior
            if (mode == PublisherMode.Default)
            {
                mode = PublisherMode.RabbitOnly;
            }

            // Helper to run secondary probabilistically
            async Task TrySecondaryAsync()
            {
                try
                {
                    await _secondary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("DualPublisher secondary publish canceled.");
                    throw;
                }
                catch (Exception ex)
                {
                    // Secondary failure should not block primary; log for reconciliation
                    _logger.LogWarning(ex, "DualPublisher secondary publisher failed");
                }
            }

            switch (mode)
            {
                case PublisherMode.RabbitOnly:
                    await _primary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                    return;

                case PublisherMode.EventBridgeOnly:
                    await _secondary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                    return;

                case PublisherMode.Parallel:
                    // Fire both and wait for both to complete; if secondary fails we log but still propagate primary result
                    var primaryTask = _primary.PublishAsync(@event, options, ct);
                    var secondaryTask = _secondary.PublishAsync(@event, options, ct);
                    await Task.WhenAll(primaryTask, secondaryTask).ConfigureAwait(false);
                    return;

                case PublisherMode.Dual:
                    // Ensure primary completes; start secondary but do not fail primary if secondary fails.
                    await _primary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                    _ = TrySecondaryAsync(); // intentionally not awaited
                    return;

                case PublisherMode.Shadow:
                    // Publish to primary always, and probabilistically publish to secondary per ShadowPercentage
                    await _primary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                    var percentage = publishOptions.ShadowPercentage;
                    if (percentage <= 0)
                    {
                        return;
                    }

                    var roll = _rng.Next(0, 100) + 1; // 1..100
                    if (roll <= percentage)
                    {
                        _ = TrySecondaryAsync();
                    }

                    return;

                default:
                    // Fallback to primary-only
                    await _primary.PublishAsync(@event, options, ct).ConfigureAwait(false);
                    return;
            }
        }
    }
}
