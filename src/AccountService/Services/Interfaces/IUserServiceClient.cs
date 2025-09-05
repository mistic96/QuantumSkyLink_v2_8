using Refit;

namespace AccountService.Services.Interfaces;

public interface IUserServiceClient
{
    [Get("/api/users/{userId}")]
    Task<ApiResponse<UserDto>> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);

    [Get("/api/users/{userId}/exists")]
    Task<ApiResponse<bool>> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    [Get("/api/users/{userId}/roles")]
    Task<ApiResponse<IEnumerable<UserRoleDto>>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    [Get("/api/users/{userId}/verification-status")]
    Task<ApiResponse<UserVerificationStatusDto>> GetUserVerificationStatusAsync(Guid userId, CancellationToken cancellationToken = default);
}

// DTOs for UserService integration
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserRoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class UserVerificationStatusDto
{
    public Guid UserId { get; set; }
    public bool IsKycVerified { get; set; }
    public bool IsAmlVerified { get; set; }
    public string VerificationLevel { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
}
