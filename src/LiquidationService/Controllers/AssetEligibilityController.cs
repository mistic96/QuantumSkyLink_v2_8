using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using System.Security.Claims;

namespace LiquidationService.Controllers;

/// <summary>
/// Controller for managing asset eligibility rules and restrictions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetEligibilityController : ControllerBase
{
    private readonly IAssetEligibilityService _assetEligibilityService;
    private readonly ILogger<AssetEligibilityController> _logger;

    public AssetEligibilityController(
        IAssetEligibilityService assetEligibilityService,
        ILogger<AssetEligibilityController> logger)
    {
        _assetEligibilityService = assetEligibilityService;
        _logger = logger;
    }

    /// <summary>
    /// Configure asset eligibility rules
    /// </summary>
    /// <param name="request">Asset eligibility configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility configuration result</returns>
    [HttpPost("configure")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AssetEligibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AssetEligibilityResponse>> ConfigureAssetEligibility(
        [FromBody] ConfigureAssetEligibilityModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Configuring asset eligibility for {AssetSymbol}", request.AssetSymbol);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _assetEligibilityService.ConfigureAssetEligibilityAsync(request, cancellationToken);
            
            _logger.LogInformation("Asset eligibility configured successfully for {AssetSymbol}", request.AssetSymbol);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for asset eligibility configuration");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring asset eligibility for {AssetSymbol}", request.AssetSymbol);
            return StatusCode(500, "An error occurred while configuring asset eligibility");
        }
    }

    /// <summary>
    /// Get asset eligibility by asset symbol
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility details</returns>
    [HttpGet("{assetSymbol}")]
    [ProducesResponseType(typeof(AssetEligibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AssetEligibilityResponse>> GetAssetEligibility(
        string assetSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving asset eligibility for {AssetSymbol}", assetSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            var result = await _assetEligibilityService.GetAssetEligibilityAsync(assetSymbol, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Asset eligibility not found for {AssetSymbol}", assetSymbol);
                return NotFound($"Asset eligibility for {assetSymbol} not found");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset eligibility for {AssetSymbol}", assetSymbol);
            return StatusCode(500, "An error occurred while retrieving asset eligibility");
        }
    }

    /// <summary>
    /// Get all asset eligibilities with filtering
    /// </summary>
    /// <param name="status">Filter by status</param>
    /// <param name="isEnabled">Filter by enabled status</param>
    /// <param name="riskLevel">Filter by risk level</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of asset eligibilities</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<AssetEligibilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<AssetEligibilityResponse>>> GetAssetEligibilities(
        [FromQuery] string? status = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] string? riskLevel = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving asset eligibilities - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            if (pageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            var result = await _assetEligibilityService.GetAssetEligibilitiesAsync(
                status, isEnabled, riskLevel, page, pageSize, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} asset eligibilities (Page {Page} of {TotalPages})", 
                result.Items.Count(), page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting asset eligibilities");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset eligibilities");
            return StatusCode(500, "An error occurred while retrieving asset eligibilities");
        }
    }

    /// <summary>
    /// Check if an asset is eligible for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="userCountry">User's country</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Eligibility status</returns>
    [HttpGet("{assetSymbol}/eligible")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> IsAssetEligibleForLiquidation(
        string assetSymbol,
        [FromQuery] decimal amount,
        [FromQuery] string? userCountry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Checking asset eligibility for {AssetSymbol} amount {Amount} for user {UserId}", 
                assetSymbol, amount, currentUserId);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _assetEligibilityService.IsAssetEligibleForLiquidationAsync(
                assetSymbol, amount, currentUserId, userCountry, cancellationToken);
            
            _logger.LogInformation("Asset eligibility check for {AssetSymbol}: {IsEligible}", assetSymbol, result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for asset eligibility check");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking asset eligibility for {AssetSymbol}", assetSymbol);
            return StatusCode(500, "An error occurred while checking asset eligibility");
        }
    }

    /// <summary>
    /// Get detailed eligibility validation for an asset
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="userCountry">User's country</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed validation result</returns>
    [HttpGet("{assetSymbol}/validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> ValidateAssetEligibility(
        string assetSymbol,
        [FromQuery] decimal amount,
        [FromQuery] string? userCountry = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Validating asset eligibility for {AssetSymbol} amount {Amount} for user {UserId}", 
                assetSymbol, amount, currentUserId);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _assetEligibilityService.ValidateAssetEligibilityAsync(
                assetSymbol, amount, currentUserId, userCountry, cancellationToken);
            
            _logger.LogInformation("Asset eligibility validation completed for {AssetSymbol}", assetSymbol);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for asset eligibility validation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating asset eligibility for {AssetSymbol}", assetSymbol);
            return StatusCode(500, "An error occurred while validating asset eligibility");
        }
    }

    /// <summary>
    /// Update asset eligibility status
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="isEnabled">New enabled status</param>
    /// <param name="reason">Reason for status change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset eligibility</returns>
    [HttpPut("{assetSymbol}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AssetEligibilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AssetEligibilityResponse>> UpdateAssetStatus(
        string assetSymbol,
        [FromQuery] bool isEnabled,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating asset status for {AssetSymbol} to {IsEnabled}", assetSymbol, isEnabled);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            var result = await _assetEligibilityService.UpdateAssetStatusAsync(
                assetSymbol, isEnabled, reason, cancellationToken);
            
            _logger.LogInformation("Asset status updated successfully for {AssetSymbol}", assetSymbol);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating asset status");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset status for {AssetSymbol}", assetSymbol);
            return StatusCode(500, "An error occurred while updating asset status");
        }
    }

    /// <summary>
    /// Get supported output currencies for an asset
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported output currencies</returns>
    [HttpGet("{assetSymbol}/output-currencies")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedOutputCurrencies(
        string assetSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving supported output currencies for {AssetSymbol}", assetSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            var result = await _assetEligibilityService.GetSupportedOutputCurrenciesAsync(assetSymbol, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} supported output currencies for {AssetSymbol}", 
                result.Count(), assetSymbol);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported output currencies for {AssetSymbol}", assetSymbol);
            return StatusCode(500, "An error occurred while retrieving supported output currencies");
        }
    }

    /// <summary>
    /// Check if asset liquidation is restricted in a country
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="country">Country code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Whether liquidation is restricted</returns>
    [HttpGet("{assetSymbol}/restricted/{country}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> IsAssetRestrictedInCountry(
        string assetSymbol,
        string country,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking if {AssetSymbol} is restricted in {Country}", assetSymbol, country);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(country))
            {
                return BadRequest("Country is required");
            }

            var result = await _assetEligibilityService.IsAssetRestrictedInCountryAsync(
                assetSymbol, country, cancellationToken);
            
            _logger.LogInformation("Asset {AssetSymbol} restriction status in {Country}: {IsRestricted}", 
                assetSymbol, country, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking asset restriction for {AssetSymbol} in {Country}", assetSymbol, country);
            return StatusCode(500, "An error occurred while checking asset restriction");
        }
    }

    /// <summary>
    /// Get asset eligibility statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Asset eligibility statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetAssetEligibilityStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving asset eligibility statistics");

            var stats = await _assetEligibilityService.GetAssetEligibilityStatisticsAsync(cancellationToken);
            
            _logger.LogInformation("Retrieved asset eligibility statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset eligibility statistics");
            return StatusCode(500, "An error occurred while retrieving asset eligibility statistics");
        }
    }

    #region Private Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    #endregion
}
