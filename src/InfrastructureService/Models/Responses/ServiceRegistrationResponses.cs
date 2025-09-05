namespace InfrastructureService.Models.Responses;

/// <summary>
/// Response model for service registration
/// </summary>
public class ServiceRegistrationResponse
{
    /// <summary>
    /// Unique identifier for the registered service
    /// </summary>
    public required string ServiceId { get; set; }

    /// <summary>
    /// Name of the registered service
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Unique service address (blockchain-style address)
    /// </summary>
    public required string ServiceAddress { get; set; }

    /// <summary>
    /// Public keys for the service
    /// </summary>
    public required ServicePublicKeys PublicKeys { get; set; }

    /// <summary>
    /// Key identifiers for the service
    /// </summary>
    public required ServiceKeyIds KeyIds { get; set; }

    /// <summary>
    /// Registration status
    /// </summary>
    public required string RegistrationStatus { get; set; }

    /// <summary>
    /// When the service was registered
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Registration message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Public keys for a service
/// </summary>
public class ServicePublicKeys
{
    /// <summary>
    /// Dilithium public key (Base64 encoded)
    /// </summary>
    public required string DilithiumPublicKey { get; set; }

    /// <summary>
    /// Falcon public key (Base64 encoded)
    /// </summary>
    public required string FalconPublicKey { get; set; }

    /// <summary>
    /// EC256 public key (Base64 encoded)
    /// </summary>
    public required string EC256PublicKey { get; set; }
}

/// <summary>
/// Key identifiers for a service
/// </summary>
public class ServiceKeyIds
{
    /// <summary>
    /// Dilithium key identifier
    /// </summary>
    public required string DilithiumKeyId { get; set; }

    /// <summary>
    /// Falcon key identifier
    /// </summary>
    public required string FalconKeyId { get; set; }

    /// <summary>
    /// EC256 key identifier
    /// </summary>
    public required string EC256KeyId { get; set; }
}

/// <summary>
/// Summary response for service listing
/// </summary>
public class ServiceSummaryResponse
{
    /// <summary>
    /// Unique identifier for the service
    /// </summary>
    public required string ServiceId { get; set; }

    /// <summary>
    /// Name of the service
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Service address
    /// </summary>
    public required string ServiceAddress { get; set; }

    /// <summary>
    /// Service version
    /// </summary>
    public required string ServiceVersion { get; set; }

    /// <summary>
    /// Service endpoint
    /// </summary>
    public required string ServiceEndpoint { get; set; }

    /// <summary>
    /// Registration status
    /// </summary>
    public required string RegistrationStatus { get; set; }

    /// <summary>
    /// When the service was registered
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime? LastHealthCheck { get; set; }

    /// <summary>
    /// Whether the service has quantum-resistant keys
    /// </summary>
    public bool HasQuantumKeys { get; set; }

    /// <summary>
    /// Whether the service has traditional keys
    /// </summary>
    public bool HasTraditionalKeys { get; set; }
}

/// <summary>
/// Response model for key rotation
/// </summary>
public class KeyRotationResponse
{
    /// <summary>
    /// Name of the service
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Dictionary of rotated keys (algorithm -> new key ID)
    /// </summary>
    public required Dictionary<string, string> RotatedKeys { get; set; }

    /// <summary>
    /// When the rotation was completed
    /// </summary>
    public DateTime RotatedAt { get; set; }

    /// <summary>
    /// Rotation message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Response model for bulk registration
/// </summary>
public class BulkRegistrationResponse
{
    /// <summary>
    /// Total number of services processed
    /// </summary>
    public int TotalServices { get; set; }

    /// <summary>
    /// Number of successful registrations
    /// </summary>
    public int SuccessfulRegistrations { get; set; }

    /// <summary>
    /// Number of failed registrations
    /// </summary>
    public int FailedRegistrations { get; set; }

    /// <summary>
    /// Detailed results for each service
    /// </summary>
    public required List<ServiceRegistrationResult> Results { get; set; }

    /// <summary>
    /// When the bulk operation completed
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Overall operation message
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Result for individual service registration in bulk operation
/// </summary>
public class ServiceRegistrationResult
{
    /// <summary>
    /// Name of the service
    /// </summary>
    public required string ServiceName { get; set; }

    /// <summary>
    /// Whether the registration was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Service address (if successful)
    /// </summary>
    public string? ServiceAddress { get; set; }

    /// <summary>
    /// Key identifiers (if successful)
    /// </summary>
    public ServiceKeyIds? KeyIds { get; set; }
}
