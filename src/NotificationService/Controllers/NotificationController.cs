using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Send a single notification
    /// </summary>
    /// <param name="request">Notification details</param>
    /// <returns>Notification send result</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SendNotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SendNotificationResponse>> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending notification for user {UserId} of type {Type}", request.UserId, request.Type);

            // Validate user access - users can only send notifications to themselves unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && request.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to send notification for user {TargetUserId}", currentUserId, request.UserId);
                return Forbid("You can only send notifications to yourself");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _notificationService.SendNotificationAsync(request);
            
            _logger.LogInformation("Notification sent successfully with ID {NotificationId}", result.NotificationId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for sending notification");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification for user {UserId}", request.UserId);
            return StatusCode(500, "An error occurred while sending the notification");
        }
    }

    /// <summary>
    /// Send bulk notifications to multiple users
    /// </summary>
    /// <param name="request">Bulk notification details</param>
    /// <returns>Bulk notification send results</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(SendBulkNotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SendBulkNotificationResponse>> SendBulkNotification([FromBody] SendBulkNotificationRequest request)
    {
        try
        {
            _logger.LogInformation("Sending bulk notification to {UserCount} users of type {Type}", request.UserIds.Count, request.Type);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.UserIds.Count == 0)
            {
                return BadRequest("At least one user ID must be provided");
            }

            if (request.UserIds.Count > 1000)
            {
                return BadRequest("Maximum 1000 users allowed per bulk notification");
            }

            var result = await _notificationService.SendBulkNotificationAsync(request);
            
            _logger.LogInformation("Bulk notification completed: {Successful}/{Total} successful", result.SuccessfullyQueued, result.TotalRequested);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for bulk notification");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notification to {UserCount} users", request.UserIds.Count);
            return StatusCode(500, "An error occurred while sending the bulk notification");
        }
    }

    /// <summary>
    /// Get a specific notification by ID
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Notification details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationResponse>> GetNotification(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving notification {NotificationId}", id);

            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", id);
                return NotFound($"Notification with ID {id} not found");
            }

            // Validate user access - users can only view their own notifications unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && notification.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access notification {NotificationId} belonging to user {OwnerId}", 
                    currentUserId, id, notification.UserId);
                return Forbid("You can only access your own notifications");
            }

            return Ok(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification {NotificationId}", id);
            return StatusCode(500, "An error occurred while retrieving the notification");
        }
    }

    /// <summary>
    /// Get notifications with filtering and pagination
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <returns>Paginated list of notifications</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<NotificationResponse>>> GetNotifications([FromQuery] GetNotificationsRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving notifications with filters - Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

            // Validate user access - users can only view their own notifications unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin())
            {
                if (request.UserId.HasValue && request.UserId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access notifications for user {TargetUserId}", currentUserId, request.UserId);
                    return Forbid("You can only access your own notifications");
                }
                
                // Force filter to current user if not admin
                request.UserId = currentUserId;
            }

            if (request.PageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            var result = await _notificationService.GetNotificationsAsync(request);
            
            _logger.LogInformation("Retrieved {Count} notifications (Page {Page} of {TotalPages})", 
                result.Data.Count, result.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting notifications");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications");
            return StatusCode(500, "An error occurred while retrieving notifications");
        }
    }

    /// <summary>
    /// Update notification status
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <param name="request">Status update details</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateNotificationStatus(Guid id, [FromBody] UpdateNotificationStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating notification {NotificationId} status to {Status}", id, request.Status);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _notificationService.UpdateNotificationStatusAsync(id, request);
            if (!success)
            {
                _logger.LogWarning("Notification {NotificationId} not found for status update", id);
                return NotFound($"Notification with ID {id} not found");
            }

            _logger.LogInformation("Notification {NotificationId} status updated successfully", id);
            return Ok(new { message = "Notification status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating notification status");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification {NotificationId} status", id);
            return StatusCode(500, "An error occurred while updating the notification status");
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Success result</returns>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> MarkNotificationAsRead(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", id, currentUserId);

            var request = new MarkNotificationReadRequest
            {
                NotificationId = id,
                UserId = currentUserId
            };

            var success = await _notificationService.MarkNotificationAsReadAsync(request);
            if (!success)
            {
                _logger.LogWarning("Notification {NotificationId} not found or not accessible for user {UserId}", id, currentUserId);
                return NotFound($"Notification with ID {id} not found or not accessible");
            }

            _logger.LogInformation("Notification {NotificationId} marked as read successfully", id);
            return Ok(new { message = "Notification marked as read successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, "An error occurred while marking the notification as read");
        }
    }

    /// <summary>
    /// Cancel a pending notification
    /// </summary>
    /// <param name="id">Notification ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelNotification(Guid id)
    {
        try
        {
            _logger.LogInformation("Cancelling notification {NotificationId}", id);

            // First check if notification exists and user has access
            var notification = await _notificationService.GetNotificationAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for cancellation", id);
                return NotFound($"Notification with ID {id} not found");
            }

            // Validate user access - users can only cancel their own notifications unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && notification.UserId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to cancel notification {NotificationId} belonging to user {OwnerId}", 
                    currentUserId, id, notification.UserId);
                return Forbid("You can only cancel your own notifications");
            }

            var success = await _notificationService.CancelNotificationAsync(id);
            if (!success)
            {
                _logger.LogWarning("Failed to cancel notification {NotificationId}", id);
                return BadRequest("Notification cannot be cancelled (may already be sent or processed)");
            }

            _logger.LogInformation("Notification {NotificationId} cancelled successfully", id);
            return Ok(new { message = "Notification cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling notification {NotificationId}", id);
            return StatusCode(500, "An error occurred while cancelling the notification");
        }
    }

    /// <summary>
    /// Get notification statistics
    /// </summary>
    /// <param name="userId">Optional user ID filter (admin only)</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>Notification statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(NotificationStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationStatsResponse>> GetNotificationStats(
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Retrieving notification statistics");

            // Validate user access - users can only view their own stats unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin())
            {
                if (userId.HasValue && userId.Value != currentUserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access stats for user {TargetUserId}", currentUserId, userId);
                    return Forbid("You can only access your own notification statistics");
                }
                
                // Force filter to current user if not admin
                userId = currentUserId;
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var stats = await _notificationService.GetNotificationStatsAsync(userId, fromDate, toDate);
            
            _logger.LogInformation("Retrieved notification statistics successfully");
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification statistics");
            return StatusCode(500, "An error occurred while retrieving notification statistics");
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
