using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/preferences")]
[Authorize]
public class UserPreferenceController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserPreferenceController> _logger;

    public UserPreferenceController(
        INotificationService notificationService,
        ILogger<UserPreferenceController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get user notification preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="notificationType">Optional notification type filter</param>
    /// <param name="channel">Optional channel filter</param>
    /// <returns>User notification preferences</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserNotificationPreferenceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<UserNotificationPreferenceResponse>>> GetUserPreferences(
        Guid userId,
        [FromQuery] string? notificationType = null,
        [FromQuery] string? channel = null)
    {
        try
        {
            _logger.LogInformation("Retrieving notification preferences for user {UserId}", userId);

            // Validate user access - users can only access their own preferences unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access preferences for user {TargetUserId}", 
                    currentUserId, userId);
                return Forbid("You can only access your own notification preferences");
            }

            // Validate notification type filter if provided
            if (!string.IsNullOrEmpty(notificationType))
            {
                var validTypes = new[] { "Email", "SMS", "Push", "InApp", "System", "Marketing", "Security", "Transaction" };
                if (!validTypes.Contains(notificationType))
                {
                    return BadRequest($"Invalid notification type. Valid types are: {string.Join(", ", validTypes)}");
                }
            }

            // Validate channel filter if provided
            if (!string.IsNullOrEmpty(channel))
            {
                var validChannels = new[] { "Email", "SMS", "Push", "InApp" };
                if (!validChannels.Contains(channel))
                {
                    return BadRequest($"Invalid channel. Valid channels are: {string.Join(", ", validChannels)}");
                }
            }

            var request = new GetUserNotificationPreferencesRequest
            {
                UserId = userId,
                NotificationType = notificationType,
                Channel = channel
            };

            var preferences = await _notificationService.GetUserPreferencesAsync(request);
            
            _logger.LogInformation("Retrieved {Count} notification preferences for user {UserId}", 
                preferences.Data.Count, userId);
            
            return Ok(preferences);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting user preferences");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification preferences for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving notification preferences");
        }
    }

    /// <summary>
    /// Update user notification preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Preference update details</param>
    /// <returns>Success result</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateUserPreferences(
        Guid userId, 
        [FromBody] List<UserNotificationPreferenceRequest> request)
    {
        try
        {
            _logger.LogInformation("Updating notification preferences for user {UserId}", userId);

            // Validate user access - users can only update their own preferences unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update preferences for user {TargetUserId}", 
                    currentUserId, userId);
                return Forbid("You can only update your own notification preferences");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request == null || request.Count == 0)
            {
                return BadRequest("At least one preference must be provided");
            }

            // Validate each preference in the request
            foreach (var preference in request)
            {
                // Validate notification type
                var validTypes = new[] { "Email", "SMS", "Push", "InApp", "System", "Marketing", "Security", "Transaction" };
                if (!validTypes.Contains(preference.NotificationType))
                {
                    return BadRequest($"Invalid notification type '{preference.NotificationType}'. Valid types are: {string.Join(", ", validTypes)}");
                }

                // Validate channel
                var validChannels = new[] { "Email", "SMS", "Push", "InApp" };
                if (!validChannels.Contains(preference.Channel))
                {
                    return BadRequest($"Invalid channel '{preference.Channel}'. Valid channels are: {string.Join(", ", validChannels)}");
                }

                // Validate frequency if provided
                if (!string.IsNullOrEmpty(preference.Frequency))
                {
                    var validFrequencies = new[] { "Immediate", "Hourly", "Daily", "Weekly", "Never" };
                    if (!validFrequencies.Contains(preference.Frequency))
                    {
                        return BadRequest($"Invalid frequency '{preference.Frequency}'. Valid frequencies are: {string.Join(", ", validFrequencies)}");
                    }
                }

                // Validate timezone if provided
                if (!string.IsNullOrEmpty(preference.TimeZone))
                {
                    try
                    {
                        TimeZoneInfo.FindSystemTimeZoneById(preference.TimeZone);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        return BadRequest($"Invalid timezone '{preference.TimeZone}'");
                    }
                }
            }

            var updateRequest = new UpdateUserNotificationPreferencesRequest
            {
                UserId = userId,
                Preferences = request
            };

            var success = await _notificationService.UpdateUserPreferencesAsync(updateRequest);
            if (!success)
            {
                _logger.LogWarning("Failed to update notification preferences for user {UserId}", userId);
                return BadRequest("Failed to update notification preferences");
            }

            _logger.LogInformation("Notification preferences updated successfully for user {UserId}", userId);
            return Ok(new { message = "Notification preferences updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating user preferences");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
            return StatusCode(500, "An error occurred while updating notification preferences");
        }
    }

    /// <summary>
    /// Reset user notification preferences to default values
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success result</returns>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ResetUserPreferencesToDefault(Guid userId)
    {
        try
        {
            _logger.LogInformation("Resetting notification preferences to default for user {UserId}", userId);

            // Validate user access - users can only reset their own preferences unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to reset preferences for user {TargetUserId}", 
                    currentUserId, userId);
                return Forbid("You can only reset your own notification preferences");
            }

            var success = await _notificationService.ResetUserPreferencesToDefaultAsync(userId);
            if (!success)
            {
                _logger.LogWarning("Failed to reset notification preferences for user {UserId}", userId);
                return BadRequest("Failed to reset notification preferences");
            }

            _logger.LogInformation("Notification preferences reset to default successfully for user {UserId}", userId);
            return Ok(new { message = "Notification preferences reset to default values successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting notification preferences for user {UserId}", userId);
            return StatusCode(500, "An error occurred while resetting notification preferences");
        }
    }

    /// <summary>
    /// Get user notification preferences summary
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Preferences summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetUserPreferencesSummary(Guid userId)
    {
        try
        {
            _logger.LogInformation("Retrieving notification preferences summary for user {UserId}", userId);

            // Validate user access - users can only access their own preferences unless admin
            var currentUserId = GetCurrentUserId();
            if (!IsAdmin() && userId != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access preferences summary for user {TargetUserId}", 
                    currentUserId, userId);
                return Forbid("You can only access your own notification preferences");
            }

            var request = new GetUserNotificationPreferencesRequest
            {
                UserId = userId
            };

            var preferences = await _notificationService.GetUserPreferencesAsync(request);
            
            // Create summary from preferences
            var summary = new
            {
                UserId = userId,
                TotalPreferences = preferences.Data.Count,
                EnabledPreferences = preferences.Data.Count(p => p.IsEnabled),
                DisabledPreferences = preferences.Data.Count(p => !p.IsEnabled),
                PreferencesByChannel = preferences.Data
                    .GroupBy(p => p.Channel)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Total = g.Count(),
                        Enabled = g.Count(p => p.IsEnabled),
                        Disabled = g.Count(p => !p.IsEnabled)
                    }),
                PreferencesByType = preferences.Data
                    .GroupBy(p => p.NotificationType)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Total = g.Count(),
                        Enabled = g.Count(p => p.IsEnabled),
                        Disabled = g.Count(p => !p.IsEnabled)
                    }),
                PreferredTime = preferences.Data
                    .Where(p => p.PreferredTime.HasValue)
                    .Select(p => p.PreferredTime)
                    .FirstOrDefault(),
                TimeZone = preferences.Data
                    .Where(p => !string.IsNullOrEmpty(p.TimeZone))
                    .Select(p => p.TimeZone)
                    .FirstOrDefault(),
                LastUpdated = preferences.Data
                    .Max(p => p.UpdatedAt)
            };
            
            _logger.LogInformation("Retrieved notification preferences summary for user {UserId}", userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification preferences summary for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving notification preferences summary");
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
