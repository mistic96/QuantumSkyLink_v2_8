using MobileAPIGateway.Authentication;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Services;

/// <summary>
/// Auth service
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthClient _authClient;
    private readonly UserContextAccessor _userContextAccessor;
    private readonly ILogger<AuthService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class
    /// </summary>
    /// <param name="authClient">Auth client</param>
    /// <param name="userContextAccessor">User context accessor</param>
    /// <param name="logger">Logger</param>
    public AuthService(
        IAuthClient authClient,
        UserContextAccessor userContextAccessor,
        ILogger<AuthService> logger)
    {
        _authClient = authClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Logging in user with email {Email}", request.Email);
            
            var response = await _authClient.LoginAsync(request, cancellationToken);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user with email {Email}", request.Email);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refreshing token");
            
            var response = await _authClient.RefreshTokenAsync(request, cancellationToken);
            
            _logger.LogInformation("Token refreshed successfully for user {Email}", response.Email);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user profile for user {UserId}", userId);
            
            var userProfile = await _authClient.GetUserProfileAsync(userId, cancellationToken);
            
            _logger.LogInformation("User profile retrieved successfully for user {UserId}", userId);
            
            return userProfile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserProfile> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user profile for user with email {Email}", email);
            
            var userProfile = await _authClient.GetUserProfileByEmailAsync(email, cancellationToken);
            
            _logger.LogInformation("User profile retrieved successfully for user with email {Email}", email);
            
            return userProfile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for user with email {Email}", email);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserProfile> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        return await GetUserProfileByEmailAsync(userContext.Email, cancellationToken);
    }
}
