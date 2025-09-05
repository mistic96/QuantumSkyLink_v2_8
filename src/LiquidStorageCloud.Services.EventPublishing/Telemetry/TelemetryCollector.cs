using System;
using System.Collections.Concurrent;

namespace LiquidStorageCloud.Services.EventPublishing.Telemetry
{
    /// <summary>
    /// Lightweight in-process telemetry counter used by publishers for reconciliation diagnostics.
    /// Designed to be very small and only for operational telemetry / tests. Real deployments should
    /// wire this to CloudWatch, Prometheus, or other metrics sinks.
    /// </summary>
    public sealed class TelemetryCollector
    {
        private readonly ConcurrentDictionary<string, long> _counters = new();

        public void Increment(string key, long value = 1)
        {
            _counters.AddOrUpdate(key, value, (_, existing) => existing + value);
        }

        public long Get(string key)
        {
            _counters.TryGetValue(key, out var v);
            return v;
        }

        public (string Key, long Value)[] Snapshot()
        {
            var arr = new (string, long)[_counters.Count];
            var i = 0;
            foreach (var kv in _counters)
            {
                arr[i++] = (kv.Key, kv.Value);
            }

            return arr;
        }

        // Convenience helpers
        public void IncrementPublishAttempt() => Increment("publish.attempt");
        public void IncrementPublishSuccess() => Increment("publish.success");
        public void IncrementPublishFailure() => Increment("publish.failure");
        public void IncrementSecondaryAttempt() => Increment("publish.secondary.attempt");
        public void IncrementSecondarySuccess() => Increment("publish.secondary.success");
        public void IncrementSecondaryFailure() => Increment("publish.secondary.failure");
    }
}
