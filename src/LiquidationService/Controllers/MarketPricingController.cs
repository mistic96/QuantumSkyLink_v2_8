using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using System.Security.Claims;

namespace LiquidationService.Controllers;

/// <summary>
/// Controller for market pricing and price discovery
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketPricingController : ControllerBase
{
    private readonly IMarketPricingService _marketPricingService;
    private readonly ILogger<MarketPricingController> _logger;

    public MarketPricingController(
        IMarketPricingService marketPricingService,
        ILogger<MarketPricingController> logger)
    {
        _marketPricingService = marketPricingService;
        _logger = logger;
    }

    /// <summary>
    /// Get current market price for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current market price snapshot</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(MarketPriceSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketPriceSnapshotResponse>> GetCurrentPrice(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting current price for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            var result = await _marketPricingService.GetCurrentPriceAsync(assetSymbol, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Retrieved current price for {AssetSymbol}/{OutputSymbol}: {Price}", 
                assetSymbol, outputSymbol, result.Price);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for current price");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting current price");
        }
    }

    /// <summary>
    /// Get price with slippage estimation for a specific amount
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price snapshot with slippage estimation</returns>
    [HttpGet("slippage")]
    [ProducesResponseType(typeof(MarketPriceSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketPriceSnapshotResponse>> GetPriceWithSlippage(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting price with slippage for {AssetSymbol}/{OutputSymbol} amount {Amount}", 
                assetSymbol, outputSymbol, amount);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _marketPricingService.GetPriceWithSlippageAsync(assetSymbol, outputSymbol, amount, cancellationToken);
            
            _logger.LogInformation("Retrieved price with slippage for {AssetSymbol}/{OutputSymbol}: {Price} (Slippage: {Slippage}%)", 
                assetSymbol, outputSymbol, result.Price, result.EstimatedSlippage);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for price with slippage");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price with slippage for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting price with slippage");
        }
    }

    /// <summary>
    /// Get historical prices for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical price snapshots</returns>
    [HttpGet("historical")]
    [ProducesResponseType(typeof(IEnumerable<MarketPriceSnapshotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MarketPriceSnapshotResponse>>> GetHistoricalPrices(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting historical prices for {AssetSymbol}/{OutputSymbol} from {FromDate} to {ToDate}", 
                assetSymbol, outputSymbol, fromDate, toDate);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (fromDate >= toDate)
            {
                return BadRequest("From date must be before to date");
            }

            if (toDate > DateTime.UtcNow)
            {
                return BadRequest("To date cannot be in the future");
            }

            var result = await _marketPricingService.GetHistoricalPricesAsync(assetSymbol, outputSymbol, fromDate, toDate, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} historical prices for {AssetSymbol}/{OutputSymbol}", 
                result.Count(), assetSymbol, outputSymbol);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for historical prices");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical prices for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting historical prices");
        }
    }

    /// <summary>
    /// Validate if a price is suitable for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="maxSlippagePercent">Maximum acceptable slippage percentage</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price validation result</returns>
    [HttpGet("validate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> ValidatePriceForLiquidation(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] decimal amount,
        [FromQuery] decimal maxSlippagePercent = 5.0m,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating price for liquidation {AssetSymbol}/{OutputSymbol} amount {Amount} with max slippage {MaxSlippage}%", 
                assetSymbol, outputSymbol, amount, maxSlippagePercent);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            if (maxSlippagePercent < 0 || maxSlippagePercent > 100)
            {
                return BadRequest("Max slippage percent must be between 0 and 100");
            }

            var result = await _marketPricingService.ValidatePriceForLiquidationAsync(
                assetSymbol, outputSymbol, amount, maxSlippagePercent, cancellationToken);
            
            _logger.LogInformation("Price validation for {AssetSymbol}/{OutputSymbol}: {IsValid}", 
                assetSymbol, outputSymbol, result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for price validation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating price for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while validating price");
        }
    }

    /// <summary>
    /// Get best available price from multiple sources
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="amount">Amount to liquidate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best price snapshot</returns>
    [HttpGet("best")]
    [ProducesResponseType(typeof(MarketPriceSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketPriceSnapshotResponse>> GetBestPrice(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting best price for {AssetSymbol}/{OutputSymbol} amount {Amount}", 
                assetSymbol, outputSymbol, amount);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero");
            }

            var result = await _marketPricingService.GetBestPriceAsync(assetSymbol, outputSymbol, amount, cancellationToken);
            
            _logger.LogInformation("Retrieved best price for {AssetSymbol}/{OutputSymbol}: {Price} from {Exchange}", 
                assetSymbol, outputSymbol, result.Price, result.Exchange);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for best price");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best price for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting best price");
        }
    }

    /// <summary>
    /// Update price data from external sources
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated price snapshot</returns>
    [HttpPost("refresh")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(MarketPriceSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MarketPriceSnapshotResponse>> RefreshPriceData(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refreshing price data for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            var result = await _marketPricingService.RefreshPriceDataAsync(assetSymbol, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Price data refreshed for {AssetSymbol}/{OutputSymbol}: {Price}", 
                assetSymbol, outputSymbol, result.Price);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for price data refresh");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing price data for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while refreshing price data");
        }
    }

    /// <summary>
    /// Get price confidence level for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confidence level (0-100)</returns>
    [HttpGet("confidence")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GetPriceConfidenceLevel(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting price confidence level for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            var result = await _marketPricingService.GetPriceConfidenceLevelAsync(assetSymbol, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Price confidence level for {AssetSymbol}/{OutputSymbol}: {ConfidenceLevel}%", 
                assetSymbol, outputSymbol, result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price confidence level for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting price confidence level");
        }
    }

    /// <summary>
    /// Calculate estimated output amount for liquidation
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="assetAmount">Amount of asset to liquidate</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="includeSlippage">Whether to include slippage in calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Estimated output amount</returns>
    [HttpGet("estimate")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<decimal>> CalculateEstimatedOutput(
        [FromQuery] string assetSymbol,
        [FromQuery] decimal assetAmount,
        [FromQuery] string outputSymbol,
        [FromQuery] bool includeSlippage = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating estimated output for {AssetSymbol} {AssetAmount} to {OutputSymbol} (Include slippage: {IncludeSlippage})", 
                assetSymbol, assetAmount, outputSymbol, includeSlippage);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (assetAmount <= 0)
            {
                return BadRequest("Asset amount must be greater than zero");
            }

            var result = await _marketPricingService.CalculateEstimatedOutputAsync(
                assetSymbol, assetAmount, outputSymbol, includeSlippage, cancellationToken);
            
            _logger.LogInformation("Estimated output for {AssetSymbol} {AssetAmount} to {OutputSymbol}: {EstimatedOutput}", 
                assetSymbol, assetAmount, outputSymbol, result);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for estimated output calculation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating estimated output for {AssetSymbol} to {OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while calculating estimated output");
        }
    }

    /// <summary>
    /// Get supported trading pairs
    /// </summary>
    /// <param name="assetSymbol">Asset symbol (optional filter)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of supported trading pairs</returns>
    [HttpGet("trading-pairs")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<string>>> GetSupportedTradingPairs(
        [FromQuery] string? assetSymbol = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting supported trading pairs for asset {AssetSymbol}", assetSymbol ?? "all");

            var result = await _marketPricingService.GetSupportedTradingPairsAsync(assetSymbol, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} supported trading pairs", result.Count());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported trading pairs");
            return StatusCode(500, "An error occurred while getting supported trading pairs");
        }
    }

    /// <summary>
    /// Get market depth for an asset pair
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Market depth information</returns>
    [HttpGet("market-depth")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetMarketDepth(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting market depth for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            var result = await _marketPricingService.GetMarketDepthAsync(assetSymbol, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Retrieved market depth for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market depth for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting market depth");
        }
    }

    /// <summary>
    /// Setup price alerts and notifications
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="priceThreshold">Price threshold for alerts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price alert configuration</returns>
    [HttpPost("alerts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> SetupPriceAlert(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] decimal priceThreshold,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting up price alert for {AssetSymbol}/{OutputSymbol} at threshold {PriceThreshold}", 
                assetSymbol, outputSymbol, priceThreshold);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (priceThreshold <= 0)
            {
                return BadRequest("Price threshold must be greater than zero");
            }

            var result = await _marketPricingService.SetupPriceAlertAsync(assetSymbol, outputSymbol, priceThreshold, cancellationToken);
            
            _logger.LogInformation("Price alert setup for {AssetSymbol}/{OutputSymbol} at threshold {PriceThreshold}", 
                assetSymbol, outputSymbol, priceThreshold);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for price alert setup");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up price alert for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while setting up price alert");
        }
    }

    /// <summary>
    /// Get pricing statistics and analytics
    /// </summary>
    /// <param name="assetSymbol">Asset symbol</param>
    /// <param name="outputSymbol">Output currency symbol</param>
    /// <param name="fromDate">Start date for statistics</param>
    /// <param name="toDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Pricing statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetPricingStatistics(
        [FromQuery] string assetSymbol,
        [FromQuery] string outputSymbol,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting pricing statistics for {AssetSymbol}/{OutputSymbol} from {FromDate} to {ToDate}", 
                assetSymbol, outputSymbol, fromDate, toDate);

            if (string.IsNullOrWhiteSpace(assetSymbol))
            {
                return BadRequest("Asset symbol is required");
            }

            if (string.IsNullOrWhiteSpace(outputSymbol))
            {
                return BadRequest("Output symbol is required");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value >= toDate.Value)
            {
                return BadRequest("From date must be before to date");
            }

            var result = await _marketPricingService.GetPricingStatisticsAsync(
                assetSymbol, outputSymbol, fromDate, toDate, cancellationToken);
            
            _logger.LogInformation("Retrieved pricing statistics for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for pricing statistics");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing statistics for {AssetSymbol}/{OutputSymbol}", assetSymbol, outputSymbol);
            return StatusCode(500, "An error occurred while getting pricing statistics");
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
