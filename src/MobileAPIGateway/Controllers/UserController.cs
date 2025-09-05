using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.User;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// User controller
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class
    /// </summary>
    /// <param name="userService">User service</param>
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    /// <summary>
    /// Gets the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDetails>> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userDetails = await _userService.GetUserDetailsAsync(userId, cancellationToken);
        return Ok(userDetails);
    }
    
    /// <summary>
    /// Gets the current user details
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpGet("me")]
    public async Task<ActionResult<UserDetails>> GetCurrentUserDetailsAsync(CancellationToken cancellationToken = default)
    {
        var userDetails = await _userService.GetCurrentUserDetailsAsync(cancellationToken);
        return Ok(userDetails);
    }
    
    /// <summary>
    /// Updates the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update user request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDetails>> UpdateUserDetailsAsync(string userId, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userDetails = await _userService.UpdateUserDetailsAsync(userId, request, cancellationToken);
        return Ok(userDetails);
    }
    
    /// <summary>
    /// Updates the current user details
    /// </summary>
    /// <param name="request">Update user request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [HttpPut("me")]
    public async Task<ActionResult<UserDetails>> UpdateCurrentUserDetailsAsync([FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userDetails = await _userService.UpdateCurrentUserDetailsAsync(request, cancellationToken);
        return Ok(userDetails);
    }
    
    /// <summary>
    /// Changes the user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Change password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("{userId}/change-password")]
    public async Task<ActionResult> ChangePasswordAsync(string userId, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _userService.ChangePasswordAsync(userId, request, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Changes the current user password
    /// </summary>
    /// <param name="request">Change password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("me/change-password")]
    public async Task<ActionResult> ChangeCurrentUserPasswordAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _userService.ChangeCurrentUserPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
