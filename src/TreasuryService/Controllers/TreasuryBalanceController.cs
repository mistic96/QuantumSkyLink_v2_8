using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TreasuryService.Services.Interfaces;
using TreasuryService.Models.Requests;
using TreasuryService.Models.Responses;

namespace TreasuryService.Controllers;

/// <summary>
/// Controller for treasury balance management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TreasuryBalanceController : ControllerBase
{
    private readonly ITreasuryService _treasuryService;
    private readonly ILogger<TreasuryBalanceController> _logger;

    public TreasuryBalanceController(
        ITreasuryService treasuryService,
        ILogger<TreasuryBalanceController> logger)
    {
        _treasuryService = treasuryService;
        _logger = logger;
    }

    /// <summary>
    /// Get current balance for a treasury account (with Redis caching)
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <returns>Current balance details</returns>
    [HttpGet("{accountId}/current")]
    [ProducesResponseType(typeof(TreasuryBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryBalanceResponse>> GetCurrentBalance(
        [FromRoute] Guid accountId)
    {
        try
        {
            _logger.LogInformation("Getting current balance for treasury account: {AccountId}", accountId);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            var balance = await _treasuryService.GetCurrentBalanceAsync(accountId);
            
            if (balance == null)
            {
                return NotFound($"Balance not found for treasury account: {accountId}");
            }

            return Ok(balance);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid balance request for account: {AccountId}", accountId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current balance for account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving the current balance");
        }
    }

    /// <summary>
    /// Get balance history for a treasury account with pagination and filtering
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="startDate">Optional start date filter (ISO 8601 format)</param>
    /// <param name="endDate">Optional end date filter (ISO 8601 format)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of balance history records</returns>
    [HttpGet("{accountId}/history")]
    [ProducesResponseType(typeof(IEnumerable<TreasuryBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TreasuryBalanceResponse>>> GetBalanceHistory(
        [FromRoute] Guid accountId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting balance history for account: {AccountId}, StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, Size: {PageSize}", 
                accountId, startDate, endDate, page, pageSize);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest("Start date cannot be greater than end date");
            }

            var request = new GetBalanceHistoryRequest
            {
                AccountId = accountId,
                FromDate = startDate,
                ToDate = endDate,
                Page = page,
                PageSize = pageSize
            };

            var history = await _treasuryService.GetBalanceHistoryAsync(request);
            return Ok(history);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid balance history request for account: {AccountId}", accountId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance history for account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while retrieving balance history");
        }
    }

    /// <summary>
    /// Reconcile balance with external systems and calculate differences
    /// </summary>
    /// <param name="accountId">Treasury account ID</param>
    /// <param name="request">Balance reconciliation request</param>
    /// <returns>Reconciliation result with differences and audit trail</returns>
    [HttpPost("{accountId}/reconcile")]
    [ProducesResponseType(typeof(TreasuryBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TreasuryBalanceResponse>> ReconcileBalance(
        [FromRoute] Guid accountId,
        [FromBody] ReconcileBalanceRequest request)
    {
        try
        {
            _logger.LogInformation("Reconciling balance for treasury account: {AccountId}, External Balance: {ExternalBalance}", 
                accountId, request.ExternalBalance);

            if (accountId == Guid.Empty)
            {
                return BadRequest("Account ID cannot be empty");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the account ID in the route matches the request
            if (request.AccountId != accountId)
            {
                return BadRequest("Account ID in route must match account ID in request body");
            }

            var result = await _treasuryService.ReconcileBalanceAsync(request);
            
            if (result == null)
            {
                return NotFound($"Treasury account not found: {accountId}");
            }

            _logger.LogInformation("Balance reconciliation completed for account: {AccountId}, Difference: {Difference}", 
                accountId, result.Balance - request.ExternalBalance);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid balance reconciliation request for account: {AccountId}", accountId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Balance reconciliation failed for account: {AccountId}", accountId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling balance for account: {AccountId}", accountId);
            return StatusCode(500, "An error occurred while reconciling the balance");
        }
    }
}
