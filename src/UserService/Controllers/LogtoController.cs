using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Models.Logto;
using UserService.Services.Interfaces;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogtoController : ControllerBase
{
    private readonly ILogtoUserService _logtoUserService;
    private readonly ILogger<LogtoController> _logger;

    public LogtoController(ILogtoUserService logtoUserService, ILogger<LogtoController> logger)
    {
        _logtoUserService = logtoUserService;
        _logger = logger;
    }

    /// <summary>
    /// Create user in Logto
    /// </summary>
    /// <param name="request">Logto user creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created Logto user information</returns>
    [HttpPost("users")]
    [ProducesResponseType(typeof(LogtoUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LogtoUserResponse>> CreateLogtoUser(
        [FromBody] CreateLogtoUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert CreateLogtoUserRequest to RegisterUserRequest
            var registerRequest = new RegisterUserRequest
            {
                Email = request.Email,
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? "",
                PhoneNumber = request.PhoneNumber,
                Country = null,
                City = null,
                PreferredLanguage = request.Locale,
                PreferredCurrency = "USD"
            };
            
            var logtoUser = await _logtoUserService.CreateUserAsync(registerRequest, cancellationToken);
            return CreatedAtAction(nameof(GetLogtoUser), new { id = logtoUser.Data?.Id }, logtoUser.Data);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Logto user");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get Logto user by ID
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logto user information</returns>
    [HttpGet("users/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(LogtoUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LogtoUserResponse>> GetLogtoUser(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logtoUser = await _logtoUserService.GetUserAsync(id, cancellationToken);
            return Ok(logtoUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Logto user {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update Logto user
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="request">Updated Logto user information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated Logto user information</returns>
    [HttpPut("users/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(LogtoUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LogtoUserResponse>> UpdateLogtoUser(
        string id,
        [FromBody] UpdateLogtoUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert UpdateLogtoUserRequest to LogtoUserRequest
            var logtoUserRequest = new LogtoUserRequest
            {
                Username = request.Username ?? "",
                PrimaryEmail = null,
                Name = $"{request.FirstName} {request.LastName}".Trim(),
                Avatar = request.Avatar,
                CustomData = new LogtoCustomData
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PreferredLanguage = request.Locale
                }
            };
            
            var logtoUser = await _logtoUserService.UpdateUserAsync(id, logtoUserRequest, cancellationToken);
            return Ok(logtoUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Logto user {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete Logto user
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete("users/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteLogtoUser(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _logtoUserService.DeleteUserAsync(id, cancellationToken);
            if (!result.Success || !result.Data)
            {
                return NotFound(new { message = "Logto user not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Logto user {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Sync user from Logto to local database
    /// </summary>
    /// <param name="logtoUserId">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Synced user information</returns>
    [HttpPost("sync/{logtoUserId}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> SyncUserFromLogto(
        string logtoUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _logtoUserService.SyncUserFromLogtoAsync(logtoUserId, cancellationToken);
            return Ok(user);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user from Logto {LogtoUserId}", logtoUserId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get Logto user profile
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logto user profile</returns>
    [HttpGet("users/{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(LogtoUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LogtoUserProfileResponse>> GetLogtoUserProfile(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _logtoUserService.GetUserProfileAsync(id, cancellationToken);
            return Ok(profile);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Logto user profile {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update Logto user profile
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="request">Updated profile information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated profile information</returns>
    [HttpPut("users/{id}/profile")]
    [Authorize]
    [ProducesResponseType(typeof(LogtoUserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LogtoUserProfileResponse>> UpdateLogtoUserProfile(
        string id,
        [FromBody] UpdateLogtoUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert UpdateLogtoUserProfileRequest to LogtoUserRequest
            var logtoUserRequest = new LogtoUserRequest
            {
                Username = "",
                Name = "",
                Avatar = request.Avatar,
                CustomData = new LogtoCustomData
                {
                    Country = request.Country,
                    City = request.City,
                    PreferredLanguage = request.Locale,
                    PreferredCurrency = request.PreferredCurrency
                }
            };
            
            var profile = await _logtoUserService.UpdateUserProfileAsync(id, logtoUserRequest, cancellationToken);
            return Ok(profile);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Logto user profile {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Verify user email in Logto
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("users/{id}/verify-email")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyUserEmail(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _logtoUserService.VerifyUserEmailAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Logto user not found" });
            }

            return Ok(new { message = "Email verification initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email for Logto user {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Reset user password in Logto
    /// </summary>
    /// <param name="id">Logto user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpPost("users/{id}/reset-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetUserPassword(
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _logtoUserService.ResetUserPasswordAsync(id, cancellationToken);
            if (!result)
            {
                return NotFound(new { message = "Logto user not found" });
            }

            return Ok(new { message = "Password reset initiated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for Logto user {LogtoUserId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get Logto authentication status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication status</returns>
    [HttpGet("auth/status")]
    [ProducesResponseType(typeof(LogtoAuthStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LogtoAuthStatusResponse>> GetAuthStatus(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // GetAuthenticationStatusAsync requires userId parameter
            var userId = User.Identity?.Name ?? "anonymous";
            var status = await _logtoUserService.GetAuthenticationStatusAsync(userId, cancellationToken);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Logto authentication status");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
