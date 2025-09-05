using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using RefitClient.Interfaces;
using RefitClient.Models;
using RefitClient.Resilience;

namespace RefitClient.Extensions;

/// <summary>
/// Extension methods for registering Refit clients in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a Refit client for the specified service interface.
    /// </summary>
    /// <typeparam name="TClient">The type of the service client interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service to connect to.</param>
    /// <returns>The HTTP client builder for further configuration.</returns>
    public static IHttpClientBuilder AddRefitClient<TClient>(
        this IServiceCollection services,
        string serviceName) where TClient : class, IServiceClient
    {
        return services.AddRefitClient<TClient>(options => 
        {
            options.UseServiceDiscovery = true;
        }, serviceName);
    }

    /// <summary>
    /// Adds a Refit client for the specified service interface with custom options.
    /// </summary>
    /// <typeparam name="TClient">The type of the service client interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">A delegate to configure the client options.</param>
    /// <param name="serviceName">The name of the service to connect to.</param>
    /// <returns>The HTTP client builder for further configuration.</returns>
    public static IHttpClientBuilder AddRefitClient<TClient>(
        this IServiceCollection services,
        Action<RefitClientOptions> configureOptions,
        string serviceName) where TClient : class, IServiceClient
    {
        var options = new RefitClientOptions();
        configureOptions(options);

        // Register the options
        services.Configure<RefitClientOptions>(serviceName, opt => 
        {
            opt.BaseUrl = options.BaseUrl;
            opt.Timeout = options.Timeout;
            opt.UseAuthentication = options.UseAuthentication;
            opt.AuthenticationScheme = options.AuthenticationScheme;
            opt.UseResiliencePolicies = options.UseResiliencePolicies;
            opt.UseServiceDiscovery = options.UseServiceDiscovery;
            opt.DefaultHeaders = options.DefaultHeaders;
        });

        // Configure Refit settings
        var settings = new RefitSettings();

        // Register the Refit client
        var builder = services.AddRefitClient<TClient>(settings)
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var clientOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RefitClientOptions>>()
                    .Get(serviceName);

                // Configure base address
                if (!string.IsNullOrEmpty(clientOptions.BaseUrl))
                {
                    client.BaseAddress = new Uri(clientOptions.BaseUrl);
                }
                else if (clientOptions.UseServiceDiscovery)
                {
                    // Use service discovery with the service name
                    client.BaseAddress = new Uri($"http://{serviceName}");
                }

                // Configure timeout
                client.Timeout = clientOptions.Timeout;

                // Add default headers
                if (clientOptions.DefaultHeaders != null)
                {
                    foreach (var header in clientOptions.DefaultHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            });

        // Add resilience policies if enabled
        if (options.UseResiliencePolicies)
        {
            builder = (IHttpClientBuilder)builder.AddStandardResilienceHandler();
        }

        // Add service discovery if enabled
        if (options.UseServiceDiscovery)
        {
            // In a real implementation, we would add service discovery here
            // For now, we'll just return the builder
            // When the Microsoft.Extensions.ServiceDiscovery package is properly referenced,
            // this method can be enhanced to use the full service discovery capabilities
        }

        return builder;
    }
}
