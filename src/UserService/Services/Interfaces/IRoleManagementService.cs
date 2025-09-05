using UserService.Models.Requests;
using UserService.Models.Responses;

namespace UserService.Services.Interfaces;

public interface IRoleManagementService
{
    // Existing methods
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> AssignRoleWithExpirationAsync(Guid userId, Guid roleId, DateTime? expiresAt, string? assignedBy = null, string? reason = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> CheckPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default);
    Task<IEnumerable<PermissionResponse>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> QualifiesForMarketSellerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> QualifiesForLiquidityProviderAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> GetSellerTierAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<string> GetLiquidityTierAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpgradeUserRoleAsync(Guid userId, string targetRole, CancellationToken cancellationToken = default);
    Task CleanupExpiredRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> SeedSystemRolesAsync(CancellationToken cancellationToken = default);
    
    // Missing methods needed by controller
    Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleResponse> GetRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> AssignPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PermissionResponse>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
}
