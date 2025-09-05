using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests;

/// <summary>
/// Request to start a volume test for address generation
/// </summary>
public class StartVolumeAddressTestRequest
{
    /// <summary>
    /// Number of addresses to generate
    /// </summary>
    [Required]
    [Range(1, 1000000, ErrorMessage = "Address count must be between 1 and 1,000,000")]
    public int AddressCount { get; set; } = 100000;

    /// <summary>
    /// Batch size for processing
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Batch size must be between 1 and 10,000")]
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Network type for address generation
    /// </summary>
    [Required]
    public string NetworkType { get; set; } = "MULTICHAIN";

    /// <summary>
    /// Number of parallel workers
    /// </summary>
    [Range(1, 50, ErrorMessage = "Worker count must be between 1 and 50")]
    public int ParallelWorkers { get; set; } = 5;

    /// <summary>
    /// Whether to include signature generation testing
    /// </summary>
    public bool IncludeSignatureTesting { get; set; } = true;

    /// <summary>
    /// Signature algorithm to test (if signature testing enabled)
    /// </summary>
    public string SignatureAlgorithm { get; set; } = "DILITHIUM";

    /// <summary>
    /// Whether to save generated addresses to storage
    /// </summary>
    public bool SaveAddresses { get; set; } = false;

    /// <summary>
    /// Test identifier for tracking
    /// </summary>
    public string? TestId { get; set; }

    /// <summary>
    /// Additional metadata for the test
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Request to start a volume test for signature operations
/// </summary>
public class StartVolumeSignatureTestRequest
{
    /// <summary>
    /// Number of signatures to generate and validate
    /// </summary>
    [Required]
    [Range(1, 1000000, ErrorMessage = "Signature count must be between 1 and 1,000,000")]
    public int SignatureCount { get; set; } = 50000;

    /// <summary>
    /// Batch size for processing
    /// </summary>
    [Range(1, 5000, ErrorMessage = "Batch size must be between 1 and 5,000")]
    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// Signature algorithm to test
    /// </summary>
    [Required]
    public string Algorithm { get; set; } = "DILITHIUM";

    /// <summary>
    /// Number of parallel workers
    /// </summary>
    [Range(1, 20, ErrorMessage = "Worker count must be between 1 and 20")]
    public int ParallelWorkers { get; set; } = 3;

    /// <summary>
    /// Whether to include validation testing
    /// </summary>
    public bool IncludeValidation { get; set; } = true;

    /// <summary>
    /// Whether to test replay protection
    /// </summary>
    public bool TestReplayProtection { get; set; } = true;

    /// <summary>
    /// Service names to use for testing (if empty, uses all registered services)
    /// </summary>
    public List<string> ServiceNames { get; set; } = new();

    /// <summary>
    /// Test identifier for tracking
    /// </summary>
    public string? TestId { get; set; }

    /// <summary>
    /// Additional metadata for the test
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Request to get volume test status
/// </summary>
public class GetVolumeTestStatusRequest
{
    /// <summary>
    /// Test identifier
    /// </summary>
    [Required]
    public string TestId { get; set; } = string.Empty;
}

/// <summary>
/// Request to stop a running volume test
/// </summary>
public class StopVolumeTestRequest
{
    /// <summary>
    /// Test identifier
    /// </summary>
    [Required]
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Reason for stopping the test
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request to get volume test results
/// </summary>
public class GetVolumeTestResultsRequest
{
    /// <summary>
    /// Test identifier
    /// </summary>
    [Required]
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to include detailed metrics
    /// </summary>
    public bool IncludeDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Whether to include generated data samples
    /// </summary>
    public bool IncludeSamples { get; set; } = false;

    /// <summary>
    /// Number of samples to include (if IncludeSamples is true)
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Sample count must be between 1 and 1,000")]
    public int SampleCount { get; set; } = 100;
}

/// <summary>
/// Request to start a comprehensive stress test
/// </summary>
public class StartStressTestRequest
{
    /// <summary>
    /// Duration of the stress test in minutes
    /// </summary>
    [Required]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1,440 minutes (24 hours)")]
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Target operations per second
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Target OPS must be between 1 and 10,000")]
    public int TargetOperationsPerSecond { get; set; } = 100;

    /// <summary>
    /// Types of operations to include in stress test
    /// </summary>
    public List<string> OperationTypes { get; set; } = new() { "AddressGeneration", "SignatureGeneration", "SignatureValidation" };

    /// <summary>
    /// Whether to gradually increase load
    /// </summary>
    public bool GradualLoadIncrease { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent operations
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max concurrent operations must be between 1 and 1,000")]
    public int MaxConcurrentOperations { get; set; } = 50;

    /// <summary>
    /// Test identifier for tracking
    /// </summary>
    public string? TestId { get; set; }

    /// <summary>
    /// Additional metadata for the test
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
