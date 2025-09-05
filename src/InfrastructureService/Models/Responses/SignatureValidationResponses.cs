namespace InfrastructureService.Models.Responses;

/// <summary>
/// Response from signature generation operation
/// </summary>
public class GenerateSignatureResponse
{
    /// <summary>
    /// The service name that generated the signature
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The generated signature (base64 encoded)
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// The cryptographic algorithm used
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// The nonce used for replay protection
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// The blockchain address used for signing
    /// </summary>
    public string BlockchainAddress { get; set; } = string.Empty;

    /// <summary>
    /// The public key used for verification (base64 encoded)
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// The message hash that was signed (base64 encoded)
    /// </summary>
    public string MessageHash { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the signature was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Time taken to generate the signature
    /// </summary>
    public TimeSpan GenerationTime { get; set; }

    /// <summary>
    /// Metadata included in signature context
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Indicates if the signature was generated successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if signature generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response from signature validation operation
/// </summary>
public class ValidateSignatureResponse
{
    /// <summary>
    /// The service name that generated the signature
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The cryptographic algorithm used
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// The nonce used for replay protection
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// The blockchain address used for signing
    /// </summary>
    public string BlockchainAddress { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the signature is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Indicates if the nonce has been used before (replay attack detection)
    /// </summary>
    public bool IsNonceReused { get; set; }

    /// <summary>
    /// Timestamp when the validation was performed
    /// </summary>
    public DateTime ValidatedAt { get; set; }

    /// <summary>
    /// Time taken to validate the signature
    /// </summary>
    public TimeSpan ValidationTime { get; set; }

    /// <summary>
    /// Detailed validation results
    /// </summary>
    public SignatureValidationDetails ValidationDetails { get; set; } = new();

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Detailed signature validation results
/// </summary>
public class SignatureValidationDetails
{
    /// <summary>
    /// Indicates if the signature format is valid
    /// </summary>
    public bool SignatureFormatValid { get; set; }

    /// <summary>
    /// Indicates if the public key is valid
    /// </summary>
    public bool PublicKeyValid { get; set; }

    /// <summary>
    /// Indicates if the message hash matches
    /// </summary>
    public bool MessageHashValid { get; set; }

    /// <summary>
    /// Indicates if the cryptographic signature is valid
    /// </summary>
    public bool CryptographicSignatureValid { get; set; }

    /// <summary>
    /// Indicates if the nonce is valid (not reused)
    /// </summary>
    public bool NonceValid { get; set; }

    /// <summary>
    /// Indicates if the blockchain address matches the service
    /// </summary>
    public bool BlockchainAddressValid { get; set; }

    /// <summary>
    /// Additional validation notes
    /// </summary>
    public List<string> ValidationNotes { get; set; } = new();
}

/// <summary>
/// Response from bulk signature generation operation
/// </summary>
public class BulkGenerateSignatureResponse
{
    /// <summary>
    /// The cryptographic algorithm used for all signatures
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// List of individual signature generation results
    /// </summary>
    public List<GenerateSignatureResponse> Signatures { get; set; } = new();

    /// <summary>
    /// Total number of signatures requested
    /// </summary>
    public int TotalRequested { get; set; }

    /// <summary>
    /// Number of signatures generated successfully
    /// </summary>
    public int SuccessfullyGenerated { get; set; }

    /// <summary>
    /// Number of signatures that failed to generate
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Total time taken for bulk generation
    /// </summary>
    public TimeSpan TotalGenerationTime { get; set; }

    /// <summary>
    /// Average time per signature
    /// </summary>
    public TimeSpan AverageGenerationTime { get; set; }

    /// <summary>
    /// Timestamp when bulk generation was completed
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// List of errors encountered during bulk generation
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Response from nonce check operation
/// </summary>
public class CheckNonceResponse
{
    /// <summary>
    /// The service name checked
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// The nonce that was checked
    /// </summary>
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if the nonce has been used before
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Timestamp when the nonce was first used (if applicable)
    /// </summary>
    public DateTime? FirstUsedAt { get; set; }

    /// <summary>
    /// Number of times this nonce has been used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Indicates if the nonce is valid for use
    /// </summary>
    public bool IsValidForUse { get; set; }

    /// <summary>
    /// Timestamp when the check was performed
    /// </summary>
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Response containing signature validation metrics
/// </summary>
public class SignatureMetricsResponse
{
    /// <summary>
    /// Service name filter applied (if any)
    /// </summary>
    public string? ServiceNameFilter { get; set; }

    /// <summary>
    /// Algorithm filter applied (if any)
    /// </summary>
    public string? AlgorithmFilter { get; set; }

    /// <summary>
    /// Date range for metrics
    /// </summary>
    public DateRange DateRange { get; set; } = new();

    /// <summary>
    /// Total number of signatures generated
    /// </summary>
    public int TotalSignaturesGenerated { get; set; }

    /// <summary>
    /// Total number of signatures validated
    /// </summary>
    public int TotalSignaturesValidated { get; set; }

    /// <summary>
    /// Number of valid signatures
    /// </summary>
    public int ValidSignatures { get; set; }

    /// <summary>
    /// Number of invalid signatures
    /// </summary>
    public int InvalidSignatures { get; set; }

    /// <summary>
    /// Number of replay attacks detected
    /// </summary>
    public int ReplayAttacksDetected { get; set; }

    /// <summary>
    /// Average signature generation time
    /// </summary>
    public TimeSpan AverageGenerationTime { get; set; }

    /// <summary>
    /// Average signature validation time
    /// </summary>
    public TimeSpan AverageValidationTime { get; set; }

    /// <summary>
    /// Metrics broken down by algorithm
    /// </summary>
    public List<AlgorithmMetrics> AlgorithmMetrics { get; set; } = new();

    /// <summary>
    /// Metrics broken down by service
    /// </summary>
    public List<ServiceMetrics> ServiceMetrics { get; set; } = new();

    /// <summary>
    /// Timestamp when metrics were generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Date range for metrics filtering
/// </summary>
public class DateRange
{
    /// <summary>
    /// Start date of the range
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the range
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Metrics for a specific algorithm
/// </summary>
public class AlgorithmMetrics
{
    /// <summary>
    /// The algorithm name
    /// </summary>
    public string Algorithm { get; set; } = string.Empty;

    /// <summary>
    /// Number of signatures generated with this algorithm
    /// </summary>
    public int SignaturesGenerated { get; set; }

    /// <summary>
    /// Number of signatures validated with this algorithm
    /// </summary>
    public int SignaturesValidated { get; set; }

    /// <summary>
    /// Number of valid signatures for this algorithm
    /// </summary>
    public int ValidSignatures { get; set; }

    /// <summary>
    /// Average generation time for this algorithm
    /// </summary>
    public TimeSpan AverageGenerationTime { get; set; }

    /// <summary>
    /// Average validation time for this algorithm
    /// </summary>
    public TimeSpan AverageValidationTime { get; set; }
}

/// <summary>
/// Metrics for a specific service
/// </summary>
public class ServiceMetrics
{
    /// <summary>
    /// The service name
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Number of signatures generated by this service
    /// </summary>
    public int SignaturesGenerated { get; set; }

    /// <summary>
    /// Number of signatures validated for this service
    /// </summary>
    public int SignaturesValidated { get; set; }

    /// <summary>
    /// Number of valid signatures for this service
    /// </summary>
    public int ValidSignatures { get; set; }

    /// <summary>
    /// Number of replay attacks detected for this service
    /// </summary>
    public int ReplayAttacksDetected { get; set; }
}
