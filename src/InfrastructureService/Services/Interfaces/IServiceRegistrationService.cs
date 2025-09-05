using InfrastructureService.Models;

namespace InfrastructureService.Services.Interfaces;

/// <summary>
/// Interface for service registration operations
/// </summary>
public interface IServiceRegistrationService
{
    /// <summary>
    /// Registers a new service
    /// </summary>
    /// <param name="serviceRegistration">Service registration details</param>
    /// <returns>The registered service</returns>
    Task<ServiceRegistration> RegisterServiceAsync(ServiceRegistration serviceRegistration);

    /// <summary>
    /// Gets a service by name
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>Service registration or null if not found</returns>
    Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName);

    /// <summary>
    /// Gets a service by ID
    /// </summary>
    /// <param name="serviceId">ID of the service</param>
    /// <returns>Service registration or null if not found</returns>
    Task<ServiceRegistration?> GetServiceByIdAsync(string serviceId);

    /// <summary>
    /// Gets a service by address
    /// </summary>
    /// <param name="serviceAddress">Address of the service</param>
    /// <returns>Service registration or null if not found</returns>
    Task<ServiceRegistration?> GetServiceByAddressAsync(string serviceAddress);

    /// <summary>
    /// Gets all registered services
    /// </summary>
    /// <returns>List of all service registrations</returns>
    Task<List<ServiceRegistration>> GetAllServicesAsync();

    /// <summary>
    /// Gets services by type
    /// </summary>
    /// <param name="serviceType">Type of services to retrieve</param>
    /// <returns>List of services of the specified type</returns>
    Task<List<ServiceRegistration>> GetServicesByTypeAsync(string serviceType);

    /// <summary>
    /// Gets active services only
    /// </summary>
    /// <returns>List of active service registrations</returns>
    Task<List<ServiceRegistration>> GetActiveServicesAsync();

    /// <summary>
    /// Updates a service registration
    /// </summary>
    /// <param name="serviceRegistration">Updated service registration</param>
    /// <returns>The updated service</returns>
    Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration serviceRegistration);

    /// <summary>
    /// Deactivates a service
    /// </summary>
    /// <param name="serviceName">Name of the service to deactivate</param>
    /// <returns>True if successful</returns>
    Task<bool> DeactivateServiceAsync(string serviceName);

    /// <summary>
    /// Activates a service
    /// </summary>
    /// <param name="serviceName">Name of the service to activate</param>
    /// <returns>True if successful</returns>
    Task<bool> ActivateServiceAsync(string serviceName);

    /// <summary>
    /// Deletes a service registration
    /// </summary>
    /// <param name="serviceName">Name of the service to delete</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteServiceAsync(string serviceName);

    /// <summary>
    /// Checks if a service is registered
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>True if the service is registered</returns>
    Task<bool> IsServiceRegisteredAsync(string serviceName);

    /// <summary>
    /// Gets services that need health checks
    /// </summary>
    /// <param name="maxAge">Maximum age since last health check</param>
    /// <returns>List of services needing health checks</returns>
    Task<List<ServiceRegistration>> GetServicesNeedingHealthCheckAsync(TimeSpan maxAge);

    /// <summary>
    /// Gets service registration statistics
    /// </summary>
    /// <returns>Registration statistics</returns>
    Task<ServiceRegistrationStats> GetRegistrationStatsAsync();
}

/// <summary>
/// Service registration statistics
/// </summary>
public class ServiceRegistrationStats
{
    /// <summary>
    /// Total number of registered services
    /// </summary>
    public int TotalServices { get; set; }

    /// <summary>
    /// Number of active services
    /// </summary>
    public int ActiveServices { get; set; }

    /// <summary>
    /// Number of inactive services
    /// </summary>
    public int InactiveServices { get; set; }

    /// <summary>
    /// Services by type
    /// </summary>
    public Dictionary<string, int> ServicesByType { get; set; } = new();

    /// <summary>
    /// Services with quantum-resistant keys
    /// </summary>
    public int ServicesWithQuantumKeys { get; set; }

    /// <summary>
    /// Services with traditional keys only
    /// </summary>
    public int ServicesWithTraditionalKeysOnly { get; set; }

    /// <summary>
    /// Average key rotation count
    /// </summary>
    public double AverageKeyRotations { get; set; }

    /// <summary>
    /// Services needing health checks
    /// </summary>
    public int ServicesNeedingHealthCheck { get; set; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
