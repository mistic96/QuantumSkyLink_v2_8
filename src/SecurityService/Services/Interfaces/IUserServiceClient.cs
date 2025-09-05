using Refit;

namespace SecurityService.Services.Interfaces;

public interface IUserServiceClient
{
    [Get("/api/users/{userId}")]
    Task<UserResponse> GetUserAsync(Guid userId);

    [Get("/api/users/{userId}/security-settings")]
    Task<UserSecuritySettingsResponse> GetUserSecuritySettingsAsync(Guid userId);

    [Post("/api/users/{userId}/security-settings")]
    Task UpdateUserSecuritySettingsAsync(Guid userId, [Body] UpdateUserSecuritySettingsRequest request);

    [Get("/api/users/{userId}/roles")]
    Task<List<UserRoleResponse>> GetUserRolesAsync(Guid userId);
}

// Response models for UserService integration
public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserSecuritySettingsResponse
{
    public Guid UserId { get; set; }
    public bool MfaEnabled { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PreferredMfaMethod { get; set; }
    public bool LoginNotificationsEnabled { get; set; }
    public int SessionTimeoutMinutes { get; set; }
}

public class UserRoleResponse
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class UpdateUserSecuritySettingsRequest
{
    public bool MfaEnabled { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PreferredMfaMethod { get; set; }
    public bool LoginNotificationsEnabled { get; set; }
    public int SessionTimeoutMinutes { get; set; }
}
