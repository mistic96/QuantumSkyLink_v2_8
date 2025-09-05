using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InfrastructureService.Models;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using QuantumLedger.Cryptography.Interfaces;
using QuantumLedger.Cryptography.Models;

namespace InfrastructureService.Controllers;

/// <summary>
/// Controller for managing service registration with quantum-resistant cryptographic keys
/// Phase 2: Service Registration with Operational Keys
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Services need to register without authentication initially
public class ServiceRegistrationController : ControllerBase
{
    private readonly IServiceRegistrationService _serviceRegistrationService;
    private readonly IKeyManager _keyManager;
    private readonly ILogger<ServiceRegistrationController> _logger;

    // All 24 QuantumSkyLink v2 microservices
    private static readonly HashSet<string> ValidServiceNames = new()
    {
        // Core Domain Services
        "UserService", "AccountService", "ComplianceService", "FeeService", 
        "SecurityService", "TokenService", "GovernanceService",
        
        // Financial Services
        "TreasuryService", "PaymentGatewayService", "LiquidationService",
        
        // Infrastructure Services
        "InfrastructureService", "IdentityVerificationService",
        
        // Supporting Services
        "AIReviewService", "NotificationService",
        
        // Zero-Trust Security Services
        "SignatureService",
        
        // Workflow Orchestration Service
        "OrchestrationService",
        
        // Marketplace Service
        "MarketplaceService",
        
        // API Gateways
        "WebAPIGateway", "AdminAPIGateway", "MobileAPIGateway",
        
        // QuantumLedger Hub
        "QuantumLedger.Hub"
    };

    public ServiceRegistrationController(
        IServiceRegistrationService serviceRegistrationService,
        IKeyManager keyManager,
        ILogger<ServiceRegistrationController> logger)
    {
        _serviceRegistrationService = serviceRegistrationService;
        _keyManager = keyManager;
        _logger = logger;
    }

    /// <summary>
    /// Registers a service with quantum-resistant operational keys
    /// Generates Dilithium, Falcon, and EC256 key pairs for the service
    /// </summary>
    /// <param name="request">Service registration request</param>
    /// <returns>Service registration response with key information</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ServiceRegistrationResponse>> RegisterService([FromBody] ServiceRegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting service registration for {ServiceName}", request.ServiceName);

            // Validate service name
            if (!ValidServiceNames.Contains(request.ServiceName))
            {
                return BadRequest($"Invalid service name. Must be one of: {string.Join(", ", ValidServiceNames)}");
            }

            // Check if service is already registered
            var existingService = await _serviceRegistrationService.GetServiceByNameAsync(request.ServiceName);
            if (existingService != null)
            {
                return Conflict($"Service {request.ServiceName} is already registered");
            }

            // Generate service address (unique identifier)
            var serviceAddress = GenerateServiceAddress(request.ServiceName);

            // Generate quantum-resistant key pairs
            var keyGenerationTasks = new List<Task<string>>
            {
                // Dilithium (NIST Level 3 - balanced security/performance)
                _keyManager.GenerateKeyPairAsync(serviceAddress, "DILITHIUM", KeyCategory.PostQuantum, 1),
                
                // Falcon (NIST Level 1 - high performance)
                _keyManager.GenerateKeyPairAsync(serviceAddress, "FALCON", KeyCategory.PostQuantum, 1),
                
                // EC256 (Traditional - for compatibility)
                _keyManager.GenerateKeyPairAsync(serviceAddress, "EC256", KeyCategory.Traditional, 1)
            };

            var keyIds = await Task.WhenAll(keyGenerationTasks);

            // Create service registration record
            var serviceRegistration = new ServiceRegistration
            {
                ServiceName = request.ServiceName,
                ServiceAddress = serviceAddress,
                ServiceVersion = request.ServiceVersion,
                ServiceEndpoint = request.ServiceEndpoint,
                DilithiumKeyId = keyIds[0],
                FalconKeyId = keyIds[1],
                EC256KeyId = keyIds[2],
                RegistrationMetadata = request.Metadata ?? new Dictionary<string, string>(),
                RegisteredAt = DateTime.UtcNow,
                IsActive = true
            };

            // Save service registration
            var registeredService = await _serviceRegistrationService.RegisterServiceAsync(serviceRegistration);

