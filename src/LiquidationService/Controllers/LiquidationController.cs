using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LiquidationService.Models.Requests;
using LiquidationService.Models.Responses;
using LiquidationService.Services.Interfaces;
using System.Security.Claims;

namespace LiquidationService.Controllers;

/// <summary>
/// Controller for managing liquidation requests and operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiquidationController : ControllerBase
{
    private readonly ILiquidationService _liquidationService;
    private readonly ILogger<LiquidationController> _logger;

    public LiquidationController(
        ILiquidationService liquidationService,
        ILogger<LiquidationController> logger)
    {
        _liquidationService = liquidationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new liquidation request
    /// </summary>
    /// <param name="request">Liquidation request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created liquidation request</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> CreateLiquidationRequest(
        [FromBody] CreateLiquidationRequestModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Creating liquidation request for user {UserId} - Asset: {AssetSymbol}, Amount: {Amount}", 
                currentUserId, request.AssetSymbol, request.AssetAmount);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _liquidationService.CreateLiquidationRequestAsync(currentUserId, request, cancellationToken);
            
            _logger.LogInformation("Liquidation request created successfully with ID {RequestId}", result.Id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for liquidation creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating liquidation request");
            return StatusCode(500, "An error occurred while creating the liquidation request");
        }
    }

    /// <summary>
    /// Get a specific liquidation request by ID
    /// </summary>
    /// <param name="id">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation request details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> GetLiquidationRequest(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrieving liquidation request {RequestId} for user {UserId}", id, currentUserId);

            var result = await _liquidationService.GetLiquidationRequestAsync(id, currentUserId, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("Liquidation request {RequestId} not found", id);
                return NotFound($"Liquidation request with ID {id} not found");
            }

            // Additional validation - users can only view their own requests unless admin
            if (!IsAdmin() && result.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access liquidation request {RequestId} belonging to user {OwnerId}", 
                    currentUserId, id, result.UserId);
                return Forbid("You can only access your own liquidation requests");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liquidation request {RequestId}", id);
            return StatusCode(500, "An error occurred while retrieving the liquidation request");
        }
    }

    /// <summary>
    /// Get liquidation requests with filtering and pagination
    /// </summary>
    /// <param name="filter">Filter and pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of liquidation requests</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<LiquidationRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<LiquidationRequestResponse>>> GetLiquidationRequests(
        [FromQuery] LiquidationRequestFilterModel filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrieving liquidation requests for user {UserId} - Page: {Page}, PageSize: {PageSize}", 
                currentUserId, filter.Page, filter.PageSize);

            // Validate user access - users can only view their own requests unless admin
            if (!IsAdmin())
            {
                if (filter.UserId != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access liquidation requests for user {TargetUserId}", 
                        currentUserId, filter.UserId);
                    return Forbid("You can only access your own liquidation requests");
                }
            }

            if (filter.PageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            var result = await _liquidationService.GetLiquidationRequestsAsync(filter, cancellationToken);
            
            _logger.LogInformation("Retrieved {Count} liquidation requests (Page {Page} of {TotalPages})", 
                result.Items.Count(), filter.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting liquidation requests");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liquidation requests");
            return StatusCode(500, "An error occurred while retrieving liquidation requests");
        }
    }

    /// <summary>
    /// Update liquidation request status
    /// </summary>
    /// <param name="id">Liquidation request ID</param>
    /// <param name="request">Status update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> UpdateLiquidationStatus(
        Guid id,
        [FromBody] UpdateLiquidationStatusModel request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating liquidation request {RequestId} status to {Status}", id, request.Status);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _liquidationService.UpdateLiquidationStatusAsync(id, request, cancellationToken);
            
            _logger.LogInformation("Liquidation request {RequestId} status updated successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating liquidation status");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating liquidation request {RequestId} status", id);
            return StatusCode(500, "An error occurred while updating the liquidation status");
        }
    }

    /// <summary>
    /// Cancel a pending liquidation request
    /// </summary>
    /// <param name="id">Liquidation request ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> CancelLiquidationRequest(
        Guid id,
        [FromQuery] string reason = "Cancelled by user",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Cancelling liquidation request {RequestId} for user {UserId}", id, currentUserId);

            // First check if request exists and user has access
            var liquidationRequest = await _liquidationService.GetLiquidationRequestAsync(id, currentUserId, cancellationToken);
            if (liquidationRequest == null)
            {
                _logger.LogWarning("Liquidation request {RequestId} not found for cancellation", id);
                return NotFound($"Liquidation request with ID {id} not found");
            }

            // Validate user access - users can only cancel their own requests unless admin
            if (!IsAdmin() && liquidationRequest.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to cancel liquidation request {RequestId} belonging to user {OwnerId}", 
                    currentUserId, id, liquidationRequest.UserId);
                return Forbid("You can only cancel your own liquidation requests");
            }

            var result = await _liquidationService.CancelLiquidationRequestAsync(id, currentUserId, reason, cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} cancelled successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling liquidation request {RequestId}", id);
            return StatusCode(500, "An error occurred while cancelling the liquidation request");
        }
    }

    /// <summary>
    /// Process a liquidation request through the complete workflow
    /// </summary>
    /// <param name="id">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processed liquidation request</returns>
    [HttpPost("{id:guid}/process")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> ProcessLiquidation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing liquidation request {RequestId}", id);

            var result = await _liquidationService.ProcessLiquidationAsync(id, cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} processed successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for processing liquidation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing liquidation request {RequestId}", id);
            return StatusCode(500, "An error occurred while processing the liquidation request");
        }
    }

    /// <summary>
    /// Estimate liquidation output amount and fees
    /// </summary>
    /// <param name="assetSymbol">Asset to liquidate</param>
    /// <param name="assetAmount">Amount to liquidate</param>
    /// <param name="outputSymbol">Desired output currency</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation estimation</returns>
    [HttpPost("estimate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> EstimateLiquidation(
        [FromQuery] string assetSymbol,
        [FromQuery] decimal assetAmount,
        [FromQuery] string outputSymbol,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Estimating liquidation for {AssetSymbol} {AssetAmount} to {OutputSymbol}", 
                assetSymbol, assetAmount, outputSymbol);

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

            var result = await _liquidationService.EstimateLiquidationAsync(assetSymbol, assetAmount, outputSymbol, cancellationToken);
            
            _logger.LogInformation("Liquidation estimation completed successfully");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for liquidation estimation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating liquidation");
            return StatusCode(500, "An error occurred while estimating the liquidation");
        }
    }

    /// <summary>
    /// Retry a failed liquidation request
    /// </summary>
    /// <param name="id">Liquidation request ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated liquidation request</returns>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(typeof(LiquidationRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LiquidationRequestResponse>> RetryLiquidation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Retrying liquidation request {RequestId} for user {UserId}", id, currentUserId);

            // First check if request exists and user has access
            var liquidationRequest = await _liquidationService.GetLiquidationRequestAsync(id, currentUserId, cancellationToken);
            if (liquidationRequest == null)
            {
                _logger.LogWarning("Liquidation request {RequestId} not found for retry", id);
                return NotFound($"Liquidation request with ID {id} not found");
            }

            // Validate user access - users can only retry their own requests unless admin
            if (!IsAdmin() && liquidationRequest.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to retry liquidation request {RequestId} belonging to user {OwnerId}", 
                    currentUserId, id, liquidationRequest.UserId);
                return Forbid("You can only retry your own liquidation requests");
            }

            var result = await _liquidationService.RetryLiquidationAsync(id, cancellationToken);

            _logger.LogInformation("Liquidation request {RequestId} retried successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying liquidation request {RequestId}", id);
            return StatusCode(500, "An error occurred while retrying the liquidation request");
        }
    }

    /// <summary>
    /// Get liquidation statistics for a user
    /// </summary>
    /// <param name="userId">Optional user ID filter (admin only)</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liquidation statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetLiquidationStatistics(
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving liquidation statistics");

            // Validate user access - users can only view their own stats unless admin
            var currentUserId = GetCurrentUserId();
            var targetUserId = currentUserId; // Default to current user

            if (!IsAdmin())
            {
                if (userId.HasValue && userId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access stats for user {TargetUserId}", 
                        currentUserId, userId);
                    return Forbid("You can only access your own liquidation statistics");
                }
            }
            else if (userId.HasValue)
            {
                // Admin can specify any user ID
                targetUserId = userId.Value;
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _liquidationService.GetLiquidationStatisticsAsync(targetUserId, fromDate, toDate, cancellationToken);
            
            _logger.LogInformation("Retrieved liquidation statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liquidation statistics");
            return StatusCode(500, "An error occurred while retrieving liquidation statistics");
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
