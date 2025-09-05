using MobileAPIGateway.Models.Compatibility.Auth;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for the authentication compatibility service
/// </summary>
public interface IAuthCompatibilityService
{
    /// <summary>
    /// Sends a reset password request
    /// </summary>
    /// <param name="request">The password reset request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The password reset response</returns>
    Task<PasswordResetResponse> SendResetPasswordRequestAsync(PasswordResetRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a reset password operation
    /// </summary>
    /// <param name="request">The password reset operation request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The password reset response</returns>
    Task<PasswordResetResponse> ResetPasswordOperationAsync(PasswordResetOperationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a user is allowed to login
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="applicationType">The application type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user is allowed to login, otherwise false</returns>
    Task<bool> IsLoginAllowedAsync(string email, ApplicationInterfaceType applicationType, CancellationToken cancellationToken = default);
}
