using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests;

/// <summary>
/// Request model for service registration
/// </summary>
public class ServiceRegistrationRequest
{
    /// <summary>
    /// Name of the service to register
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string ServiceName { get; set; }

    /// <summary>
    /// Version of the service
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string ServiceVersion { get; set; }

    /// <summary>
    /// Service endpoint URL
    /// </summary>
    [Required]
    [Url]
    public required string ServiceEndpoint { get; set; }

    /// <summary>
    /// Additional metadata for the service
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for key rotation
/// </summary>
public class KeyRotationRequest
{
    /// <summary>
    /// Whether to rotate Dilithium keys
    /// </summary>
    public bool RotateDilithium { get; set; } = false;

    /// <summary>
    /// Whether to rotate Falcon keys
    /// </summary>
    public bool RotateFalcon { get; set; } = false;

    /// <summary>
    /// Whether to rotate EC256 keys
    /// </summary>
    public bool RotateEC256 { get; set; } = false;

    /// <summary>
    /// Reason for key rotation
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for service health updates
/// </summary>
public class ServiceHealthUpdateRequest
{
    /// <summary>
    /// Health status of the service
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Status { get; set; }

    /// <summary>
    /// Additional health metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for bulk service registration
/// </summary>
public class BulkRegistrationRequest
{
    /// <summary>
    /// Default service version for all services
    /// </summary>
    [StringLength(20)]
    public string? DefaultServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Whether to force re-registration of existing services
    /// </summary>
    public bool ForceReregistration { get; set; } = false;

    /// <summary>
    /// Additional metadata to apply to all services
    /// </summary>
    public Dictionary<string, string>? GlobalMetadata { get; set; }
}
