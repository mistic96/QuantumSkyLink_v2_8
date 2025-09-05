using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests;

/// <summary>
/// Request to generate a signature using RAGS (Robust Anti-replay Governance Signature) system
/// </summary>
public class GenerateSignatureRequest
{
    /// <summary>
    /// The service name to generate signature for
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The message to sign (will be hashed before signing)
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The cryptographic algorithm to use (DILITHIUM, FALCON, EC256)
    /// </summary>
    [Required]
    [RegularExpression("^(DILITHIUM|FALCON|EC256)$", ErrorMessage = "Algorithm must be DILITHIUM, FALCON, or EC256")]
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Optional nonce for replay protection (if not provided, system will generate one)
    /// </summary>
    public string? Nonce { get; set; }

    /// <summary>
    /// Optional blockchain address to use for signing (if not provided, will use service's primary address)
    /// </summary>
    public string? BlockchainAddress { get; set; }

    /// <summary>
    /// Optional metadata to include in signature context
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request to validate a signature using RAGS system
/// </summary>
public class ValidateSignatureRequest
{
    /// <summary>
    /// The service name that generated the signature
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The original message that was signed
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The signature to validate (base64 encoded)
    /// </summary>
    [Required]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// The cryptographic algorithm used (DILITHIUM, FALCON, EC256)
    /// </summary>
    [Required]
    [RegularExpression("^(DILITHIUM|FALCON|EC256)$", ErrorMessage = "Algorithm must be DILITHIUM, FALCON, or EC256")]
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// The nonce used for replay protection
    /// </summary>
    [Required]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// The blockchain address used for signing
    /// </summary>
    [Required]
    public string BlockchainAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional metadata that was included in signature context
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request to generate signatures for multiple services in bulk
/// </summary>
public class BulkGenerateSignatureRequest
{
    /// <summary>
    /// The message to sign for all services
    /// </summary>
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The cryptographic algorithm to use for all signatures
    /// </summary>
    [Required]
    [RegularExpression("^(DILITHIUM|FALCON|EC256)$", ErrorMessage = "Algorithm must be DILITHIUM, FALCON, or EC256")]
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of specific service names (if not provided, will sign for all registered services)
    /// </summary>
    public List<string>? ServiceNames { get; set; }

    /// <summary>
    /// Optional metadata to include in all signature contexts
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request to check nonce status for replay protection
/// </summary>
public class CheckNonceRequest
{
    /// <summary>
    /// The service name to check nonce for
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The nonce to check
    /// </summary>
    [Required]
    public string Nonce { get; set; } = string.Empty;
}

/// <summary>
/// Request to get signature validation metrics
/// </summary>
public class GetSignatureMetricsRequest
{
    /// <summary>
    /// Optional service name to filter metrics (if not provided, returns metrics for all services)
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Optional algorithm to filter metrics (if not provided, returns metrics for all algorithms)
    /// </summary>
    [RegularExpression("^(DILITHIUM|FALCON|EC256)$", ErrorMessage = "Algorithm must be DILITHIUM, FALCON, or EC256")]
    public string? Algorithm { get; set; }

    /// <summary>
    /// Optional start date for metrics filtering
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Optional end date for metrics filtering
    /// </summary>
    public DateTime? EndDate { get; set; }
}
