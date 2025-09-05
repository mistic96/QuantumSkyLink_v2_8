using Refit;
using UserService.Models.Logto;

namespace UserService.Services.Interfaces;

public interface ILogtoApiClient
{
    [Post("/api/users")]
    Task<ApiResponse<LogtoUserResponse>> CreateUserAsync([Body] LogtoUserRequest request, CancellationToken cancellationToken = default);

    [Get("/api/users/{userId}")]
    Task<ApiResponse<LogtoUserResponse>> GetUserAsync(string userId, CancellationToken cancellationToken = default);

    [Get("/api/users")]
    Task<ApiResponse<LogtoUserResponse>> GetUserByEmailAsync([Query] string email, CancellationToken cancellationToken = default);

    [Patch("/api/users/{userId}")]
    Task<ApiResponse<LogtoUserResponse>> UpdateUserAsync(string userId, [Body] LogtoUserRequest request, CancellationToken cancellationToken = default);

    [Delete("/api/users/{userId}")]
    Task<ApiResponse<object>> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    [Post("/oidc/token")]
    Task<ApiResponse<LogtoTokenResponse>> GetTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    [Post("/oidc/token/revocation")]
    Task<ApiResponse<object>> RevokeTokenAsync([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    [Get("/oidc/me")]
    Task<ApiResponse<LogtoUserResponse>> GetCurrentUserAsync([Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}
