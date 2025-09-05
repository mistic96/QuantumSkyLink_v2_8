using UserService.Models.Logto;
using UserService.Models.Requests;
using UserService.Models.Responses;

namespace UserService.Services.Interfaces;

public interface ILogtoUserService
{
    // Existing methods
    Task<LogtoApiResponse<LogtoUserResponse>> CreateUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<LogtoUserResponse>> GetUserAsync(string logtoUserId, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<LogtoUserResponse>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<LogtoUserResponse>> UpdateUserAsync(string logtoUserId, LogtoUserRequest request, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<bool>> DeleteUserAsync(string logtoUserId, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<LogtoTokenResponse>> AuthenticateUserAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<LogtoTokenResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<bool>> VerifyTokenAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<LogtoApiResponse<bool>> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    
    // Missing methods needed by controller
    Task<LogtoUserResponse> SyncUserFromLogtoAsync(string logtoUserId, CancellationToken cancellationToken = default);
    Task<LogtoUserProfileResponse> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<LogtoUserProfileResponse> UpdateUserProfileAsync(string userId, LogtoUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyUserEmailAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ResetUserPasswordAsync(string userId, CancellationToken cancellationToken = default);
    Task<LogtoAuthStatusResponse> GetAuthenticationStatusAsync(string userId, CancellationToken cancellationToken = default);
}
