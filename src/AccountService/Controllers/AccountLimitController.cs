using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccountService.Models.Requests;
using AccountService.Models.Responses;
using AccountService.Services.Interfaces;
using AccountService.Data.Entities;
using System.Security.Claims;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AccountLimitController : ControllerBase
{
    private readonly IAccountLimitService _limitService;
    private readonly ILogger<AccountLimitController> _logger;

    public AccountLimitController(
        IAccountLimitService limitService,
        ILogger<AccountLimitController> logger)
    {
        _limitService = limitService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new account limit
    /// </summary>
    /// <param name="request">Limit creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created limit information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountLimitResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountLimitResponse>> CreateLimit(
        [FromBody] CreateAccountLimitRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var limit = await _limitService.CreateLimitAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetLimit), new { id = limit.Id }, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating limit for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limit by ID
    /// </summary>
    /// <param name="id">Limit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Limit information</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountLimitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountLimitResponse>> GetLimit(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the limit
            if (!await _limitService.UserOwnsLimitAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this limit");
            }

            var limit = await _limitService.GetLimitAsync(id, cancellationToken);
            return Ok(limit);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limit {LimitId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limits for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of limits</returns>
    [HttpGet("account/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AccountLimitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountLimitResponse>>> GetAccountLimits(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var limits = await _limitService.GetAccountLimitsAsync(accountId, cancellationToken);
            return Ok(limits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limits for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user limits</returns>
    [HttpGet("user")]
    [ProducesResponseType(typeof(IEnumerable<AccountLimitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AccountLimitResponse>>> GetUserLimits(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var limits = await _limitService.GetUserLimitsAsync(userId, cancellationToken);
            return Ok(limits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limits for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update account limit
    /// </summary>
    /// <param name="id">Limit ID</param>
    /// <param name="request">Limit update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated limit</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountLimitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountLimitResponse>> UpdateLimit(
        Guid id,
        [FromBody] UpdateAccountLimitRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            
            // Verify user owns the limit
            if (!await _limitService.UserOwnsLimitAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this limit");
            }

            var limit = await _limitService.UpdateLimitAsync(id, request, cancellationToken);
            return Ok(limit);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating limit {LimitId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete account limit
    /// </summary>
    /// <param name="id">Limit ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLimit(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user owns the limit
            if (!await _limitService.UserOwnsLimitAsync(userId, id, cancellationToken))
            {
                return Forbid("You do not have access to this limit");
            }

            var result = await _limitService.DeleteLimitAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Limit not found" });
            }

            return Ok(new { message = "Limit deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting limit {LimitId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Validate transaction against account limits
    /// </summary>
    /// <param name="request">Limit validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(LimitValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LimitValidationResponse>> ValidateLimit(
        [FromBody] ValidateLimitRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _limitService.ValidateLimitAsync(
                request.AccountId, 
                request.Amount, 
                Enum.Parse<LimitType>(request.LimitType), 
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating limit for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Check if transaction amount is within limits
    /// </summary>
    /// <param name="request">Limit check request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Check result</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(LimitCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LimitCheckResponse>> CheckLimit(
        [FromBody] CheckLimitRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _limitService.CheckLimitAsync(
                request.AccountId, 
                Enum.Parse<LimitType>(request.LimitType), 
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking limit for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current usage for account limits
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="limitType">Limit type (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current usage information</returns>
    [HttpGet("usage/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<LimitUsageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LimitUsageResponse>>> GetCurrentUsage(
        Guid accountId,
        [FromQuery] LimitType? limitType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var usage = limitType.HasValue 
                ? new List<LimitUsageResponse> { await _limitService.GetLimitUsageAsync(accountId, limitType.Value, cancellationToken) }
                : await _limitService.GetAllLimitUsageAsync(accountId, cancellationToken);
            return Ok(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current usage for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get remaining limits for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="limitType">Limit type (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Remaining limits information</returns>
    [HttpGet("remaining/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RemainingLimitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RemainingLimitResponse>>> GetRemainingLimits(
        Guid accountId,
        [FromQuery] LimitType? limitType = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var remaining = await _limitService.GetRemainingLimitsAsync(accountId, cancellationToken);
            return Ok(remaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining limits for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update limit usage after transaction
    /// </summary>
    /// <param name="request">Usage update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update result</returns>
    [HttpPost("usage/update")]
    [ProducesResponseType(typeof(UsageUpdateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsageUpdateResponse>> UpdateUsage(
        [FromBody] UpdateUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _limitService.UpdateUsageAsync(
                request.AccountId, 
                request, 
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating usage for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limit alerts for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of limit alerts</returns>
    [HttpGet("alerts/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<LimitAlertResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LimitAlertResponse>>> GetLimitAlerts(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = await _limitService.GetLimitAlertsAsync(accountId, cancellationToken);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limit alerts for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Apply default limits to an account
    /// </summary>
    /// <param name="request">Default limits application request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Applied limits</returns>
    [HttpPost("defaults/apply")]
    [ProducesResponseType(typeof(IEnumerable<AccountLimitResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AccountLimitResponse>>> ApplyDefaultLimits(
        [FromBody] ApplyDefaultLimitsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var limits = await _limitService.ApplyDefaultLimitsAsync(request.AccountId, cancellationToken);
            return CreatedAtAction(nameof(GetAccountLimits), new { accountId = request.AccountId }, limits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying default limits for account {AccountId}", request.AccountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get default limit requirements for account type
    /// </summary>
    /// <param name="accountType">Account type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Default limit requirements</returns>
    [HttpGet("defaults/requirements/{accountType}")]
    [ProducesResponseType(typeof(IEnumerable<DefaultLimitRequirementResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DefaultLimitRequirementResponse>>> GetDefaultLimitRequirements(
        AccountType accountType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requirements = await _limitService.GetDefaultLimitRequirementsAsync(accountType, cancellationToken);
            return Ok(requirements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default limit requirements for account type {AccountType}", accountType);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limit history for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="limitType">Limit type (optional)</param>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Limit history</returns>
    [HttpGet("history/{accountId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<LimitHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LimitHistoryResponse>>> GetLimitHistory(
        Guid accountId,
        [FromQuery] LimitType? limitType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _limitService.GetLimitHistoryAsync(accountId, limitType, page, pageSize, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limit history for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get limit statistics for an account
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="startDate">Start date (optional)</param>
    /// <param name="endDate">End date (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Limit statistics</returns>
    [HttpGet("statistics/{accountId:guid}")]
    [ProducesResponseType(typeof(LimitStatisticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LimitStatisticsResponse>> GetLimitStatistics(
        Guid accountId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statistics = await _limitService.GetLimitStatisticsAsync(accountId, startDate, endDate, cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting limit statistics for account {AccountId}", accountId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }
}
