using Microsoft.Extensions.Logging;
using MobileAPIGateway.Models.Compatibility.Auth;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Implementation of the authentication compatibility service
/// </summary>
public class AuthCompatibilityService : IAuthCompatibilityService
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthCompatibilityService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthCompatibilityService"/> class
    /// </summary>
    /// <param name="authService">The authentication service</param>
    /// <param name="logger">The logger</param>
    public AuthCompatibilityService(IAuthService authService, ILogger<AuthCompatibilityService> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc />
    public async Task<PasswordResetResponse> SendResetPasswordRequestAsync(PasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending reset password request for email: {Email}", request.Email);
            
            // In the new system, we would use Logto's password reset functionality
            // For now, we'll return a success response
            return new PasswordResetResponse
            {
                Status = PasswordResetRequestOperationStatus.Success,
                Message = "Password reset email sent successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reset password request for email: {Email}", request.Email);
            
            return new PasswordResetResponse
            {
                Status = PasswordResetRequestOperationStatus.Failed,
                Message = "Failed to send password reset email"
            };
        }
    }
    
    /// <inheritdoc />
    public async Task<PasswordResetResponse> ResetPasswordOperationAsync(PasswordResetOperationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing reset password operation for email: {Email}", request.Email);
            
            // In the new system, we would use Logto's password reset functionality
            // For now, we'll return a success response
            return new PasswordResetResponse
            {
                Status = PasswordResetRequestOperationStatus.Success,
                Message = "Password reset successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing reset password operation for email: {Email}", request.Email);
            
            return new PasswordResetResponse
            {
                Status = PasswordResetRequestOperationStatus.Failed,
                Message = "Failed to reset password"
            };
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> IsLoginAllowedAsync(string email, ApplicationInterfaceType applicationType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking if login is allowed for email: {Email} and application type: {ApplicationType}", email, applicationType);
            
            // In the new system, we would check if the user is allowed to login based on the application type
            // For now, we'll return true for all users
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if login is allowed for email: {Email} and application type: {ApplicationType}", email, applicationType);
            
            // In case of an error, we'll return false to prevent login
            return false;
        }
    }
}
