using InfrastructureService.Models;
using InfrastructureService.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace InfrastructureService.Services;

/// <summary>
/// Service for managing service registrations with quantum-resistant operational keys
/// </summary>
public class ServiceRegistrationService : IServiceRegistrationService
{
    private readonly ILogger<ServiceRegistrationService> _logger;
    private readonly ConcurrentDictionary<string, ServiceRegistration> _services;
    private readonly object _lockObject = new();

    public ServiceRegistrationService(ILogger<ServiceRegistrationService> logger)
    {
        _logger = logger;
        _services = new ConcurrentDictionary<string, ServiceRegistration>();
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistration> RegisterServiceAsync(ServiceRegistration serviceRegistration)
    {
        try
        {
            _logger.LogInformation("Registering service {ServiceName} with address {ServiceAddress}", 
                serviceRegistration.ServiceName, serviceRegistration.ServiceAddress);

            // Generate ID if not set
            if (string.IsNullOrEmpty(serviceRegistration.Id))
            {
                serviceRegistration.Id = Guid.NewGuid().ToString();
            }

            // Ensure timestamps are set
            serviceRegistration.RegisteredAt = DateTime.UtcNow;
            serviceRegistration.UpdatedAt = DateTime.UtcNow;
            serviceRegistration.LastModified = DateTimeOffset.UtcNow;

            // Add default tags based on service type
            if (!serviceRegistration.Tags.Any())
            {
                serviceRegistration.Tags.Add(serviceRegistration.ServiceType.ToLowerInvariant());
                serviceRegistration.Tags.Add("quantum-resistant");
                serviceRegistration.Tags.Add("aspire-managed");
            }

            // Add default capabilities
            if (!serviceRegistration.Capabilities.Any())
            {
                serviceRegistration.Capabilities.AddRange(new[]
                {
                    "dilithium-signatures",
                    "falcon-signatures", 
                    "ec256-signatures",
                    "health-monitoring",
                    "key-rotation"
                });
            }

            _services.AddOrUpdate(serviceRegistration.ServiceName, serviceRegistration, (key, existing) => serviceRegistration);
            
            _logger.LogInformation("Successfully registered service {ServiceName} with ID {ServiceId}", 
                serviceRegistration.ServiceName, serviceRegistration.Id);

            return serviceRegistration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service {ServiceName}", serviceRegistration.ServiceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistration?> GetServiceByNameAsync(string serviceName)
    {
        try
        {
            _services.TryGetValue(serviceName, out var service);
            return await Task.FromResult(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service by name {ServiceName}", serviceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistration?> GetServiceByIdAsync(string serviceId)
    {
        try
        {
            var service = _services.Values.FirstOrDefault(s => s.Id == serviceId);
            return await Task.FromResult(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service by ID {ServiceId}", serviceId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistration?> GetServiceByAddressAsync(string serviceAddress)
    {
        try
        {
            var service = _services.Values.FirstOrDefault(s => s.ServiceAddress == serviceAddress);
            return await Task.FromResult(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service by address {ServiceAddress}", serviceAddress);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ServiceRegistration>> GetAllServicesAsync()
    {
        try
        {
            var services = _services.Values.OrderByDescending(s => s.RegisteredAt).ToList();
            return await Task.FromResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all services");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ServiceRegistration>> GetServicesByTypeAsync(string serviceType)
    {
        try
        {
            var services = _services.Values
                .Where(s => s.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.ServiceName)
                .ToList();
            return await Task.FromResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services by type {ServiceType}", serviceType);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ServiceRegistration>> GetActiveServicesAsync()
    {
        try
        {
            var services = _services.Values
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToList();
            return await Task.FromResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active services");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistration> UpdateServiceAsync(ServiceRegistration serviceRegistration)
    {
        try
        {
            _logger.LogInformation("Updating service {ServiceName}", serviceRegistration.ServiceName);

            // Update timestamps
            serviceRegistration.Touch();

            _services.AddOrUpdate(serviceRegistration.ServiceName, serviceRegistration, (key, existing) => serviceRegistration);
            
            _logger.LogInformation("Successfully updated service {ServiceName}", serviceRegistration.ServiceName);
            return serviceRegistration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service {ServiceName}", serviceRegistration.ServiceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateServiceAsync(string serviceName)
    {
        try
        {
            var service = await GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return false;
            }

            service.IsActive = false;
            service.Touch();

            _services.AddOrUpdate(serviceName, service, (key, existing) => service);
            
            _logger.LogInformation("Deactivated service {ServiceName}", serviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating service {ServiceName}", serviceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ActivateServiceAsync(string serviceName)
    {
        try
        {
            var service = await GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return false;
            }

            service.IsActive = true;
            service.Touch();

            _services.AddOrUpdate(serviceName, service, (key, existing) => service);
            
            _logger.LogInformation("Activated service {ServiceName}", serviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating service {ServiceName}", serviceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteServiceAsync(string serviceName)
    {
        try
        {
            var service = await GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return false;
            }

            _services.TryRemove(serviceName, out _);
            
            _logger.LogInformation("Deleted service {ServiceName}", serviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service {ServiceName}", serviceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsServiceRegisteredAsync(string serviceName)
    {
        try
        {
            var service = await GetServiceByNameAsync(serviceName);
            return service != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if service {ServiceName} is registered", serviceName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<ServiceRegistration>> GetServicesNeedingHealthCheckAsync(TimeSpan maxAge)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(maxAge);
            var services = _services.Values
                .Where(s => s.IsActive && (s.LastHealthCheck == null || s.LastHealthCheck < cutoffTime))
                .OrderBy(s => s.LastHealthCheck ?? DateTime.MinValue)
                .ToList();
            return await Task.FromResult(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting services needing health check");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceRegistrationStats> GetRegistrationStatsAsync()
    {
        try
        {
            var allServices = await GetAllServicesAsync();
            
            var stats = new ServiceRegistrationStats
            {
                TotalServices = allServices.Count,
                ActiveServices = allServices.Count(s => s.IsActive),
                InactiveServices = allServices.Count(s => !s.IsActive),
                ServicesWithQuantumKeys = allServices.Count(s => s.SupportsQuantumResistant),
                ServicesWithTraditionalKeysOnly = allServices.Count(s => s.SupportsTraditional && !s.SupportsQuantumResistant),
                AverageKeyRotations = allServices.Any() ? allServices.Average(s => s.KeyRotationCount) : 0,
                ServicesNeedingHealthCheck = (await GetServicesNeedingHealthCheckAsync(TimeSpan.FromHours(1))).Count
            };

            // Group by service type
            stats.ServicesByType = allServices
                .GroupBy(s => s.ServiceType)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registration statistics");
            throw;
        }
    }
}
