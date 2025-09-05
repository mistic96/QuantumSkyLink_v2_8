using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Auth;
using MobileAPIGateway.Services;
using System.Text.Json;
using System.Text;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Auth controller
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly HttpClient _userServiceClient;

    private readonly ILogToService _logToService;
    private readonly ILogger<AuthController> _logger;
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class
    /// </summary>
    /// <param name="authService">Auth service</param>
    /// <param name="httpClientFactory">HTTP client factory</param>
    /// <param name="logToService">LogTo service</param>
    /// <param name="logger">Logger</param>
    public AuthController(IAuthService authService, IHttpClientFactory httpClientFactory, ILogToService logToService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userServiceClient = httpClientFactory.CreateClient("UserService");
        _logToService = logToService;
        _logger = logger;
    }

    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">Login request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Refreshes a token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets the user profile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [HttpGet("users/{userId}")]
    [Authorize]
    public async Task<ActionResult<UserProfile>> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var userProfile = await _authService.GetUserProfileAsync(userId, cancellationToken);
        return Ok(userProfile);
    }

    /// <summary>
    /// Gets the current user profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfile>> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var userProfile = await _authService.GetCurrentUserProfileAsync(cancellationToken);
        return Ok(userProfile);
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">Signup request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Signup response</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<SignupResponse>> RegisterAsync([FromBody] SignupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate terms acceptance
            if (!request.AcceptTerms || !request.AcceptPrivacyPolicy)
            {
                return BadRequest(new { message = "You must accept the terms and conditions and privacy policy to register." });
            }

            // Create UserService registration request
            var userServiceRequest = new
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Country = request.Country,
                City = request.City,
                PreferredLanguage = request.PreferredLanguage,
                PreferredCurrency = request.PreferredCurrency,
                AcceptTerms = request.AcceptTerms,
                AcceptPrivacyPolicy = request.AcceptPrivacyPolicy
            };

            // Forward request to UserService
            var json = JsonSerializer.Serialize(userServiceRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _userServiceClient.PostAsync("/api/users/register", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var userResponse = JsonSerializer.Deserialize<dynamic>(responseContent);

                return Ok(new SignupResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    Email = request.Email,
                    Name = $"{request.FirstName} {request.LastName}",
                    CreatedAt = DateTime.UtcNow,
                    RequiresEmailVerification = false,
                    Metadata = new Dictionary<string, object>
                    {
                        ["source"] = "mobile_app",
                        ["registrationMethod"] = "email_password"
                    }
                });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Handle specific error cases
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return Conflict(new SignupResponse
                    {
                        Success = false,
                        Message = "A user with this email already exists",
                        Email = request.Email
                    });
                }

                return BadRequest(new SignupResponse
                {
                    Success = false,
                    Message = "Registration failed. Please try again.",
                    Email = request.Email
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SignupResponse
            {
                Success = false,
                Message = "An internal error occurred during registration",
                Email = request.Email
            });
        }
    }
    // Public endpoint for basic connectivity testing
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {

        return Ok(new { message = "Public endpoint accessible", authenticated = false });
    }

    // Protected endpoint requiring authentication
    [Authorize]
    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {


        return Ok(new
        {
            message = "Authentication successful",
            authenticated = true,
            userId = User.FindFirst("sub")?.Value,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
    
     /// <summary>
    /// Validates a JWT access token and retrieves associated user information
    /// </summary>
    /// <param name="request">Token validation request containing the access token</param>
    /// <returns>Validation result with user details if token is valid</returns>
    /// <response code="200">Token is valid, returns user information</response>
    /// <response code="400">Missing or malformed request</response>
    /// <response code="401">Invalid or expired token</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/token/validate
    ///     {
    ///         "AccessToken": "eyJhbGciOiJFUzM4NCIsInR5cCI6IkpXVCJ..."
    ///     }
    /// 
    /// Response for user token:
    /// 
    ///     {
    ///         "valid": true,
    ///         "tokenType": "user",
    ///         "user": {
    ///             "id": "u123e456-e89b-12d3-a456-426614174000",
    ///             "email": "john.doe@hospital.com",
    ///             "firstName": "John",
    ///             "lastName": "Doe",
    ///             "role": "Technician",
    ///             "isActive": true,
    ///             "companyId": "c123e456-e89b-12d3-a456-426614174000"
    ///         }
    ///     }
    /// 
    /// This endpoint:
    /// - Validates token signature and expiration
    /// - Creates new user record if first login
    /// - Updates user information from OIDC provider
    /// - Returns current role and permissions
    /// </remarks>
    [HttpPost("token/validate")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.AccessToken))
            {
                return BadRequest(new { error = "Access token is required" });
            }

            var isValid = await _logToService.ValidateTokenAsync(request.AccessToken);
            
            if (!isValid)
            {
                return Unauthorized(new { error = "Invalid or expired token" });
            }

            // Check if this is a machine-to-machine token
            if (IsM2MToken(request.AccessToken))
            {
                _logger.LogInformation("Machine-to-machine token validated successfully");
                return Ok(new
                {
                    valid = true,
                    tokenType = "machine-to-machine",
                    message = "M2M token is valid"
                });
            }

            // Handle user token - try to get user information
            var userInfo = await _logToService.GetUserInfoAsync(request.AccessToken);
            
            if (userInfo == null)
            {
                return Unauthorized(new { error = "Unable to retrieve user information" });
            }

            var user = await _logToService.GetOrCreateUserFromLogToAsync(userInfo);
            
            if (user == null)
            {
                return StatusCode(500, new { error = "Failed to create or retrieve user" });
            }

                return Ok(new
                {
                    valid = true,
                    tokenType = "user",
                    user = new
                    {
                        
                        email = (user as dynamic)?.Email?.ToString() ?? string.Empty,
                        firstName = (user as dynamic)?.FirstName?.ToString() ?? string.Empty,
                        lastName = (user as dynamic)?.LastName?.ToString() ?? string.Empty,
                        role = (user as dynamic)?.Role?.ToString() ?? string.Empty
                    }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Retrieves the authenticated user's profile information
    /// </summary>
    /// <returns>Current user's profile data</returns>
    /// <response code="200">Returns user profile</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">User profile not found</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/auth/user/profile
    ///     Authorization: Bearer {token}
    /// 
    /// Response:
    /// 
    ///     {
    ///         "id": "u123e456-e89b-12d3-a456-426614174000",
    ///         "email": "john.doe@hospital.com",
    ///         "firstName": "John",
    ///         "lastName": "Doe",
    ///         "fullName": "John Doe",
    ///         "role": "Technician",
    ///         "status": "Active",
    ///         "companyId": "c123e456-e89b-12d3-a456-426614174000",
    ///         "companyName": "General Hospital",
    ///         "permissions": ["equipment.view", "workorder.create", "workorder.update"],
    ///         "lastLogin": "2024-04-15T10:30:00Z",
    ///         "createdAt": "2024-01-15T08:00:00Z"
    ///     }
    /// 
    /// Profile is synchronized with LogTo on each request.
    /// </remarks>
    [HttpGet("user/profile")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetUserProfile()
    {
        try
        {
            var accessToken = GetAccessTokenFromHeader();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized(new { error = "Access token not found" });
            }

            var userInfo = await _logToService.GetUserInfoAsync(accessToken);
            
            if (userInfo == null)
            {
                return Unauthorized(new { error = "Unable to retrieve user information" });
            }

            var user = await _logToService.GetOrCreateUserFromLogToAsync(userInfo);
            
            if (user == null)
            {
                return StatusCode(500, new { error = "Failed to retrieve user" });
            }

            return Ok(new
            {
                id = (user as dynamic)?.LogtoUserId?.ToString() ?? string.Empty,
                email = (user as dynamic)?.Email?.ToString() ?? string.Empty,
                firstName = (user as dynamic)?.FirstName?.ToString() ?? string.Empty,
                lastName = (user as dynamic)?.LastName?.ToString() ?? string.Empty,
                username = (user as dynamic)?.Username?.ToString() ?? string.Empty,
                role = (user as dynamic)?.Role?.ToString() ?? string.Empty,
                lastLoginAt = (user as dynamic)?.LastLoginAt?.ToString() ?? string.Empty,
                isExternalAuth = (user as dynamic)?.IsExternalAuth?.ToString() ?? string.Empty,
                externalAuthProvider = (user as dynamic)?.ExternalAuthProvider?.ToString() ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("user/sync")]
    public async Task<IActionResult> SyncUser()
    {
        try
        {
            _logger.LogInformation("SyncUser endpoint called");
            
            var accessToken = GetAccessTokenFromHeader();
            
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Access token not found in request headers");
                return Unauthorized(new { error = "Access token not found" });
            }

            _logger.LogInformation("Validating access token...");
            var isValid = await _logToService.ValidateTokenAsync(accessToken);
            
            if (!isValid)
            {
                _logger.LogWarning("Access token validation failed");
                return Unauthorized(new { error = "Invalid or expired access token" });
            }

            // Check if this is a machine-to-machine token
            if (IsM2MToken(accessToken))
            {
                _logger.LogInformation("Machine-to-machine token detected for user sync - returning service authentication success");
                return Ok(new
                {
                    message = "Service authentication successful",
                    tokenType = "machine-to-machine",
                    authenticated = true
                });
            }

            _logger.LogInformation("Getting user info from LogTo...");
            var userInfo = await _logToService.GetUserInfoAsync(accessToken);
            
            if (userInfo == null)
            {
                _logger.LogWarning("Unable to retrieve user information from LogTo");
                return Unauthorized(new { error = "Unable to retrieve user information" });
            }

            var userIdStr = (userInfo as dynamic)?.Sub?.ToString() ?? string.Empty;
            var emailStr = (userInfo as dynamic)?.Email?.ToString() ?? string.Empty;
            _logger.LogInformation($"Syncing user with LogTo data. User ID: {userIdStr}, Email: {emailStr}");
            var user = await _logToService.SyncUserWithLogToAsync(userInfo);
            
            if (user == null)
            {
                var userId = (userInfo as dynamic)?.Sub?.ToString() ?? string.Empty;
                _logger.LogWarning($"User sync failed for LogTo user ID: {userId}");
                return NotFound(new { error = "User not found" });
            }

            var logtoUserId = (user as dynamic)?.LogtoUserId?.ToString() ?? string.Empty;
            var userEmail = (user as dynamic)?.Email?.ToString() ?? string.Empty;
            _logger.LogInformation($"User synchronized successfully. User ID: {logtoUserId}, Email: {userEmail}");
            return Ok(new
            {
                message = "User synchronized successfully",
                user = new
                {
                    
                    email = (user as dynamic)?.Email?.ToString() ?? string.Empty,
                    firstName = (user as dynamic)?.FirstName?.ToString() ?? string.Empty,
                    lastName = (user as dynamic)?.LastName?.ToString() ?? string.Empty,
                    lastSyncAt = (user as dynamic)?.LastSyncAt?.ToString() ?? string.Empty
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing user");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // For LogTo, logout is handled on the frontend
        // This endpoint can be used for any server-side cleanup if needed
        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("status")]
    [Authorize]
    public IActionResult GetAuthStatus()
    {
        // This endpoint is used as a lightweight way to check if the user's token is valid.
        // The [Authorize] attribute handles the validation. If the request reaches this point,
        // the user is considered authenticated.
        return Ok(new { status = "authenticated", timestamp = DateTime.UtcNow });
    }

    private bool IsM2MToken(string accessToken)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);
            
            // M2M tokens typically have the client_id as the subject and no user-specific claims
            // They also don't have user-related scopes like 'profile', 'email', etc.
            var subject = token.Subject;
            var clientId = token.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            
            // If subject equals client_id, it's likely an M2M token
            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(clientId) && subject == clientId)
            {
                _logger.LogInformation("Detected M2M token: subject={Subject}, client_id={ClientId}", subject, clientId);
                return true;
            }
            
            // Additional check: M2M tokens typically don't have user-specific claims
            var hasUserClaims = token.Claims.Any(c => 
                c.Type == "email" || 
                c.Type == "given_name" || 
                c.Type == "family_name" || 
                c.Type == "name" ||
                c.Type == "picture");
            
            if (!hasUserClaims && !string.IsNullOrEmpty(clientId))
            {
                _logger.LogInformation("Detected M2M token: no user claims, client_id={ClientId}", clientId);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error determining token type, assuming user token");
            return false;
        }
    }

    private string? GetAccessTokenFromHeader()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        
        _logger.LogInformation("Authorization header: {AuthHeader}", 
            string.IsNullOrEmpty(authHeader) ? "NULL" : $"{authHeader.Substring(0, Math.Min(30, authHeader.Length))}...");
        
        if (string.IsNullOrEmpty(authHeader))
        {
            _logger.LogWarning("Authorization header is missing");
            return null;
        }
        
        // Case-insensitive Bearer token check
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Authorization header does not start with 'Bearer '. Header: {Header}", authHeader);
            return null;
        }

        // Extract token and apply comprehensive cleanup
        var token = authHeader.Substring("Bearer ".Length)
            .Trim()
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("\t", "")
            .Replace(" ", "");
        
        // Validate token is not empty after cleanup
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token is empty after cleanup");
            return null;
        }

        // Basic JWT format validation (3 parts separated by dots)
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            _logger.LogWarning("Token does not have valid JWT format. Expected 3 parts, got {PartCount}", parts.Length);
            return null;
        }
        
        _logger.LogInformation("Extracted token length: {TokenLength}, Token prefix: {TokenPrefix}, Parts: {PartCount}", 
            token.Length, 
            token.Length > 20 ? token.Substring(0, 20) : token,
            parts.Length);
        
        return token;
    }

}
public class ValidateTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
}
