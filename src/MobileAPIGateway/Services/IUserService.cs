using MobileAPIGateway.Models.User;

namespace MobileAPIGateway.Services;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    Task<UserDetails> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the user details by email
    /// </summary>
    /// <param name="email">Email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    Task<UserDetails> GetUserDetailsByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current user details
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    Task<UserDetails> GetCurrentUserDetailsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the user details
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update user request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    Task<UserDetails> UpdateUserDetailsAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the current user details
    /// </summary>
    /// <param name="request">Update user request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User details</returns>
    Task<UserDetails> UpdateCurrentUserDetailsAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes the user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Change password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes the current user password
    /// </summary>
    /// <param name="request">Change password request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task ChangeCurrentUserPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
