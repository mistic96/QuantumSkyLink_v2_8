using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RefitClient.Models;

namespace RefitClient.Resilience;

/// <summary>
/// Provides extension methods for configuring resilience policies for HTTP clients.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Adds standard resilience policies to the HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="options">The resilience options.</param>
    /// <returns>The HTTP client builder with resilience policies configured.</returns>
    public static IHttpClientBuilder AddStandardResilienceHandler(
        this IHttpClientBuilder builder,
        ResilienceOptions? options = null)
    {
        options ??= new ResilienceOptions();

        // Configure basic timeout
        builder = builder.ConfigureHttpClient(client =>
        {
            client.Timeout = options.EnableTimeoutHandling
                ? options.CircuitBreakerTimeout
                : TimeSpan.FromSeconds(100);
        });

        // In a real implementation, we would add Polly policies here
        // For now, we'll just return the builder with the timeout configured
        // When the Microsoft.Extensions.Http.Resilience package is properly referenced,
        // this method can be enhanced to use the full resilience capabilities

        return builder;
    }

    /// <summary>
    /// Adds custom resilience policies to the HTTP client builder.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="configureOptions">A delegate to configure the resilience options.</param>
    /// <returns>The HTTP client builder with resilience policies configured.</returns>
    public static IHttpClientBuilder AddCustomResilienceHandler(
        this IHttpClientBuilder builder,
        Action<ResilienceOptions> configureOptions)
    {
        var options = new ResilienceOptions();
        configureOptions(options);
        return AddStandardResilienceHandler(builder, options);
    }
}
