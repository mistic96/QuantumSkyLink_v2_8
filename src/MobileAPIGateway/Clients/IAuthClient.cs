using Refit;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Auth client interface
/// </summary>
public interface IAuthClient
{
    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">Login request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    [Post("/api/auth/login")]
    Task<LoginResponse> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refreshes a token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response</returns>
    [Post("/api/auth/refresh")]
    Task<LoginResponse> RefreshTokenAsync([Body] RefreshTokenRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user profile
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [Get("/api/auth/users/{userId}")]
    Task<UserProfile> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user profile by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile</returns>
    [Get("/api/auth/users/by-email/{email}")]
    Task<UserProfile> GetUserProfileByEmailAsync(string email, CancellationToken cancellationToken = default);
}
