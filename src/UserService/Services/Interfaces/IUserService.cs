using UserService.Models.Requests;
using UserService.Models.Responses;

namespace UserService.Services.Interfaces;

public interface IUserService
{
    Task<UserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserByLogtoIdAsync(string logtoUserId, CancellationToken cancellationToken = default);
    Task<UserResponse> UpdateUserAsync(Guid userId, RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserResponse>> GetUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
}
