using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;

namespace InfrastructureService.Services.Interfaces
{
    /// <summary>
    /// Service interface for blockchain address generation and management.
    /// </summary>
    public interface IBlockchainAddressService
    {
        /// <summary>
        /// Generates a blockchain address for a specific service.
        /// </summary>
        /// <param name="request">The address generation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The generated address response.</returns>
        Task<GenerateAddressResponse> GenerateAddressAsync(GenerateAddressRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates blockchain addresses for multiple services.
        /// </summary>
        /// <param name="request">The bulk address generation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk generation response.</returns>
        Task<BulkGenerateAddressResponse> BulkGenerateAddressesAsync(BulkGenerateAddressRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a blockchain address.
        /// </summary>
        /// <param name="request">The address validation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The validation response.</returns>
        Task<ValidateAddressResponse> ValidateAddressAsync(ValidateAddressRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets detailed information about a blockchain address.
        /// </summary>
        /// <param name="request">The address info request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The address information response.</returns>
        Task<AddressInfoResponse> GetAddressInfoAsync(GetAddressInfoRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets network statistics for a specific blockchain network.
        /// </summary>
        /// <param name="networkType">The blockchain network type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The network statistics response.</returns>
        Task<NetworkStatsResponse> GetNetworkStatsAsync(string networkType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets address generation performance metrics.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The performance metrics response.</returns>
        Task<AddressGenerationMetricsResponse> GetGenerationMetricsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates addresses for all registered services on a specific network.
        /// </summary>
        /// <param name="networkType">The blockchain network type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The bulk generation response for all services.</returns>
        Task<BulkGenerateAddressResponse> GenerateAddressesForAllServicesAsync(string networkType, CancellationToken cancellationToken = default);
    }
}
