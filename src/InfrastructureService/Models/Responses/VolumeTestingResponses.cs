namespace InfrastructureService.Models.Responses;

/// <summary>
/// Response for starting a volume test
/// </summary>
public class StartVolumeTestResponse
{
    /// <summary>
    /// Test identifier
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the test was started successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if test failed to start
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Test configuration details
    /// </summary>
    public VolumeTestConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// Test start time
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response for volume test status
/// </summary>
public class VolumeTestStatusResponse
{
    /// <summary>
    /// Test identifier
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Current test status
    /// </summary>
    public string Status { get; set; } = string.Empty; // Running, Completed, Failed, Stopped

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public double ProgressPercentage { get; set; }

    /// <summary>
    /// Current operation being performed
    /// </summary>
    public string CurrentOperation { get; set; } = string.Empty;

    /// <summary>
    /// Number of operations completed
    /// </summary>
    public long OperationsCompleted { get; set; }

    /// <summary>
    /// Total number of operations planned
    /// </summary>
    public long TotalOperations { get; set; }

    /// <summary>
    /// Current operations per second
    /// </summary>
    public double CurrentOperationsPerSecond { get; set; }

    /// <summary>
    /// Average operations per second
    /// </summary>
    public double AverageOperationsPerSecond { get; set; }

    /// <summary>
    /// Test start time
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; set; }

    /// <summary>
    /// Elapsed time
    /// </summary>
    public TimeSpan ElapsedTime { get; set; }

    /// <summary>
    /// Number of errors encountered
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Recent error messages
    /// </summary>
    public List<string> RecentErrors { get; set; } = new();

    /// <summary>
    /// Real-time performance metrics
    /// </summary>
    public VolumeTestPerformanceMetrics PerformanceMetrics { get; set; } = new();

    /// <summary>
    /// Resource utilization information
    /// </summary>
    public ResourceUtilization ResourceUtilization { get; set; } = new();
}

/// <summary>
/// Response for volume test results
/// </summary>
public class VolumeTestResultsResponse
{
    /// <summary>
    /// Test identifier
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// Test configuration
    /// </summary>
    public VolumeTestConfiguration Configuration { get; set; } = new();

    /// <summary>
    /// Overall test results
    /// </summary>
    public VolumeTestSummary Summary { get; set; } = new();

    /// <summary>
    /// Detailed performance metrics
    /// </summary>
    public VolumeTestPerformanceMetrics PerformanceMetrics { get; set; } = new();

    /// <summary>
    /// Address generation results (if applicable)
    /// </summary>
    public AddressGenerationResults? AddressResults { get; set; }

    /// <summary>
    /// Signature operation results (if applicable)
    /// </summary>
    public SignatureOperationResults? SignatureResults { get; set; }

    /// <summary>
    /// Error analysis
    /// </summary>
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();

    /// <summary>
    /// Resource utilization statistics
    /// </summary>
    public ResourceUtilization ResourceUtilization { get; set; } = new();

    /// <summary>
    /// Sample data (if requested)
    /// </summary>
    public List<object> Samples { get; set; } = new();

    /// <summary>
    /// Test recommendations based on results
    /// </summary>
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Volume test configuration details
/// </summary>
public class VolumeTestConfiguration
{
    /// <summary>
    /// Type of volume test
    /// </summary>
    public string TestType { get; set; } = string.Empty; // Address, Signature, Stress

    /// <summary>
    /// Target operation count
    /// </summary>
    public long TargetOperations { get; set; }

    /// <summary>
    /// Batch size
    /// </summary>
    public int BatchSize { get; set; }

    /// <summary>
    /// Number of parallel workers
    /// </summary>
    public int ParallelWorkers { get; set; }

    /// <summary>
    /// Network type (for address tests)
    /// </summary>
    public string? NetworkType { get; set; }

    /// <summary>
    /// Algorithm (for signature tests)
    /// </summary>
    public string? Algorithm { get; set; }

    /// <summary>
    /// Additional configuration parameters
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Volume test summary
/// </summary>
public class VolumeTestSummary
{
    /// <summary>
    /// Test status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total operations completed
    /// </summary>
    public long TotalOperationsCompleted { get; set; }

