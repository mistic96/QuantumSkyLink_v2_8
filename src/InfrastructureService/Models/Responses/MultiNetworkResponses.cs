namespace InfrastructureService.Models.Responses
{
    /// <summary>
    /// Response for multi-network address generation.
    /// </summary>
    public class MultiNetworkGenerateResponse
    {
        /// <summary>
        /// Gets or sets the service name for which addresses were generated.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated addresses for each network.
        /// </summary>
        public List<GenerateAddressResponse> NetworkAddresses { get; set; } = new();

        /// <summary>
        /// Gets or sets the cross-network metadata relationships.
        /// </summary>
        public CrossNetworkMetadata? CrossNetworkMetadata { get; set; }

        /// <summary>
        /// Gets or sets the generation summary statistics.
        /// </summary>
        public MultiNetworkGenerationSummary GenerationSummary { get; set; } = new();

        /// <summary>
        /// Gets or sets when the addresses were generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the total generation time across all networks.
        /// </summary>
        public TimeSpan TotalGenerationTime { get; set; }
    }

    /// <summary>
    /// Response for multi-network address validation.
    /// </summary>
    public class MultiNetworkValidateResponse
    {
        /// <summary>
        /// Gets or sets the validation results for each address.
        /// </summary>
        public List<ValidateAddressResponse> ValidationResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the cross-network validation results.
        /// </summary>
        public CrossNetworkValidationResult? CrossNetworkValidation { get; set; }

        /// <summary>
        /// Gets or sets the overall validation summary.
        /// </summary>
        public ValidationSummary ValidationSummary { get; set; } = new();

        /// <summary>
        /// Gets or sets when the validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response for network performance comparison.
    /// </summary>
    public class NetworkPerformanceComparisonResponse
    {
        /// <summary>
        /// Gets or sets the performance metrics for each network.
        /// </summary>
        public List<NetworkPerformanceMetrics> NetworkMetrics { get; set; } = new();

        /// <summary>
        /// Gets or sets the comparative analysis results.
        /// </summary>
        public PerformanceComparison Comparison { get; set; } = new();

        /// <summary>
        /// Gets or sets the time period analyzed.
        /// </summary>
        public TimeSpan AnalysisPeriod { get; set; }

        /// <summary>
        /// Gets or sets when the analysis was performed.
        /// </summary>
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets recommendations based on the analysis.
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// Response for cross-network metadata queries.
    /// </summary>
    public class CrossNetworkMetadataResponse
    {
        /// <summary>
        /// Gets or sets the primary address information.
        /// </summary>
        public AddressMetadata PrimaryAddress { get; set; } = new();

        /// <summary>
        /// Gets or sets related addresses from other networks.
        /// </summary>
        public List<AddressMetadata> RelatedAddresses { get; set; } = new();

        /// <summary>
        /// Gets or sets the cross-network relationships.
        /// </summary>
        public List<CrossNetworkRelationship> Relationships { get; set; } = new();

        /// <summary>
        /// Gets or sets historical metadata if requested.
        /// </summary>
        public List<HistoricalMetadata>? HistoricalData { get; set; }

        /// <summary>
        /// Gets or sets when the metadata was retrieved.
        /// </summary>
        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response for multi-network testing.
    /// </summary>
    public class MultiNetworkTestResponse
    {
        /// <summary>
        /// Gets or sets the test results for each network.
        /// </summary>
        public List<NetworkTestResult> NetworkResults { get; set; } = new();

        /// <summary>
        /// Gets or sets the overall test summary.
        /// </summary>
        public MultiNetworkTestSummary TestSummary { get; set; } = new();

        /// <summary>
        /// Gets or sets the test execution details.
        /// </summary>
        public TestExecutionDetails ExecutionDetails { get; set; } = new();

        /// <summary>
        /// Gets or sets when the test was completed.
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Response for network configuration operations.
    /// </summary>
    public class NetworkConfigurationResponse
    {
        /// <summary>
        /// Gets or sets whether the configuration was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the network type that was configured.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the applied configuration.
        /// </summary>
        public NetworkConfiguration AppliedConfiguration { get; set; } = new();

        /// <summary>
        /// Gets or sets any configuration warnings or messages.
        /// </summary>
        public List<string> Messages { get; set; } = new();

        /// <summary>
        /// Gets or sets when the configuration was applied.
        /// </summary>
        public DateTime ConfiguredAt { get; set; } = DateTime.UtcNow;
    }

    #region Supporting Classes

    /// <summary>
    /// Cross-network metadata for address relationships.
    /// </summary>
    public class CrossNetworkMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier for this cross-network group.
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service name associated with these addresses.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relationships between addresses.
        /// </summary>
        public List<AddressRelationship> AddressRelationships { get; set; } = new();

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Summary of multi-network address generation.
    /// </summary>
    public class MultiNetworkGenerationSummary
    {
        /// <summary>
        /// Gets or sets the total number of networks processed.
        /// </summary>
        public int TotalNetworks { get; set; }

        /// <summary>
        /// Gets or sets the number of successful generations.
        /// </summary>
        public int SuccessfulGenerations { get; set; }

        /// <summary>
        /// Gets or sets the number of failed generations.
        /// </summary>
        public int FailedGenerations { get; set; }

        /// <summary>
        /// Gets or sets the average generation time per network.
        /// </summary>
        public TimeSpan AverageGenerationTime { get; set; }

        /// <summary>
        /// Gets or sets the fastest network generation time.
        /// </summary>
        public TimeSpan FastestGeneration { get; set; }

        /// <summary>
        /// Gets or sets the slowest network generation time.
        /// </summary>
        public TimeSpan SlowestGeneration { get; set; }
    }

    /// <summary>
    /// Cross-network validation result.
    /// </summary>
    public class CrossNetworkValidationResult
    {
        /// <summary>
        /// Gets or sets whether all addresses are valid across networks.
        /// </summary>
        public bool AllValid { get; set; }

        /// <summary>
        /// Gets or sets any cross-network validation issues.
        /// </summary>
        public List<string> ValidationIssues { get; set; } = new();

        /// <summary>
        /// Gets or sets network compatibility information.
        /// </summary>
        public Dictionary<string, bool> NetworkCompatibility { get; set; } = new();
    }

    /// <summary>
    /// Validation summary across multiple networks.
    /// </summary>
    public class ValidationSummary
    {
        /// <summary>
        /// Gets or sets the total number of addresses validated.
        /// </summary>
        public int TotalAddresses { get; set; }

        /// <summary>
        /// Gets or sets the number of valid addresses.
        /// </summary>
        public int ValidAddresses { get; set; }

        /// <summary>
        /// Gets or sets the number of invalid addresses.
        /// </summary>
        public int InvalidAddresses { get; set; }

        /// <summary>
        /// Gets or sets the overall validation success rate.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets validation statistics by network.
        /// </summary>
        public Dictionary<string, NetworkValidationStats> NetworkStats { get; set; } = new();
    }

    /// <summary>
    /// Performance metrics for a specific network.
    /// </summary>
    public class NetworkPerformanceMetrics
    {
        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the average address generation time.
        /// </summary>
        public TimeSpan AverageGenerationTime { get; set; }

        /// <summary>
        /// Gets or sets the average validation time.
        /// </summary>
        public TimeSpan AverageValidationTime { get; set; }

        /// <summary>
        /// Gets or sets the network latency.
        /// </summary>
        public TimeSpan NetworkLatency { get; set; }

        /// <summary>
        /// Gets or sets the success rate for operations.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the number of operations performed.
        /// </summary>
        public int OperationCount { get; set; }

        /// <summary>
        /// Gets or sets additional performance metrics.
        /// </summary>
        public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Performance comparison between networks.
    /// </summary>
    public class PerformanceComparison
    {
        /// <summary>
        /// Gets or sets the fastest network for address generation.
        /// </summary>
        public string FastestGenerationNetwork { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the most reliable network.
        /// </summary>
        public string MostReliableNetwork { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network with lowest latency.
        /// </summary>
        public string LowestLatencyNetwork { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets performance rankings by metric.
        /// </summary>
        public Dictionary<string, List<string>> PerformanceRankings { get; set; } = new();
    }

    /// <summary>
    /// Metadata for a blockchain address.
    /// </summary>
    public class AddressMetadata
    {
        /// <summary>
        /// Gets or sets the blockchain address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the service name associated with this address.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the address was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets additional metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Relationship between addresses across networks.
    /// </summary>
    public class CrossNetworkRelationship
    {
        /// <summary>
        /// Gets or sets the primary address.
        /// </summary>
        public string PrimaryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the primary network type.
        /// </summary>
        public string PrimaryNetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the related address.
        /// </summary>
        public string RelatedAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the related network type.
        /// </summary>
        public string RelatedNetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relationship type.
        /// </summary>
        public string RelationshipType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the relationship was established.
        /// </summary>
        public DateTime EstablishedAt { get; set; }
    }

    /// <summary>
    /// Historical metadata for an address.
    /// </summary>
    public class HistoricalMetadata
    {
        /// <summary>
        /// Gets or sets the timestamp of the metadata.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the metadata values at this timestamp.
        /// </summary>
        public Dictionary<string, string> MetadataValues { get; set; } = new();

        /// <summary>
        /// Gets or sets the event that triggered this metadata update.
        /// </summary>
        public string TriggerEvent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test result for a specific network.
    /// </summary>
    public class NetworkTestResult
    {
        /// <summary>
        /// Gets or sets the network type tested.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the test was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the test execution time.
        /// </summary>
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the number of operations performed.
        /// </summary>
        public int OperationsPerformed { get; set; }

        /// <summary>
        /// Gets or sets the number of successful operations.
        /// </summary>
        public int SuccessfulOperations { get; set; }

        /// <summary>
        /// Gets or sets any errors encountered during testing.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets detailed test metrics.
        /// </summary>
        public Dictionary<string, double> TestMetrics { get; set; } = new();
    }

    /// <summary>
    /// Summary of multi-network testing.
    /// </summary>
    public class MultiNetworkTestSummary
    {
        /// <summary>
        /// Gets or sets the total number of networks tested.
        /// </summary>
        public int TotalNetworks { get; set; }

        /// <summary>
        /// Gets or sets the number of networks that passed all tests.
        /// </summary>
        public int SuccessfulNetworks { get; set; }

        /// <summary>
        /// Gets or sets the overall success rate.
        /// </summary>
        public double OverallSuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the total test execution time.
        /// </summary>
        public TimeSpan TotalExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the average execution time per network.
        /// </summary>
        public TimeSpan AverageExecutionTime { get; set; }
    }

    /// <summary>
    /// Test execution details.
    /// </summary>
    public class TestExecutionDetails
    {
        /// <summary>
        /// Gets or sets when the test started.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// Gets or sets when the test completed.
        /// </summary>
        public DateTime CompletedAt { get; set; }

        /// <summary>
        /// Gets or sets whether tests were run in parallel.
        /// </summary>
        public bool RunInParallel { get; set; }

        /// <summary>
        /// Gets or sets the maximum duration allowed.
        /// </summary>
        public TimeSpan MaxDuration { get; set; }

        /// <summary>
        /// Gets or sets whether the test completed within the time limit.
        /// </summary>
        public bool CompletedWithinTimeLimit { get; set; }
    }

    /// <summary>
    /// Network configuration details.
    /// </summary>
    public class NetworkConfiguration
    {
        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RPC endpoint.
        /// </summary>
        public string? RpcEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the chain ID.
        /// </summary>
        public long? ChainId { get; set; }

        /// <summary>
        /// Gets or sets whether this is a testnet configuration.
        /// </summary>
        public bool IsTestnet { get; set; }

        /// <summary>
        /// Gets or sets the configuration parameters.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Gets or sets when the configuration was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Relationship between two addresses.
    /// </summary>
    public class AddressRelationship
    {
        /// <summary>
        /// Gets or sets the first address.
        /// </summary>
        public string Address1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first address network type.
        /// </summary>
        public string NetworkType1 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the second address.
        /// </summary>
        public string Address2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the second address network type.
        /// </summary>
        public string NetworkType2 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relationship type.
        /// </summary>
        public string RelationshipType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validation statistics for a specific network.
    /// </summary>
    public class NetworkValidationStats
    {
        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total addresses validated.
        /// </summary>
        public int TotalValidated { get; set; }

        /// <summary>
        /// Gets or sets the number of valid addresses.
        /// </summary>
        public int ValidCount { get; set; }

        /// <summary>
        /// Gets or sets the validation success rate.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets the average validation time.
        /// </summary>
        public TimeSpan AverageValidationTime { get; set; }
    }

    #endregion
}
