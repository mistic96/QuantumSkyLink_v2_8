using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests
{
    /// <summary>
    /// Request to generate addresses across multiple blockchain networks.
    /// </summary>
    public class MultiNetworkGenerateRequest
    {
        /// <summary>
        /// Gets or sets the service name for which to generate addresses.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of blockchain networks to generate addresses for.
        /// Supported networks: Bitcoin, Ethereum, Polygon, BSC, Avalanche, MultiChain
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> NetworkTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets optional metadata for the address generation.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets whether to store cross-network metadata relationships.
        /// </summary>
        public bool StoreCrossNetworkMetadata { get; set; } = true;
    }

    /// <summary>
    /// Request to validate addresses across multiple blockchain networks.
    /// </summary>
    public class MultiNetworkValidateRequest
    {
        /// <summary>
        /// Gets or sets the addresses to validate with their network types.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<NetworkAddressPair> Addresses { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to perform cross-network validation checks.
        /// </summary>
        public bool PerformCrossNetworkValidation { get; set; } = false;
    }

    /// <summary>
    /// Request to get network performance comparison across multiple networks.
    /// </summary>
    public class NetworkPerformanceComparisonRequest
    {
        /// <summary>
        /// Gets or sets the list of networks to compare.
        /// </summary>
        [Required]
        [MinLength(2)]
        public List<string> NetworkTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the time period for performance analysis (in hours).
        /// </summary>
        [Range(1, 168)] // 1 hour to 1 week
        public int TimePeriodHours { get; set; } = 24;

        /// <summary>
        /// Gets or sets the metrics to include in the comparison.
        /// </summary>
        public List<string> MetricsToInclude { get; set; } = new() 
        { 
            "AddressGenerationTime", 
            "ValidationTime", 
            "NetworkLatency", 
            "SuccessRate" 
        };
    }

    /// <summary>
    /// Request to get cross-network metadata for an address.
    /// </summary>
    public class CrossNetworkMetadataRequest
    {
        /// <summary>
        /// Gets or sets the primary address to query metadata for.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network type of the primary address.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether to include related addresses from other networks.
        /// </summary>
        public bool IncludeRelatedAddresses { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include historical metadata.
        /// </summary>
        public bool IncludeHistoricalData { get; set; } = false;
    }

    /// <summary>
    /// Request to perform multi-network testing.
    /// </summary>
    public class MultiNetworkTestRequest
    {
        /// <summary>
        /// Gets or sets the list of networks to test.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<string> NetworkTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets the number of test operations per network.
        /// </summary>
        [Range(1, 10000)]
        public int OperationsPerNetwork { get; set; } = 100;

        /// <summary>
        /// Gets or sets the test types to perform.
        /// </summary>
        public List<string> TestTypes { get; set; } = new() 
        { 
            "AddressGeneration", 
            "AddressValidation", 
            "NetworkConnectivity" 
        };

        /// <summary>
        /// Gets or sets whether to run tests in parallel across networks.
        /// </summary>
        public bool RunInParallel { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum test duration in minutes.
        /// </summary>
        [Range(1, 60)]
        public int MaxDurationMinutes { get; set; } = 10;
    }

    /// <summary>
    /// Represents an address-network pair for validation.
    /// </summary>
    public class NetworkAddressPair
    {
        /// <summary>
        /// Gets or sets the blockchain address.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional metadata for this address-network pair.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }

    /// <summary>
    /// Request to configure network-specific settings.
    /// </summary>
    public class NetworkConfigurationRequest
    {
        /// <summary>
        /// Gets or sets the network type to configure.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RPC endpoint URL for the network.
        /// </summary>
        public string? RpcEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the chain ID for EVM-compatible networks.
        /// </summary>
        public long? ChainId { get; set; }

        /// <summary>
        /// Gets or sets the network-specific configuration parameters.
        /// </summary>
        public Dictionary<string, string> ConfigurationParameters { get; set; } = new();

        /// <summary>
        /// Gets or sets whether this configuration is for testnet.
        /// </summary>
        public bool IsTestnet { get; set; } = false;
    }
}
