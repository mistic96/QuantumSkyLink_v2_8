using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Services.Interfaces;
using System.Security.Claims;

namespace PaymentGatewayService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Process a new payment
    /// </summary>
    /// <param name="request">Payment processing details</param>
    /// <returns>Payment processing result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Processing payment for user {UserId} amount {Amount} {Currency}", 
                request.UserId, request.Amount, request.Currency);

            // Validate user access - users can only process payments for themselves unless admin
            var currentUserId = GetCurrentUserId();

            // request.UserId is a string in the Requests DTO; parse and validate against the current user id
            Guid? requestUserId = null;
            if (!string.IsNullOrWhiteSpace(request.UserId) && Guid.TryParse(request.UserId, out var parsedRequestUserId))
            {
                requestUserId = parsedRequestUserId;
            }

            if (!IsAdmin() && (!requestUserId.HasValue || requestUserId.Value != currentUserId))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to process payment for user {TargetUserId}", 
                    currentUserId, request.UserId);
                return Forbid("You can only process payments for yourself");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _paymentService.ProcessPaymentAsync(request);
            
            _logger.LogInformation("Payment processed successfully with ID {PaymentId}", result.Id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for payment processing");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while processing the payment");
        }
    }

    /// <summary>
    /// Get a specific payment by ID
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> GetPayment(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving payment {PaymentId}", id);

            var currentUserId = GetCurrentUserId();
            var payment = await _paymentService.GetPaymentAsync(id, currentUserId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", id);
                return NotFound($"Payment with ID {id} not found");
            }

            // Additional validation - users can only view their own payments unless admin
            if (!IsAdmin() && payment.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access payment {PaymentId} belonging to user {OwnerId}", 
                    currentUserId, id, payment.UserId);
                return Forbid("You can only access your own payments");
            }

            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", id);
            return StatusCode(500, "An error occurred while retrieving the payment");
        }
    }

    /// <summary>
    /// Get payments with filtering and pagination
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <returns>Paginated list of payments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedPaymentResponse>> GetPayments([FromQuery] GetPaymentsRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving payments with filters - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);

            // Validate user access - users can only view their own payments unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin())
            {
                if (request.UserId != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access payments for user {TargetUserId}", 
                        currentUserId, request.UserId);
                    return Forbid("You can only access your own payments");
                }
            }

            if (request.PageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            var result = await _paymentService.GetPaymentsAsync(request);
            
            _logger.LogInformation("Retrieved {Count} payments (Page {Page} of {TotalPages})", 
                result.Payments.Count(), result.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting payments");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments");
            return StatusCode(500, "An error occurred while retrieving payments");
        }
    }

    /// <summary>
    /// Update payment status
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="request">Status update details</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating payment {PaymentId} status to {Status}", id, request.Status);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set the payment ID from the route
            request.PaymentId = id;

            var result = await _paymentService.UpdatePaymentStatusAsync(request);
            
            _logger.LogInformation("Payment {PaymentId} status updated successfully", id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating payment status");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment {PaymentId} status", id);
            return StatusCode(500, "An error occurred while updating the payment status");
        }
    }

    /// <summary>
    /// Cancel a payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> CancelPayment(Guid id, [FromQuery] string reason = "Cancelled by user")
    {
        try
        {
            _logger.LogInformation("Cancelling payment {PaymentId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment exists and user has access
            var payment = await _paymentService.GetPaymentAsync(id, currentUserId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for cancellation", id);
                return NotFound($"Payment with ID {id} not found");
            }

            // Validate user access - users can only cancel their own payments unless admin
            if (!IsAdmin() && payment.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to cancel payment {PaymentId} belonging to user {OwnerId}", 
                    currentUserId, id, payment.UserId);
                return Forbid("You can only cancel your own payments");
            }

            var result = await _paymentService.CancelPaymentAsync(id, currentUserId, reason);

            _logger.LogInformation("Payment {PaymentId} cancelled successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment {PaymentId}", id);
            return StatusCode(500, "An error occurred while cancelling the payment");
        }
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Retry result</returns>
    [HttpPost("{id:guid}/retry")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> RetryPayment(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrying payment {PaymentId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment exists and user has access
            var payment = await _paymentService.GetPaymentAsync(id, currentUserId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for retry", id);
                return NotFound($"Payment with ID {id} not found");
            }

            // Validate user access - users can only retry their own payments unless admin
            if (!IsAdmin() && payment.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to retry payment {PaymentId} belonging to user {OwnerId}", 
                    currentUserId, id, payment.UserId);
                return Forbid("You can only retry your own payments");
            }

            var result = await _paymentService.RetryPaymentAsync(id, currentUserId);

            _logger.LogInformation("Payment {PaymentId} retried successfully", id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment {PaymentId}", id);
            return StatusCode(500, "An error occurred while retrying the payment");
        }
    }

    /// <summary>
    /// Get payment history
    /// </summary>
    /// <param name="id">Payment ID</param>
    /// <returns>Payment history</returns>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(PaymentHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentHistoryResponse>> GetPaymentHistory(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving payment history for {PaymentId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment exists and user has access
            var payment = await _paymentService.GetPaymentAsync(id, currentUserId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found for history retrieval", id);
                return NotFound($"Payment with ID {id} not found");
            }

            // Validate user access - users can only view their own payment history unless admin
            if (!IsAdmin() && payment.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access payment history for {PaymentId} belonging to user {OwnerId}", 
                    currentUserId, id, payment.UserId);
                return Forbid("You can only access your own payment history");
            }

            var history = await _paymentService.GetPaymentHistoryAsync(id, currentUserId);
            
            _logger.LogInformation("Retrieved payment history for payment {PaymentId}", id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment history for {PaymentId}", id);
            return StatusCode(500, "An error occurred while retrieving payment history");
        }
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    /// <param name="userId">Optional user ID filter (admin only)</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>Payment statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(PaymentStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentStatisticsResponse>> GetPaymentStatistics(
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Retrieving payment statistics");

            // Validate user access - users can only view their own stats unless admin
            var currentUserId = GetCurrentUserId();
            var targetUserId = currentUserId; // Default to current user

            if (!IsAdmin())
            {
                if (userId.HasValue && userId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access stats for user {TargetUserId}", 
                        currentUserId, userId);
                    return Forbid("You can only access your own payment statistics");
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

            var stats = await _paymentService.GetPaymentStatisticsAsync(targetUserId, fromDate, toDate);
            
            _logger.LogInformation("Retrieved payment statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment statistics");
            return StatusCode(500, "An error occurred while retrieving payment statistics");
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