    /// <summary>
    /// Total operations planned
    /// </summary>
    public long TotalOperationsPlanned { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate { get; set; }

    /// <summary>
    /// Total test duration
    /// </summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// Average operations per second
    /// </summary>
    public double AverageOperationsPerSecond { get; set; }

    /// <summary>
    /// Peak operations per second
    /// </summary>
    public double PeakOperationsPerSecond { get; set; }

    /// <summary>
    /// Total errors encountered
    /// </summary>
    public int TotalErrors { get; set; }

    /// <summary>
    /// Test start time
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Test completion time
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Performance metrics for volume tests
/// </summary>
public class VolumeTestPerformanceMetrics
{
    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Minimum response time in milliseconds
    /// </summary>
    public double MinResponseTime { get; set; }

    /// <summary>
    /// Maximum response time in milliseconds
    /// </summary>
    public double MaxResponseTime { get; set; }

    /// <summary>
    /// 95th percentile response time
    /// </summary>
    public double P95ResponseTime { get; set; }

    /// <summary>
    /// 99th percentile response time
    /// </summary>
    public double P99ResponseTime { get; set; }

    /// <summary>
    /// Standard deviation of response times
    /// </summary>
    public double ResponseTimeStdDev { get; set; }

    /// <summary>
    /// Throughput in operations per second
    /// </summary>
    public double Throughput { get; set; }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Memory usage statistics
    /// </summary>
    public MemoryUsageStats MemoryUsage { get; set; } = new();

    /// <summary>
    /// CPU usage statistics
    /// </summary>
    public CpuUsageStats CpuUsage { get; set; } = new();

    /// <summary>
    /// Network I/O statistics
    /// </summary>
    public NetworkIOStats NetworkIO { get; set; } = new();
}

/// <summary>
/// Address generation test results
/// </summary>
public class AddressGenerationResults
{
    /// <summary>
    /// Total addresses generated
    /// </summary>
    public long TotalGenerated { get; set; }

    /// <summary>
    /// Successful generations
    /// </summary>
    public long SuccessfulGenerations { get; set; }

    /// <summary>
    /// Failed generations
    /// </summary>
    public long FailedGenerations { get; set; }

    /// <summary>
    /// Average generation time per address
    /// </summary>
    public TimeSpan AverageGenerationTime { get; set; }

    /// <summary>
    /// Addresses generated per second
    /// </summary>
    public double AddressesPerSecond { get; set; }

    /// <summary>
    /// Network-specific results
    /// </summary>
    public Dictionary<string, NetworkGenerationStats> NetworkStats { get; set; } = new();

    /// <summary>
    /// Validation results for generated addresses
    /// </summary>
    public AddressValidationResults ValidationResults { get; set; } = new();
}

/// <summary>
/// Signature operation test results
/// </summary>
public class SignatureOperationResults
{
    /// <summary>
    /// Total signatures generated
    /// </summary>
    public long TotalGenerated { get; set; }

    /// <summary>
    /// Total signatures validated
    /// </summary>
    public long TotalValidated { get; set; }

    /// <summary>
    /// Successful signature generations
    /// </summary>
    public long SuccessfulGenerations { get; set; }

    /// <summary>
    /// Successful signature validations
    /// </summary>
    public long SuccessfulValidations { get; set; }

    /// <summary>
    /// Average signature generation time
    /// </summary>
    public TimeSpan AverageGenerationTime { get; set; }

    /// <summary>
    /// Average signature validation time
    /// </summary>
    public TimeSpan AverageValidationTime { get; set; }

    /// <summary>
    /// Signatures generated per second
    /// </summary>
    public double SignaturesPerSecond { get; set; }

    /// <summary>
    /// Validations per second
    /// </summary>
    public double ValidationsPerSecond { get; set; }

    /// <summary>
    /// Algorithm-specific results
    /// </summary>
    public Dictionary<string, AlgorithmPerformanceStats> AlgorithmStats { get; set; } = new();

    /// <summary>
    /// Replay protection test results
    /// </summary>
    public ReplayProtectionResults ReplayProtection { get; set; } = new();
}

/// <summary>
/// Error analysis results
/// </summary>
public class ErrorAnalysis
{
    /// <summary>
    /// Total error count
    /// </summary>
    public int TotalErrors { get; set; }

    /// <summary>
    /// Error rate percentage
    /// </summary>
    public double ErrorRate { get; set; }

    /// <summary>
    /// Errors by category
    /// </summary>
    public Dictionary<string, int> ErrorsByCategory { get; set; } = new();

    /// <summary>
    /// Most common errors
    /// </summary>
    public List<ErrorFrequency> MostCommonErrors { get; set; } = new();

    /// <summary>
    /// Error timeline
    /// </summary>
    public List<ErrorTimelineEntry> ErrorTimeline { get; set; } = new();

    /// <summary>
    /// Critical errors that stopped processing
    /// </summary>
    public List<string> CriticalErrors { get; set; } = new();
}

/// <summary>
/// Resource utilization statistics
/// </summary>
public class ResourceUtilization
{
    /// <summary>
    /// Memory usage statistics
    /// </summary>
    public MemoryUsageStats Memory { get; set; } = new();

    /// <summary>
    /// CPU usage statistics
    /// </summary>
    public CpuUsageStats Cpu { get; set; } = new();

    /// <summary>
    /// Network I/O statistics
    /// </summary>
    public NetworkIOStats NetworkIO { get; set; } = new();

    /// <summary>
    /// Disk I/O statistics
    /// </summary>
    public DiskIOStats DiskIO { get; set; } = new();

    /// <summary>
    /// Thread pool statistics
    /// </summary>
    public ThreadPoolStats ThreadPool { get; set; } = new();
}

// Supporting classes for detailed metrics
public class MemoryUsageStats
{
    public long InitialMemoryMB { get; set; }
    public long PeakMemoryMB { get; set; }
    public long FinalMemoryMB { get; set; }
    public long AverageMemoryMB { get; set; }
    public int GCCollections { get; set; }
}

public class CpuUsageStats
{
    public double AverageCpuPercent { get; set; }
    public double PeakCpuPercent { get; set; }
    public double MinCpuPercent { get; set; }
    public TimeSpan TotalProcessorTime { get; set; }
}

public class NetworkIOStats
{
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public long RequestsSent { get; set; }
    public long ResponsesReceived { get; set; }
}

public class DiskIOStats
{
    public long BytesRead { get; set; }
    public long BytesWritten { get; set; }
    public long ReadOperations { get; set; }
    public long WriteOperations { get; set; }
}

public class ThreadPoolStats
{
    public int MaxWorkerThreads { get; set; }
    public int MaxCompletionPortThreads { get; set; }
    public int AvailableWorkerThreads { get; set; }
    public int AvailableCompletionPortThreads { get; set; }
}

public class NetworkGenerationStats
{
    public string NetworkType { get; set; } = string.Empty;
    public long AddressesGenerated { get; set; }
    public TimeSpan AverageTime { get; set; }
    public double SuccessRate { get; set; }
}

public class AddressValidationResults
{
    public long ValidAddresses { get; set; }
    public long InvalidAddresses { get; set; }
    public double ValidationSuccessRate { get; set; }
    public TimeSpan AverageValidationTime { get; set; }
}

public class AlgorithmPerformanceStats
{
    public string Algorithm { get; set; } = string.Empty;
    public long OperationsCompleted { get; set; }
    public TimeSpan AverageTime { get; set; }
    public double SuccessRate { get; set; }
    public long KeySize { get; set; }
    public long SignatureSize { get; set; }
}

public class ReplayProtectionResults
{
    public long NoncesTested { get; set; }
    public long ReplayAttemptsDetected { get; set; }
    public double ReplayDetectionRate { get; set; }
    public TimeSpan AverageNonceCheckTime { get; set; }
}

public class ErrorFrequency
{
    public string ErrorMessage { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class ErrorTimelineEntry
{
    public DateTime Timestamp { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int Count { get; set; }
}
