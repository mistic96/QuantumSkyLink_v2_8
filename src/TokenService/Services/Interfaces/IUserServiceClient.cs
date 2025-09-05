using Refit;

namespace TokenService.Services.Interfaces;

[Headers("Accept: application/json", "X-API-Version: 1.0")]
public interface IUserServiceClient
{
    /// <summary>
    /// Check if a user exists
    /// </summary>
    [Get("/api/users/{userId}/exists")]
    Task<bool> UserExistsAsync(Guid userId);

    /// <summary>
    /// Get user information
    /// </summary>
    [Get("/api/users/{userId}")]
    Task<UserInfo> GetUserAsync(Guid userId);

    /// <summary>
    /// Get user wallet information
    /// </summary>
    [Get("/api/wallet/user/{userId}")]
    Task<UserWalletInfo> GetUserWalletAsync(Guid userId);
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserWalletInfo
{
    public Guid UserId { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
