using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;

namespace InfrastructureService.Services.Interfaces
{
    /// <summary>
    /// Service interface for multi-network blockchain operations.
    /// </summary>
    public interface IMultiNetworkService
    {
        /// <summary>
        /// Generates addresses across multiple blockchain networks for a service.
        /// </summary>
        /// <param name="request">The multi-network generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network generation response with addresses for each network.</returns>
        Task<MultiNetworkGenerateResponse> GenerateMultiNetworkAddressesAsync(
            MultiNetworkGenerateRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates addresses across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The multi-network validation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network validation response with results for each address.</returns>
        Task<MultiNetworkValidateResponse> ValidateMultiNetworkAddressesAsync(
            MultiNetworkValidateRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Compares performance metrics across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The network performance comparison request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network performance comparison response with metrics and analysis.</returns>
        Task<NetworkPerformanceComparisonResponse> CompareNetworkPerformanceAsync(
            NetworkPerformanceComparisonRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves cross-network metadata for an address.
        /// </summary>
        /// <param name="request">The cross-network metadata request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Cross-network metadata response with related addresses and relationships.</returns>
        Task<CrossNetworkMetadataResponse> GetCrossNetworkMetadataAsync(
            CrossNetworkMetadataRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs comprehensive testing across multiple blockchain networks.
        /// </summary>
        /// <param name="request">The multi-network test request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Multi-network test response with results for each network.</returns>
        Task<MultiNetworkTestResponse> RunMultiNetworkTestAsync(
            MultiNetworkTestRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Configures network-specific settings for a blockchain network.
        /// </summary>
        /// <param name="request">The network configuration request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network configuration response with applied settings.</returns>
        Task<NetworkConfigurationResponse> ConfigureNetworkAsync(
            NetworkConfigurationRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of supported blockchain networks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>List of supported network types with their capabilities.</returns>
        Task<List<SupportedNetwork>> GetSupportedNetworksAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets network-specific configuration for a blockchain network.
        /// </summary>
        /// <param name="networkType">The network type to get configuration for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network configuration details.</returns>
        Task<NetworkConfiguration> GetNetworkConfigurationAsync(
            string networkType, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores cross-network metadata relationships between addresses.
        /// </summary>
        /// <param name="serviceName">The service name associated with the addresses.</param>
        /// <param name="addresses">The addresses to create relationships for.</param>
        /// <param name="metadata">Additional metadata to store.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created cross-network metadata.</returns>
        Task<CrossNetworkMetadata> StoreCrossNetworkMetadataAsync(
            string serviceName,
            List<GenerateAddressResponse> addresses,
            Dictionary<string, string>? metadata = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets network health status for all supported networks.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Network health status for each supported network.</returns>
        Task<Dictionary<string, NetworkHealthStatus>> GetNetworkHealthStatusAsync(
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a supported blockchain network.
    /// </summary>
    public class SupportedNetwork
    {
        /// <summary>
        /// Gets or sets the network type identifier.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the network.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the network description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the network is currently available.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the supported capabilities for this network.
        /// </summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>
        /// Gets or sets the chain ID for EVM-compatible networks.
        /// </summary>
        public long? ChainId { get; set; }

        /// <summary>
        /// Gets or sets whether this is a testnet.
        /// </summary>
        public bool IsTestnet { get; set; }

        /// <summary>
        /// Gets or sets additional network properties.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();
    }

    /// <summary>
    /// Represents the health status of a blockchain network.
    /// </summary>
    public class NetworkHealthStatus
    {
        /// <summary>
        /// Gets or sets the network type.
        /// </summary>
        public string NetworkType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the network is healthy.
        /// </summary>
        public bool IsHealthy { get; set; }

        /// <summary>
        /// Gets or sets the network connectivity status.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the current block height.
        /// </summary>
        public long? BlockHeight { get; set; }

        /// <summary>
        /// Gets or sets the network latency in milliseconds.
        /// </summary>
        public double? LatencyMs { get; set; }

        /// <summary>
        /// Gets or sets the last successful operation timestamp.
        /// </summary>
        public DateTime? LastSuccessfulOperation { get; set; }

        /// <summary>
        /// Gets or sets any health issues or warnings.
        /// </summary>
        public List<string> HealthIssues { get; set; } = new();

        /// <summary>
        /// Gets or sets when the health check was performed.
        /// </summary>
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
