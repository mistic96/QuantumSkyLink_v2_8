using System;

namespace RefitClient.Models;

/// <summary>
/// Configuration options for HTTP client resilience policies.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// Default is 3.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial backoff delay for retries.
    /// Default is 1 second.
    /// </summary>
    public TimeSpan InitialBackoff { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the maximum backoff delay for retries.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout for circuit breaker to trip.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the failure threshold for the circuit breaker.
    /// Default is 0.5 (50%).
    /// </summary>
    public double FailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// Gets or sets the minimum throughput before circuit breaker activates.
    /// Default is 10 requests.
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Gets or sets the sampling duration for circuit breaker.
    /// Default is 100 seconds.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets whether to enable timeout handling.
    /// Default is true.
    /// </summary>
    public bool EnableTimeoutHandling { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable circuit breaker.
    /// Default is true.
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable retry handling.
    /// Default is true.
    /// </summary>
    public bool EnableRetryHandling { get; set; } = true;
}
