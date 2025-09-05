using MobileAPIGateway.Models.Compatibility.User;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for user compatibility service
/// </summary>
public interface IUserCompatibilityService
{
    /// <summary>
    /// Registers a user device
    /// </summary>
    /// <param name="request">The device registration request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The device registration response</returns>
    Task<UserDeviceRegistrationResponse> RegisterDeviceAsync(UserDeviceRegistrationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Synchronizes a user device
    /// </summary>
    /// <param name="request">The device synchronization request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The device synchronization response</returns>
    Task<UserDeviceSynchronizationResponse> SynchronizeDeviceAsync(UserDeviceSynchronizationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new user via lite mobile registration
    /// </summary>
    /// <param name="request">The registration request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The registration response</returns>
    Task<LiteMobileRegistrationResponse> LiteMobileRegistrationAsync(LiteMobileRegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronizes user data (initial or complete)
    /// </summary>
    /// <param name="request">The sync request</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The sync response</returns>
    Task<UserSyncResponse> PostSyncAsync(UserSyncRequest request, string clientIpAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user details by email address
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The user details</returns>
    Task<GetUserResponse> GetUserAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user profile information
    /// </summary>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The operation response</returns>
    Task<UserOperationResponse> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records user acceptance of terms and conditions
    /// </summary>
    /// <param name="request">The accept terms request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The operation response</returns>
    Task<UserOperationResponse> AcceptedTermsAsync(AcceptTermsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers MFA for a user
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The MFA registration response</returns>
    Task<MfaRegistrationResponse> RegisterMfaAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies MFA code for a user
    /// </summary>
    /// <param name="emailAddress">The user's email address</param>
    /// <param name="clientIpAddress">The client IP address</param>
    /// <param name="code">The MFA verification code</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The MFA verification response</returns>
    Task<MfaVerificationResponse> VerifyMfaCodeAsync(string emailAddress, string clientIpAddress, string code, CancellationToken cancellationToken = default);
}
