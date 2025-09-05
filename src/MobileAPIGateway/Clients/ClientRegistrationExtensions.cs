using Refit;
using MobileAPIGateway.Clients.Interfaces;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Extension methods for registering clients
/// </summary>
public static class ClientRegistrationExtensions
{
    /// <summary>
    /// Adds service clients to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Auth service client
        services.AddRefitClient<IAuthServiceClient>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(configuration["ServiceUrls:AuthService"] ?? "http://localhost:5001");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        // Add more service clients here as needed

        return services;
    }
}
