namespace InfrastructureService.Models.Responses
{
    /// <summary>
    /// Response containing a generated blockchain address.
    /// </summary>
    public class GenerateAddressResponse
    {
        /// <summary>
        /// Gets or sets the service name for which the address was generated.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated blockchain address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blockchain network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public key associated with the address.
        /// </summary>
        public string PublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the private key associated with the address (if requested).
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the address was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the address.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Response containing multiple generated blockchain addresses.
    /// </summary>
    public class BulkGenerateAddressResponse
    {
        /// <summary>
        /// Gets or sets the list of generated addresses.
        /// </summary>
        public List<GenerateAddressResponse> Addresses { get; set; } = new();

        /// <summary>
        /// Gets or sets the blockchain network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of addresses generated.
        /// </summary>
        public int TotalGenerated { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the bulk generation was completed.
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Gets or sets the time taken to generate all addresses.
        /// </summary>
        public TimeSpan GenerationTime { get; set; }
    }

    /// <summary>
    /// Response containing address validation results.
    /// </summary>
    public class ValidateAddressResponse
    {
        /// <summary>
        /// Gets or sets the validated address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the address is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the blockchain network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address format (e.g., "Legacy", "SegWit", "Bech32").
        /// </summary>
        public string AddressFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets validation error messages if the address is invalid.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Gets or sets additional validation metadata.
        /// </summary>
        public Dictionary<string, string> ValidationMetadata { get; set; } = new();
    }

    /// <summary>
    /// Response containing detailed address information.
    /// </summary>
    public class AddressInfoResponse
    {
        /// <summary>
        /// Gets or sets the blockchain address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the blockchain network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current balance of the address.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the transaction count for the address.
        /// </summary>
        public long TransactionCount { get; set; }

        /// <summary>
        /// Gets or sets whether the address is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the last activity timestamp.
        /// </summary>
        public DateTime? LastActivity { get; set; }

        /// <summary>
        /// Gets or sets token balances for the address.
        /// </summary>
        public Dictionary<string, decimal> TokenBalances { get; set; } = new();

        /// <summary>
        /// Gets or sets additional address metadata.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Response containing blockchain network statistics.
    /// </summary>
    public class NetworkStatsResponse
    {
        /// <summary>
        /// Gets or sets the blockchain network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the network is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the current block height.
        /// </summary>
        public long BlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the number of connected peers.
        /// </summary>
        public int PeerCount { get; set; }

        /// <summary>
        /// Gets or sets the network version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last block timestamp.
        /// </summary>
        public DateTime LastBlockTime { get; set; }

        /// <summary>
        /// Gets or sets additional network information.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; } = new();
    }

    /// <summary>
    /// Response containing address generation performance metrics.
    /// </summary>
    public class AddressGenerationMetricsResponse
    {
        /// <summary>
        /// Gets or sets the total number of addresses generated.
        /// </summary>
        public int TotalAddressesGenerated { get; set; }

        /// <summary>
        /// Gets or sets the average generation time per address.
        /// </summary>
        public TimeSpan AverageGenerationTime { get; set; }

        /// <summary>
        /// Gets or sets the fastest generation time.
        /// </summary>
        public TimeSpan FastestGenerationTime { get; set; }

        /// <summary>
        /// Gets or sets the slowest generation time.
        /// </summary>
        public TimeSpan SlowestGenerationTime { get; set; }

        /// <summary>
        /// Gets or sets the success rate percentage.
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Gets or sets generation metrics by network type.
        /// </summary>
        public Dictionary<string, NetworkGenerationMetrics> NetworkMetrics { get; set; } = new();
    }

    /// <summary>
    /// Metrics for address generation on a specific network.
    /// </summary>
    public class NetworkGenerationMetrics
    {
        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of addresses generated on this network.
        /// </summary>
        public int AddressCount { get; set; }

        /// <summary>
        /// Gets or sets the average generation time for this network.
        /// </summary>
        public TimeSpan AverageTime { get; set; }

        /// <summary>
        /// Gets or sets the success rate for this network.
        /// </summary>
        public double SuccessRate { get; set; }
    }
}
