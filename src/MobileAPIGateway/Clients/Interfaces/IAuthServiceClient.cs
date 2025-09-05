using Refit;
using MobileAPIGateway.Models;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Clients.Interfaces;

/// <summary>
/// Client interface for the Auth service
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>
    /// Sends a password reset request
    /// </summary>
    /// <param name="request">The password reset request</param>
    /// <returns>The password reset response</returns>
    [Post("/PasswordReset/SendResetPasswordRequest")]
    Task<PasswordResetResponse> SendResetPasswordRequestAsync([Body] PasswordResetRequest request);

    /// <summary>
    /// Resets a password
    /// </summary>
    /// <param name="request">The password reset operation request</param>
    /// <returns>The password reset response</returns>
    [Post("/PasswordReset/ResetPasswordOperation")]
    Task<object> ResetPasswordOperationAsync([Body] PasswordResetOperationRequest request);

    /// <summary>
    /// Checks if a user is allowed to login
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="applicationType">The application type</param>
    /// <returns>True if the user is allowed to login, otherwise false</returns>
    [Get("/AppRoles/IsLoginAllowed/{email}/{applicationType}")]
    Task<bool> IsLoginAllowedAsync([AliasAs("email")] string email, [AliasAs("applicationType")] ApplicationInterfaceType applicationType);
}
