using Refit;

namespace InfrastructureService.Services.Interfaces;

public interface IUserServiceClient
{
    [Get("/api/users/{userId}")]
    Task<ApiResponse<UserResponse>> GetUserAsync(Guid userId);

    [Get("/api/users/{userId}/exists")]
    Task<ApiResponse<bool>> UserExistsAsync(Guid userId);

    [Get("/api/users/{userId}/wallet")]
    Task<ApiResponse<UserWalletResponse>> GetUserWalletAsync(Guid userId);

    [Post("/api/users/{userId}/wallet")]
    Task<ApiResponse<UserWalletResponse>> CreateUserWalletAsync(Guid userId, [Body] CreateUserWalletRequest request);

    [Put("/api/users/{userId}/wallet")]
    Task<ApiResponse<UserWalletResponse>> UpdateUserWalletAsync(Guid userId, [Body] UpdateUserWalletRequest request);
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserWalletResponse
{
    public Guid UserId { get; set; }
    public string? WalletAddress { get; set; }
    public string? WalletType { get; set; }
    public string? Network { get; set; }
    public decimal Balance { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserWalletRequest
{
    public string WalletAddress { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
}

public class UpdateUserWalletRequest
{
    public string? WalletAddress { get; set; }
    public decimal? Balance { get; set; }
    public string? Status { get; set; }
}
