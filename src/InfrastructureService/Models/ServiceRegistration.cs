using System.ComponentModel.DataAnnotations;
using LiquidStorageCloud.Core.Database;

namespace InfrastructureService.Models;

/// <summary>
/// Entity representing a registered service with quantum-resistant operational keys
/// </summary>
public class ServiceRegistration : ISurrealEntity
{
    /// <summary>
    /// Unique identifier for the service registration
    /// </summary>
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// SurrealDB solid state flag
    /// </summary>
    public bool SolidState { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// SurrealDB table name
    /// </summary>
    public string TableName => "service_registrations";

    /// <summary>
    /// SurrealDB namespace
    /// </summary>
    public string Namespace => "infrastructure";

    /// <summary>
    /// Name of the registered service
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string ServiceName { get; set; }

    /// <summary>
    /// Unique service address (blockchain-style address)
    /// </summary>
    [Required]
    [StringLength(42)] // 0x + 40 hex characters
    public required string ServiceAddress { get; set; }

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
    [StringLength(500)]
    public required string ServiceEndpoint { get; set; }

    /// <summary>
    /// Dilithium key identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string DilithiumKeyId { get; set; }

    /// <summary>
    /// Falcon key identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string FalconKeyId { get; set; }

    /// <summary>
    /// EC256 key identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string EC256KeyId { get; set; }

    /// <summary>
    /// When the service was registered
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the service is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime? LastHealthCheck { get; set; }

    /// <summary>
    /// Current health status
    /// </summary>
    [StringLength(50)]
    public string? HealthStatus { get; set; }

    /// <summary>
    /// Health metadata
    /// </summary>
    public Dictionary<string, string> HealthMetadata { get; set; } = new();

    /// <summary>
    /// Registration metadata
    /// </summary>
    public Dictionary<string, string> RegistrationMetadata { get; set; } = new();

    /// <summary>
    /// When the service was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of key rotations performed
    /// </summary>
    public int KeyRotationCount { get; set; } = 0;

    /// <summary>
    /// Last key rotation timestamp
    /// </summary>
    public DateTime? LastKeyRotation { get; set; }

    /// <summary>
    /// Service capabilities/features
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    /// <summary>
    /// Service dependencies
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Service tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether the service supports quantum-resistant operations
    /// </summary>
    public bool SupportsQuantumResistant => !string.IsNullOrEmpty(DilithiumKeyId) && !string.IsNullOrEmpty(FalconKeyId);

    /// <summary>
    /// Whether the service supports traditional cryptography
    /// </summary>
    public bool SupportsTraditional => !string.IsNullOrEmpty(EC256KeyId);

    /// <summary>
    /// Gets the service type based on name
    /// </summary>
    public string ServiceType
    {
        get
        {
            return ServiceName switch
            {
                "UserService" or "AccountService" or "ComplianceService" or "FeeService" or 
                "SecurityService" or "TokenService" or "GovernanceService" => "Core",
                
                "TreasuryService" or "PaymentGatewayService" or "LiquidationService" => "Financial",
                
                "InfrastructureService" or "IdentityVerificationService" => "Infrastructure",
                
                "AIReviewService" or "NotificationService" => "Supporting",
                
                "SignatureService" => "Security",
                
                "OrchestrationService" => "Orchestration",
                
                "MarketplaceService" => "Marketplace",
                
                "WebAPIGateway" or "AdminAPIGateway" or "MobileAPIGateway" => "Gateway",
                
                "QuantumLedger.Hub" => "Ledger",
                
                _ => "Unknown"
            };
        }
    }

    /// <summary>
    /// Updates the last modified timestamp
    /// </summary>
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
        LastModified = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks a key rotation as completed
    /// </summary>
    public void MarkKeyRotation()
    {
        KeyRotationCount++;
        LastKeyRotation = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Updates health status
    /// </summary>
    /// <param name="status">New health status</param>
    /// <param name="metadata">Optional health metadata</param>
    public void UpdateHealth(string status, Dictionary<string, string>? metadata = null)
    {
        HealthStatus = status;
        LastHealthCheck = DateTime.UtcNow;
        
        if (metadata != null)
        {
            HealthMetadata = metadata;
        }
        
        Touch();
    }
}
