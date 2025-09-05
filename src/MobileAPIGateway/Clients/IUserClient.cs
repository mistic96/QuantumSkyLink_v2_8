using Refit;
using MobileAPIGateway.Models.User;

namespace MobileAPIGateway.Clients;

/// <summary>
/// User client interface
/// </summary>
public interface IUserClient
{
    /// <summary>
    /// Gets the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [Get("/api/users/{userId}")]
    Task<UserDetails> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user details by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [Get("/api/users/by-email/{email}")]
    Task<UserDetails> GetUserDetailsByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update user request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    [Put("/api/users/{userId}")]
    Task<UserDetails> UpdateUserDetailsAsync(string userId, [Body] UpdateUserRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes the user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Change password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    [Post("/api/users/{userId}/change-password")]
    Task ChangePasswordAsync(string userId, [Body] ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
