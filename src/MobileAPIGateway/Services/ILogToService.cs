using System.Threading.Tasks;
using MobileAPIGateway.Models.Auth;

namespace MobileAPIGateway.Services
{
    public interface ILogToService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<SignupResponse> SignupAsync(SignupRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request);
        Task<PasswordResetResponse> ConfirmPasswordResetAsync(PasswordResetOperationRequest request);
        Task<UserProfile> GetUserProfileAsync(string userId);
        Task<bool> LogoutAsync(string userId);
        Task<bool> VerifyEmailAsync(string userId, string verificationCode);
        Task<bool> UpdateUserProfileAsync(string userId, UserProfile profile);
        
        // Additional methods required by AuthController
        Task<bool> ValidateTokenAsync(string token);
        Task<object> GetUserInfoAsync(string accessToken);
        Task<object> GetOrCreateUserFromLogToAsync(object userInfo);
        Task<object> SyncUserWithLogToAsync(object userInfo);
    }
}
