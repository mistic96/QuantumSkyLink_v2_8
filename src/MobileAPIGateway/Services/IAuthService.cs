using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Services;

/// <summary>
/// Auth service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">Login request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes a token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user profile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user profile by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    Task<UserProfile> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current user profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    Task<UserProfile> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);
}