            // Get public keys for response
            var publicKeys = new ServicePublicKeys
            {
                DilithiumPublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(keyIds[0])),
                FalconPublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(keyIds[1])),
                EC256PublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(keyIds[2]))
            };

            var response = new ServiceRegistrationResponse
            {
                ServiceId = registeredService.Id,
                ServiceName = registeredService.ServiceName,
                ServiceAddress = registeredService.ServiceAddress,
                PublicKeys = publicKeys,
                KeyIds = new ServiceKeyIds
                {
                    DilithiumKeyId = keyIds[0],
                    FalconKeyId = keyIds[1],
                    EC256KeyId = keyIds[2]
                },
                RegistrationStatus = "Active",
                RegisteredAt = registeredService.RegisteredAt,
                Message = "Service registered successfully with quantum-resistant operational keys"
            };

            _logger.LogInformation("Successfully registered service {ServiceName} with address {ServiceAddress}", 
                request.ServiceName, serviceAddress);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service {ServiceName}", request.ServiceName);
            return StatusCode(500, "An error occurred while registering the service");
        }
    }

    /// <summary>
    /// Gets registration information for a service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>Service registration information</returns>
    [HttpGet("services/{serviceName}")]
    public async Task<ActionResult<ServiceRegistrationResponse>> GetServiceRegistration(string serviceName)
    {
        try
        {
            var service = await _serviceRegistrationService.GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return NotFound($"Service {serviceName} is not registered");
            }

            // Get public keys
            var publicKeys = new ServicePublicKeys
            {
                DilithiumPublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(service.DilithiumKeyId)),
                FalconPublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(service.FalconKeyId)),
                EC256PublicKey = Convert.ToBase64String(await _keyManager.GetPublicKeyAsync(service.EC256KeyId))
            };

            var response = new ServiceRegistrationResponse
            {
                ServiceId = service.Id,
                ServiceName = service.ServiceName,
                ServiceAddress = service.ServiceAddress,
                PublicKeys = publicKeys,
                KeyIds = new ServiceKeyIds
                {
                    DilithiumKeyId = service.DilithiumKeyId,
                    FalconKeyId = service.FalconKeyId,
                    EC256KeyId = service.EC256KeyId
                },
                RegistrationStatus = service.IsActive ? "Active" : "Inactive",
                RegisteredAt = service.RegisteredAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service registration for {ServiceName}", serviceName);
            return StatusCode(500, "An error occurred while retrieving service registration");
        }
    }

    /// <summary>
    /// Lists all registered services
    /// </summary>
    /// <returns>List of registered services</returns>
    [HttpGet("services")]
    public async Task<ActionResult<List<ServiceSummaryResponse>>> GetAllServices()
    {
        try
        {
            var services = await _serviceRegistrationService.GetAllServicesAsync();
            
            var response = services.Select(s => new ServiceSummaryResponse
            {
                ServiceId = s.Id,
                ServiceName = s.ServiceName,
                ServiceAddress = s.ServiceAddress,
                ServiceVersion = s.ServiceVersion,
                ServiceEndpoint = s.ServiceEndpoint,
                RegistrationStatus = s.IsActive ? "Active" : "Inactive",
                RegisteredAt = s.RegisteredAt,
                LastHealthCheck = s.LastHealthCheck,
                HasQuantumKeys = !string.IsNullOrEmpty(s.DilithiumKeyId) && !string.IsNullOrEmpty(s.FalconKeyId),
                HasTraditionalKeys = !string.IsNullOrEmpty(s.EC256KeyId)
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all service registrations");
            return StatusCode(500, "An error occurred while retrieving service registrations");
        }
    }

    /// <summary>
    /// Rotates keys for a registered service
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="request">Key rotation request</param>
    /// <returns>New key information</returns>
    [HttpPost("services/{serviceName}/rotate-keys")]
    public async Task<ActionResult<KeyRotationResponse>> RotateServiceKeys(string serviceName, [FromBody] KeyRotationRequest request)
    {
        try
        {
            var service = await _serviceRegistrationService.GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return NotFound($"Service {serviceName} is not registered");
            }

            var rotatedKeys = new Dictionary<string, string>();

            // Rotate specified key types
            if (request.RotateDilithium)
            {
                var newKeyId = await _keyManager.RotateKeysAsync(service.ServiceAddress, KeyCategory.PostQuantum);
                rotatedKeys["Dilithium"] = newKeyId;
                service.DilithiumKeyId = newKeyId;
            }

            if (request.RotateFalcon)
            {
                var newKeyId = await _keyManager.RotateKeysAsync(service.ServiceAddress, KeyCategory.PostQuantum);
                rotatedKeys["Falcon"] = newKeyId;
                service.FalconKeyId = newKeyId;
            }

            if (request.RotateEC256)
            {
                var newKeyId = await _keyManager.RotateKeysAsync(service.ServiceAddress, KeyCategory.Traditional);
                rotatedKeys["EC256"] = newKeyId;
                service.EC256KeyId = newKeyId;
            }

            // Update service registration
            await _serviceRegistrationService.UpdateServiceAsync(service);

            var response = new KeyRotationResponse
            {
                ServiceName = serviceName,
                RotatedKeys = rotatedKeys,
                RotatedAt = DateTime.UtcNow,
                Message = $"Successfully rotated {rotatedKeys.Count} key(s) for service {serviceName}"
            };

            _logger.LogInformation("Rotated keys for service {ServiceName}: {RotatedKeys}", 
                serviceName, string.Join(", ", rotatedKeys.Keys));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating keys for service {ServiceName}", serviceName);
            return StatusCode(500, "An error occurred while rotating service keys");
        }
    }

    /// <summary>
    /// Updates service health status
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="request">Health status update request</param>
    /// <returns>Updated service information</returns>
    [HttpPost("services/{serviceName}/health")]
    public async Task<ActionResult> UpdateServiceHealth(string serviceName, [FromBody] ServiceHealthUpdateRequest request)
    {
        try
        {
            var service = await _serviceRegistrationService.GetServiceByNameAsync(serviceName);
            if (service == null)
            {
                return NotFound($"Service {serviceName} is not registered");
            }

            service.LastHealthCheck = DateTime.UtcNow;
            service.HealthStatus = request.Status;
            service.HealthMetadata = request.Metadata ?? new Dictionary<string, string>();

            await _serviceRegistrationService.UpdateServiceAsync(service);

            _logger.LogInformation("Updated health status for service {ServiceName}: {Status}", 
                serviceName, request.Status);

            return Ok(new { message = "Health status updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating health status for service {ServiceName}", serviceName);
            return StatusCode(500, "An error occurred while updating service health");
        }
    }

    /// <summary>
    /// Bulk registers all QuantumSkyLink v2 services
    /// </summary>
    /// <param name="request">Bulk registration request</param>
    /// <returns>Bulk registration results</returns>
    [HttpPost("bulk-register")]
    public async Task<ActionResult<BulkRegistrationResponse>> BulkRegisterServices([FromBody] BulkRegistrationRequest request)
    {
        try
        {
            _logger.LogInformation("Starting bulk registration of {ServiceCount} services", ValidServiceNames.Count);

            var results = new List<ServiceRegistrationResult>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var serviceName in ValidServiceNames)
            {
                try
                {
                    // Check if already registered
                    var existingService = await _serviceRegistrationService.GetServiceByNameAsync(serviceName);
                    if (existingService != null)
                    {
                        results.Add(new ServiceRegistrationResult
                        {
                            ServiceName = serviceName,
                            Success = false,
                            Message = "Service already registered",
                            ServiceAddress = existingService.ServiceAddress
                        });
                        continue;
                    }

                    // Generate service address
                    var serviceAddress = GenerateServiceAddress(serviceName);

                    // Generate keys
                    var keyGenerationTasks = new List<Task<string>>
                    {
                        _keyManager.GenerateKeyPairAsync(serviceAddress, "DILITHIUM", KeyCategory.PostQuantum, 1),
                        _keyManager.GenerateKeyPairAsync(serviceAddress, "FALCON", KeyCategory.PostQuantum, 1),
                        _keyManager.GenerateKeyPairAsync(serviceAddress, "EC256", KeyCategory.Traditional, 1)
                    };

                    var keyIds = await Task.WhenAll(keyGenerationTasks);

                    // Create service registration
                    var serviceRegistration = new ServiceRegistration
                    {
                        ServiceName = serviceName,
                        ServiceAddress = serviceAddress,
                        ServiceVersion = request.DefaultServiceVersion ?? "1.0.0",
                        ServiceEndpoint = $"https://localhost:7000/{serviceName.ToLower()}", // Default endpoint
                        DilithiumKeyId = keyIds[0],
                        FalconKeyId = keyIds[1],
                        EC256KeyId = keyIds[2],
                        RegistrationMetadata = new Dictionary<string, string>
                        {
                            ["BulkRegistration"] = "true",
                            ["RegistrationBatch"] = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")
                        },
                        RegisteredAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _serviceRegistrationService.RegisterServiceAsync(serviceRegistration);

                    results.Add(new ServiceRegistrationResult
                    {
                        ServiceName = serviceName,
                        Success = true,
                        Message = "Successfully registered with quantum-resistant keys",
                        ServiceAddress = serviceAddress,
                        KeyIds = new ServiceKeyIds
                        {
                            DilithiumKeyId = keyIds[0],
                            FalconKeyId = keyIds[1],
                            EC256KeyId = keyIds[2]
                        }
                    });

                    successCount++;
                    _logger.LogInformation("Successfully registered service {ServiceName}", serviceName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to register service {ServiceName}", serviceName);
                    results.Add(new ServiceRegistrationResult
                    {
                        ServiceName = serviceName,
                        Success = false,
                        Message = $"Registration failed: {ex.Message}"
                    });
                    failureCount++;
                }
            }

            var response = new BulkRegistrationResponse
            {
                TotalServices = ValidServiceNames.Count,
                SuccessfulRegistrations = successCount,
                FailedRegistrations = failureCount,
                Results = results,
                CompletedAt = DateTime.UtcNow,
                Message = $"Bulk registration completed: {successCount} successful, {failureCount} failed"
            };

            _logger.LogInformation("Bulk registration completed: {SuccessCount} successful, {FailureCount} failed", 
                successCount, failureCount);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk service registration");
            return StatusCode(500, "An error occurred during bulk service registration");
        }
    }

    /// <summary>
    /// Generates a unique service address based on service name
    /// </summary>
    /// <param name="serviceName">Name of the service</param>
    /// <returns>Unique service address</returns>
    private static string GenerateServiceAddress(string serviceName)
    {
        // Create deterministic but unique address based on service name and timestamp
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{serviceName}-{timestamp}")
        );
        
        // Take first 20 bytes and encode as hex (40 characters)
        var addressBytes = hash.Take(20).ToArray();
        return "0x" + Convert.ToHexString(addressBytes).ToLowerInvariant();
    }
}
