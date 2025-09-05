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
public class PaymentMethodController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ILogger<PaymentMethodController> _logger;

    public PaymentMethodController(
        IPaymentMethodService paymentMethodService,
        ILogger<PaymentMethodController> logger)
    {
        _paymentMethodService = paymentMethodService;
        _logger = logger;
    }

    /// <summary>
    /// Add a new payment method for the current user
    /// </summary>
    /// <param name="request">Payment method details</param>
    /// <returns>Created payment method</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> AddPaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Adding payment method for user {UserId}", currentUserId);

            // Validate user access - users can only add payment methods for themselves unless admin
            if (!IsAdmin() && request.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to add payment method for user {TargetUserId}", 
                    currentUserId, request.UserId);
                return Forbid("You can only add payment methods for yourself");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(request);
            
            _logger.LogInformation("Payment method {PaymentMethodId} added successfully for user {UserId}", 
                paymentMethod.Id, currentUserId);
            return CreatedAtAction(nameof(GetPaymentMethod), new { id = paymentMethod.Id }, paymentMethod);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for adding payment method");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding payment method for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while adding the payment method");
        }
    }

    /// <summary>
    /// Get all payment methods for the current user
    /// </summary>
    /// <param name="userId">User ID (admin only, defaults to current user)</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>List of payment methods</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PaymentMethodResponse>>> GetPaymentMethods(
        [FromQuery] Guid? userId = null, 
        [FromQuery] bool? isActive = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = currentUserId; // Default to current user

            // Validate user access - users can only view their own payment methods unless admin
            if (!IsAdmin())
            {
                if (userId.HasValue && userId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access payment methods for user {TargetUserId}", 
                        currentUserId, userId);
                    return Forbid("You can only access your own payment methods");
                }
            }
            else if (userId.HasValue)
            {
                // Admin can specify any user ID
                targetUserId = userId.Value;
            }

            _logger.LogInformation("Retrieving payment methods for user {UserId}", targetUserId);

            var paymentMethods = await _paymentMethodService.GetUserPaymentMethodsAsync(targetUserId, isActive);
            
            _logger.LogInformation("Retrieved {Count} payment methods for user {UserId}", 
                paymentMethods.Count(), targetUserId);
            return Ok(paymentMethods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving payment methods");
        }
    }

    /// <summary>
    /// Get a specific payment method by ID
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Payment method details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> GetPaymentMethod(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving payment method {PaymentMethodId}", id);

            var currentUserId = GetCurrentUserId();
            var paymentMethod = await _paymentMethodService.GetPaymentMethodAsync(id, currentUserId);
            
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found", id);
                return NotFound($"Payment method with ID {id} not found");
            }

            // Additional validation - users can only view their own payment methods unless admin
            if (!IsAdmin() && paymentMethod.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access payment method {PaymentMethodId} belonging to user {OwnerId}", 
                    currentUserId, id, paymentMethod.UserId);
                return Forbid("You can only access your own payment methods");
            }

            return Ok(paymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment method {PaymentMethodId}", id);
            return StatusCode(500, "An error occurred while retrieving the payment method");
        }
    }

    /// <summary>
    /// Update a payment method
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <param name="request">Update details</param>
    /// <returns>Updated payment method</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> UpdatePaymentMethod(Guid id, [FromBody] UpdatePaymentMethodRequest request)
    {
        try
        {
            _logger.LogInformation("Updating payment method {PaymentMethodId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment method exists and user has access
            var existingPaymentMethod = await _paymentMethodService.GetPaymentMethodAsync(id, currentUserId);
            if (existingPaymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for update", id);
                return NotFound($"Payment method with ID {id} not found");
            }

            // Validate user access - users can only update their own payment methods unless admin
            if (!IsAdmin() && existingPaymentMethod.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update payment method {PaymentMethodId} belonging to user {OwnerId}", 
                    currentUserId, id, existingPaymentMethod.UserId);
                return Forbid("You can only update your own payment methods");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Set the payment method ID and user ID from the route and current user
            request.MethodId = id;
            request.UserId = currentUserId;

            var updatedPaymentMethod = await _paymentMethodService.UpdatePaymentMethodAsync(request);
            
            _logger.LogInformation("Payment method {PaymentMethodId} updated successfully", id);
            return Ok(updatedPaymentMethod);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating payment method");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method {PaymentMethodId}", id);
            return StatusCode(500, "An error occurred while updating the payment method");
        }
    }

    /// <summary>
    /// Remove a payment method
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> RemovePaymentMethod(Guid id)
    {
        try
        {
            _logger.LogInformation("Removing payment method {PaymentMethodId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment method exists and user has access
            var paymentMethod = await _paymentMethodService.GetPaymentMethodAsync(id, currentUserId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for removal", id);
                return NotFound($"Payment method with ID {id} not found");
            }

            // Validate user access - users can only remove their own payment methods unless admin
            if (!IsAdmin() && paymentMethod.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to remove payment method {PaymentMethodId} belonging to user {OwnerId}", 
                    currentUserId, id, paymentMethod.UserId);
                return Forbid("You can only remove your own payment methods");
            }

            var success = await _paymentMethodService.DeletePaymentMethodAsync(id, currentUserId);
            if (!success)
            {
                _logger.LogWarning("Failed to remove payment method {PaymentMethodId}", id);
                return BadRequest("Payment method cannot be removed (may be in use or already removed)");
            }

            _logger.LogInformation("Payment method {PaymentMethodId} removed successfully", id);
            return Ok(new { message = "Payment method removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing payment method {PaymentMethodId}", id);
            return StatusCode(500, "An error occurred while removing the payment method");
        }
    }

    /// <summary>
    /// Verify a payment method
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <param name="verificationData">Verification data from the gateway</param>
    /// <returns>Verification result</returns>
    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> VerifyPaymentMethod(
        Guid id, 
        [FromBody] Dictionary<string, object>? verificationData = null)
    {
        try
        {
            _logger.LogInformation("Verifying payment method {PaymentMethodId}", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment method exists and user has access
            var paymentMethod = await _paymentMethodService.GetPaymentMethodAsync(id, currentUserId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for verification", id);
                return NotFound($"Payment method with ID {id} not found");
            }

            // Validate user access - users can only verify their own payment methods unless admin
            if (!IsAdmin() && paymentMethod.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to verify payment method {PaymentMethodId} belonging to user {OwnerId}", 
                    currentUserId, id, paymentMethod.UserId);
                return Forbid("You can only verify your own payment methods");
            }

            var verifiedPaymentMethod = await _paymentMethodService.VerifyPaymentMethodAsync(
                id, currentUserId, verificationData ?? new Dictionary<string, object>());
            
            _logger.LogInformation("Payment method {PaymentMethodId} verification completed", id);
            return Ok(verifiedPaymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment method {PaymentMethodId}", id);
            return StatusCode(500, "An error occurred while verifying the payment method");
        }
    }

    /// <summary>
    /// Set a payment method as default
    /// </summary>
    /// <param name="id">Payment method ID</param>
    /// <returns>Updated payment method</returns>
    [HttpPut("{id:guid}/default")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> SetDefaultPaymentMethod(Guid id)
    {
        try
        {
            _logger.LogInformation("Setting payment method {PaymentMethodId} as default", id);

            var currentUserId = GetCurrentUserId();

            // First check if payment method exists and user has access
            var paymentMethod = await _paymentMethodService.GetPaymentMethodAsync(id, currentUserId);
            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method {PaymentMethodId} not found for setting as default", id);
                return NotFound($"Payment method with ID {id} not found");
            }

            // Validate user access - users can only set their own payment methods as default unless admin
            if (!IsAdmin() && paymentMethod.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to set payment method {PaymentMethodId} as default for user {OwnerId}", 
                    currentUserId, id, paymentMethod.UserId);
                return Forbid("You can only set your own payment methods as default");
            }

            var defaultPaymentMethod = await _paymentMethodService.SetDefaultPaymentMethodAsync(id, currentUserId);
            
            _logger.LogInformation("Payment method {PaymentMethodId} set as default successfully", id);
            return Ok(defaultPaymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting payment method {PaymentMethodId} as default", id);
            return StatusCode(500, "An error occurred while setting the payment method as default");
        }
    }

    /// <summary>
    /// Get the default payment method for the current user
    /// </summary>
    /// <param name="userId">User ID (admin only, defaults to current user)</param>
    /// <returns>Default payment method</returns>
    [HttpGet("default")]
    [ProducesResponseType(typeof(PaymentMethodResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentMethodResponse>> GetDefaultPaymentMethod([FromQuery] Guid? userId = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var targetUserId = currentUserId; // Default to current user

            // Validate user access - users can only view their own default payment method unless admin
            if (!IsAdmin())
            {
                if (userId.HasValue && userId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access default payment method for user {TargetUserId}", 
                        currentUserId, userId);
                    return Forbid("You can only access your own default payment method");
                }
            }
            else if (userId.HasValue)
            {
                // Admin can specify any user ID
                targetUserId = userId.Value;
            }

            _logger.LogInformation("Retrieving default payment method for user {UserId}", targetUserId);

            var defaultPaymentMethod = await _paymentMethodService.GetDefaultPaymentMethodAsync(targetUserId);
            if (defaultPaymentMethod == null)
            {
                _logger.LogWarning("No default payment method found for user {UserId}", targetUserId);
                return NotFound("No default payment method found");
            }

            return Ok(defaultPaymentMethod);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default payment method for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving the default payment method");
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
