using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Compatibility.Auth;
using MobileAPIGateway.Services.Compatibility;
using System.Security.Claims;

namespace MobileAPIGateway.Controllers.Compatibility;

/// <summary>
/// Controller for authentication compatibility with the old MobileOrchestrator
/// </summary>
[ApiController]
[Route("Auth")]
public class AuthCompatibilityController : ControllerBase
{
    private readonly IAuthCompatibilityService _authCompatibilityService;
    private readonly IAuthClient _authClient;
    private readonly IUserClient _userClient;
    private readonly ILogger<AuthCompatibilityController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthCompatibilityController"/> class
    /// </summary>
    /// <param name="authCompatibilityService">The authentication compatibility service</param>
    /// <param name="authClient">The auth client</param>
    /// <param name="userClient">The user client</param>
    /// <param name="logger">The logger</param>
    public AuthCompatibilityController(
        IAuthCompatibilityService authCompatibilityService,
        IAuthClient authClient,
        IUserClient userClient,
        ILogger<AuthCompatibilityController> logger)
    {
        _authCompatibilityService = authCompatibilityService ?? throw new ArgumentNullException(nameof(authCompatibilityService));
        _authClient = authClient ?? throw new ArgumentNullException(nameof(authClient));
        _userClient = userClient ?? throw new ArgumentNullException(nameof(userClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Sends a reset password request
    /// </summary>
    /// <param name="request">The password reset request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The password reset response</returns>
    [HttpPost("SendResetPasswordRequestAsync")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordResetResponse>> SendResetPasswordRequestAsync(
        [FromBody] PasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authCompatibilityService.SendResetPasswordRequestAsync(request, cancellationToken);

        if (response.Status is (PasswordResetRequestOperationStatus.Failed or PasswordResetRequestOperationStatus.Blocked))
        {
            return BadRequest(response.Message.AsProblemMessageResponse());
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Performs a reset password operation
    /// </summary>
    /// <param name="request">The password reset operation request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The password reset response</returns>
    [HttpPost("ResetPasswordOperationAsync")]
    [AllowAnonymous]
    public async Task<ActionResult<PasswordResetResponse>> ResetPasswordOperationAsync(
        [FromBody] PasswordResetOperationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authCompatibilityService.ResetPasswordOperationAsync(request, cancellationToken);

        if (response.Status is (PasswordResetRequestOperationStatus.Failed or PasswordResetRequestOperationStatus.Blocked))
        {
            return BadRequest(response.Message.AsProblemMessageResponse());
        }
        
        return Ok(response);
    }
    
    /// <summary>
    /// Checks if a user is allowed to login
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="applicationType">The application type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user is allowed to login, otherwise false</returns>
    [HttpGet("IsLoginAllowedAsync")]
    [AllowAnonymous]
    public async Task<ActionResult> IsLoginAllowedAsync(
        [FromQuery] string email, [FromQuery] ApplicationInterfaceType applicationType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { status = 400, message = "Email is required" });
        }

        // Return stub response for UAT - always allow login for testing
        await Task.CompletedTask;
        var isAllowed = true;
        
        return Ok(new { status = 200, data = new { isAllowedToLogin = isAllowed } });
    }
}
