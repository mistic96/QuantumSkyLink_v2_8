using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;

namespace LiquidationService.Services.Interfaces;

/// <summary>
/// Service interface for managing liquidity providers
/// </summary>
public interface ILiquidityProviderService
{
    /// <summary>
    /// Register a new liquidity provider
    /// </summary>
    /// <param name="userId">User ID of the provider</param>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered liquidity provider response</returns>
    Task<LiquidityProviderResponse> RegisterLiquidityProviderAsync(
        Guid userId, 
        RegisterLiquidityProviderModel request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquidity provider by ID
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidity provider response</returns>
    Task<LiquidityProviderResponse?> GetLiquidityProviderAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all liquidity providers with filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="country">Filter by country</param>
    /// <param name="assetSymbol">Filter by supported asset</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated liquidity providers</returns>
    Task<PaginatedResponse<LiquidityProviderResponse>> GetLiquidityProvidersAsync(
        string? status = null,
        string? country = null,
        string? assetSymbol = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update liquidity provider information
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="request">Update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider response</returns>
    Task<LiquidityProviderResponse> UpdateLiquidityProviderAsync(
        Guid providerId, 
        UpdateLiquidityProviderModel request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a liquidity provider
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider response</returns>
    Task<LiquidityProviderResponse> ApproveLiquidityProviderAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspend a liquidity provider
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="reason">Suspension reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider response</returns>
    Task<LiquidityProviderResponse> SuspendLiquidityProviderAsync(
        Guid providerId, 
        string reason, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the best liquidity provider for a liquidation request
    /// </summary>
    /// <param name="assetSymbol">Asset to liquidate</param>
    /// <param name="outputSymbol">Desired output currency</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best matching liquidity provider</returns>
    Task<LiquidityProviderSummaryResponse> FindBestLiquidityProviderAsync(
        string assetSymbol,
        string outputSymbol,
        decimal amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get liquidity provider statistics
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Provider statistics</returns>
    Task<object> GetLiquidityProviderStatisticsAsync(
        Guid providerId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update liquidity provider availability
    /// </summary>
    /// <param name="providerId">Liquidity provider ID</param>
    /// <param name="isAvailable">Availability status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidity provider response</returns>
    Task<LiquidityProviderResponse> UpdateAvailabilityAsync(
        Guid providerId, 
        bool isAvailable, 
        CancellationToken cancellationToken = default);
}
