using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;

namespace LiquidationService.Services.Interfaces;

/// <summary>
/// Service interface for managing asset eligibility rules and restrictions
/// </summary>
public interface IAssetEligibilityService
{
    /// <summary>
    /// Configure asset eligibility rules
    /// </summary>
    /// <param name="request">Asset eligibility configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility response</returns>
    Task<AssetEligibilityResponse> ConfigureAssetEligibilityAsync(
        ConfigureAssetEligibilityModel request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get asset eligibility by asset symbol
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility response</returns>
    Task<AssetEligibilityResponse?> GetAssetEligibilityAsync(
        string assetSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all asset eligibilities with filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="isEnabled">Filter by enabled status</param>
    /// <param name="riskLevel">Filter by risk level</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated asset eligibilities</returns>
    Task<PaginatedResponse<AssetEligibilityResponse>> GetAssetEligibilitiesAsync(
        string? status = null,
        bool? isEnabled = null,
        string? riskLevel = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an asset is eligible for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="userId">User ID</param>
    /// <param name="userCountry">User's country</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Eligibility status</returns>
    Task<bool> IsAssetEligibleForLiquidationAsync(
        string assetSymbol,
        decimal amount,
        Guid userId,
        string? userCountry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed eligibility validation for an asset
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="userId">User ID</param>
    /// <param name="userCountry">User's country</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed validation result</returns>
    Task<object> ValidateAssetEligibilityAsync(
        string assetSymbol,
        decimal amount,
        Guid userId,
        string? userCountry = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update asset eligibility status
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="isEnabled">New enabled status</param>
    /// <param name="reason">Reason for status change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset eligibility response</returns>
    Task<AssetEligibilityResponse> UpdateAssetStatusAsync(
        string assetSymbol,
        bool isEnabled,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported output currencies for an asset
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported output currencies</returns>
    Task<IEnumerable<string>> GetSupportedOutputCurrenciesAsync(
        string assetSymbol, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if asset liquidation is restricted in a country
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="country">Country code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Whether liquidation is restricted</returns>
    Task<bool> IsAssetRestrictedInCountryAsync(
        string assetSymbol,
        string country,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get asset eligibility statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility statistics</returns>
    Task<object> GetAssetEligibilityStatisticsAsync(
        CancellationToken cancellationToken = default);
}
