using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasuryService.Services.Interfaces;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Controllers;

/// <summary>
/// Controller for treasury analytics and reporting operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TreasuryAnalyticsController : ControllerBase
{
    private readonly ITreasuryService _treasuryService;
    private readonly ILogger<TreasuryAnalyticsController> _logger;

    public TreasuryAnalyticsController(
        ITreasuryService treasuryService,
        ILogger<TreasuryAnalyticsController> logger)
    {
        _treasuryService = treasuryService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive treasury analytics with balance statistics and cash flow analysis
    /// </summary>
    /// <param name="fromDate">Optional start date filter (ISO 8601 format)</param>
    /// <param name="toDate">Optional end date filter (ISO 8601 format)</param>
    /// <param name="currency">Optional currency filter</param>
    /// <param name="accountType">Optional account type filter</param>
    /// <returns>Treasury analytics with comprehensive financial metrics</returns>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(TreasuryAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryAnalyticsResponse>> GetTreasuryAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? currency = null,
        [FromQuery] string? accountType = null)
    {
        try
        {
            _logger.LogInformation("Getting treasury analytics - FromDate: {FromDate}, ToDate: {ToDate}, Currency: {Currency}, AccountType: {AccountType}", 
                fromDate, toDate, currency, accountType);

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            // Default to last 30 days if no date range specified
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
                fromDate = toDate.Value.AddDays(-30);
            }

            var request = new GetTreasuryAnalyticsRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                Currency = currency,
                AccountType = accountType
            };

            var analytics = await _treasuryService.GetTreasuryAnalyticsAsync(request);
            
            if (analytics == null)
            {
                return NotFound("No treasury analytics data found for the specified criteria");
            }

            _logger.LogInformation("Treasury analytics retrieved successfully for period {FromDate} to {ToDate}", fromDate, toDate);
            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid treasury analytics request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury analytics");
            return StatusCode(500, "An error occurred while retrieving treasury analytics");
        }
    }

    /// <summary>
    /// Get account summaries with transaction counts and balance changes
    /// </summary>
    /// <returns>List of treasury account summaries</returns>
    [HttpGet("account-summaries")]
    [ProducesResponseType(typeof(IEnumerable<TreasuryAccountSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreasuryAccountSummaryResponse>>> GetAccountSummaries()
    {
        try
        {
            _logger.LogInformation("Getting treasury account summaries");

            var summaries = await _treasuryService.GetAccountSummariesAsync();
            
            _logger.LogInformation("Retrieved {Count} treasury account summaries", summaries?.Count() ?? 0);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting treasury account summaries");
            return StatusCode(500, "An error occurred while retrieving account summaries");
        }
    }

    /// <summary>
    /// Get cash flow analysis with inflow/outflow categorization and daily tracking
    /// </summary>
    /// <param name="fromDate">Optional start date filter (ISO 8601 format)</param>
    /// <param name="toDate">Optional end date filter (ISO 8601 format)</param>
    /// <param name="currency">Optional currency filter</param>
    /// <param name="accountId">Optional specific account ID filter</param>
    /// <returns>Cash flow analysis with detailed breakdown</returns>
    [HttpGet("cash-flow")]
    [ProducesResponseType(typeof(TreasuryCashFlowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryCashFlowResponse>> GetCashFlowAnalysis(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? currency = null,
        [FromQuery] Guid? accountId = null)
    {
        try
        {
            _logger.LogInformation("Getting cash flow analysis - FromDate: {FromDate}, ToDate: {ToDate}, Currency: {Currency}, AccountId: {AccountId}", 
                fromDate, toDate, currency, accountId);

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            // Default to last 30 days if no date range specified
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                toDate = DateTime.UtcNow;
                fromDate = toDate.Value.AddDays(-30);
            }

            var request = new GetCashFlowAnalysisRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                Currency = currency,
                AccountId = accountId
            };

            var cashFlow = await _treasuryService.GetCashFlowAnalysisAsync(request);
            
            if (cashFlow == null)
            {
                return NotFound("No cash flow data found for the specified criteria");
            }

            _logger.LogInformation("Cash flow analysis retrieved successfully for period {FromDate} to {ToDate}", fromDate, toDate);
            return Ok(cashFlow);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid cash flow analysis request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cash flow analysis");
            return StatusCode(500, "An error occurred while retrieving cash flow analysis");
        }
    }
}
