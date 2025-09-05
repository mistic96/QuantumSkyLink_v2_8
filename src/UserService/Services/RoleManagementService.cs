using Mapster;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Data.Entities;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly UserDbContext _context;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(UserDbContext context, ILogger<RoleManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await AssignRoleWithExpirationAsync(userId, roleId, null, null, null, cancellationToken);
    }

    public async Task<bool> AssignRoleWithExpirationAsync(Guid userId, Guid roleId, DateTime? expiresAt, string? assignedBy = null, string? reason = null, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Assigning role {RoleId} to user {UserId} with expiration {ExpiresAt} (CorrelationId: {CorrelationId})", 
            roleId, userId, expiresAt, correlationId);

        try
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);

            if (user == null || role == null)
            {
                _logger.LogWarning("User {UserId} or Role {RoleId} not found", userId, roleId);
                return false;
            }

            // Check if user already has this role
            var existingUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);

            if (existingUserRole != null)
            {
                _logger.LogInformation("User {UserId} already has role {RoleId}", userId, roleId);
                return true;
            }

            // Create new role assignment
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                ExpiresAt = expiresAt,
                AssignedBy = assignedBy,
                AssignmentReason = reason,
                IsActive = true
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} assigned successfully to user {UserId} (CorrelationId: {CorrelationId})", 
                roleId, userId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign role {RoleId} to user {UserId} (CorrelationId: {CorrelationId})", 
                roleId, userId, correlationId);
            throw;
        }
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Removing role {RoleId} from user {UserId} (CorrelationId: {CorrelationId})", 
            roleId, userId, correlationId);

        try
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive, cancellationToken);

            if (userRole == null)
            {
                _logger.LogWarning("Active role assignment not found for user {UserId} and role {RoleId}", userId, roleId);
                return false;
            }

            userRole.IsActive = false;
            userRole.RemovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} removed successfully from user {UserId} (CorrelationId: {CorrelationId})", 
                roleId, userId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove role {RoleId} from user {UserId} (CorrelationId: {CorrelationId})", 
                roleId, userId, correlationId);
            throw;
        }
    }

    public async Task<bool> CheckPermissionAsync(Guid userId, string resource, string action, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasPermission = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.IsActive)
                .Where(rp => rp.Permission.Resource == resource && rp.Permission.Action == action)
                .AnyAsync(cancellationToken);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check permission {Resource}.{Action} for user {UserId}", resource, action, userId);
            return false;
        }
    }

    public async Task<IEnumerable<PermissionResponse>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.IsActive)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync(cancellationToken);

            return permissions.Adapt<IEnumerable<PermissionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permissions for user {UserId}", userId);
            return Enumerable.Empty<PermissionResponse>();
        }
    }

    public async Task<IEnumerable<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            return userRoles.Adapt<IEnumerable<UserRoleResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get roles for user {UserId}", userId);
            return Enumerable.Empty<UserRoleResponse>();
        }
    }

    public async Task<bool> QualifiesForMarketSellerAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return false;

            // MarketSeller qualification criteria
            var qualifications = new[]
            {
                user.IsEmailVerified,
                user.IsPhoneVerified,
                user.Profile != null,
                !string.IsNullOrEmpty(user.Profile?.Country),
                user.Wallet?.IsVerified == true
            };

            return qualifications.All(q => q);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check MarketSeller qualification for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> QualifiesForLiquidityProviderAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Profile)
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return false;

            // LiquidityProvider qualification criteria (higher requirements)
            var qualifications = new[]
            {
                user.IsEmailVerified,
                user.IsPhoneVerified,
                user.Profile != null,
                !string.IsNullOrEmpty(user.Profile?.Country),
                user.Wallet?.IsVerified == true,
                user.Wallet?.Balance >= 100000m // $100K minimum
            };

            return qualifications.All(q => q);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check LiquidityProvider qualification for user {UserId}", userId);
            return false;
        }
    }

    public async Task<string> GetSellerTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user?.Wallet == null) return "None";

            // Tier based on wallet balance and activity
            var balance = user.Wallet.Balance;
            
            return balance switch
            {
                >= 1000000m => "Gold",      // $1M+
                >= 500000m => "Silver",     // $500K+
                >= 100000m => "Bronze",     // $100K+
                _ => "Basic"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get seller tier for user {UserId}", userId);
            return "None";
        }
    }

    public async Task<string> GetLiquidityTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user?.Wallet == null) return "None";

            // Tier based on liquidity provided
            var balance = user.Wallet.Balance;
            
            return balance switch
            {
                >= 5000000m => "Institutional", // $5M+
                >= 1000000m => "Premium",       // $1M+
                >= 500000m => "Standard",       // $500K+
                _ => "Basic"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get liquidity tier for user {UserId}", userId);
            return "None";
        }
    }

    public async Task<bool> UpgradeUserRoleAsync(Guid userId, string targetRole, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Upgrading user {UserId} to role {TargetRole} (CorrelationId: {CorrelationId})", 
            userId, targetRole, correlationId);

        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == targetRole, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("Target role {TargetRole} not found", targetRole);
                return false;
            }

            // Check qualifications based on target role
            var qualifies = targetRole switch
            {
                "MarketSeller" => await QualifiesForMarketSellerAsync(userId, cancellationToken),
                "LiquidityProvider" => await QualifiesForLiquidityProviderAsync(userId, cancellationToken),
                _ => false
            };

            if (!qualifies)
            {
                _logger.LogWarning("User {UserId} does not qualify for role {TargetRole}", userId, targetRole);
                return false;
            }

            // Assign the role
            var success = await AssignRoleWithExpirationAsync(userId, role.Id, null, "System", "Automatic upgrade", cancellationToken);

            if (success)
            {
                _logger.LogInformation("User {UserId} successfully upgraded to role {TargetRole} (CorrelationId: {CorrelationId})", 
                    userId, targetRole, correlationId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upgrade user {UserId} to role {TargetRole} (CorrelationId: {CorrelationId})", 
                userId, targetRole, correlationId);
            throw;
        }
    }

    public async Task CleanupExpiredRolesAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting expired roles cleanup (CorrelationId: {CorrelationId})", correlationId);

        try
        {
            var expiredRoles = await _context.UserRoles
                .Where(ur => ur.IsActive && ur.ExpiresAt != null && ur.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var userRole in expiredRoles)
            {
                userRole.IsActive = false;
                userRole.RemovedAt = DateTime.UtcNow;
                userRole.AssignmentReason = "Expired";
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} expired roles (CorrelationId: {CorrelationId})", 
                expiredRoles.Count, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired roles (CorrelationId: {CorrelationId})", correlationId);
            throw;
        }
    }

    public async Task<bool> SeedSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Seeding system roles and permissions (CorrelationId: {CorrelationId})", correlationId);

        try
        {
            // Define system roles
            var systemRoles = new (string Name, string Description, bool IsSystemRole, DateTime? ExpiresAt)[]
            {
                ("SuperAdmin", "System administrator with full access", true, null),
                ("SystemOperator", "Infrastructure management", true, null),
                ("ComplianceOfficer", "Regulatory oversight", true, null),
                ("TreasuryManager", "Treasury operations", true, null),
                ("BasicUser", "Standard user access", false, null),
                ("PremiumUser", "Premium user features", false, null),
                ("MarketSeller", "Secondary market seller", false, null),
                ("LiquidityProvider", "Liquidity pool provider", false, null),
                ("TrialUser", "Trial access", false, DateTime.UtcNow.AddDays(30)),
                ("GuestUser", "Guest access", false, DateTime.UtcNow.AddDays(7))
            };

            // Create roles if they don't exist
            foreach (var roleData in systemRoles)
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleData.Name, cancellationToken);

                if (existingRole == null)
                {
                    var role = new Role
                    {
                        Name = roleData.Name,
                        Description = roleData.Description,
                        IsSystemRole = roleData.IsSystemRole,
                        ExpiresAt = roleData.ExpiresAt
                    };

                    _context.Roles.Add(role);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("System roles seeded successfully (CorrelationId: {CorrelationId})", correlationId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed system roles (CorrelationId: {CorrelationId})", correlationId);
            return false;
        }
    }

    // Missing methods needed by controller
    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating role {RoleName} (CorrelationId: {CorrelationId})", 
            request.Name, correlationId);

        try
        {
            // Check if role already exists
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name, cancellationToken);

            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
            }

            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                IsSystemRole = false
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleName} created successfully with ID {RoleId} (CorrelationId: {CorrelationId})", 
                request.Name, role.Id, correlationId);

            return role.Adapt<RoleResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create role {RoleName} (CorrelationId: {CorrelationId})", 
                request.Name, correlationId);
            throw;
        }
    }

    public async Task<RoleResponse> GetRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException($"Role with ID {roleId} not found");
        }

        return role.Adapt<RoleResponse>();
    }

    public async Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .ToListAsync(cancellationToken);

        return roles.Adapt<IEnumerable<RoleResponse>>();
    }

    public async Task<RoleResponse> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating role {RoleId} (CorrelationId: {CorrelationId})", 
            roleId, correlationId);

        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role with ID {roleId} not found");
            }

            // Update role properties
            if (!string.IsNullOrEmpty(request.Name))
            {
                role.Name = request.Name;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                role.Description = request.Description;
            }

            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} updated successfully (CorrelationId: {CorrelationId})", 
                roleId, correlationId);

            return role.Adapt<RoleResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update role {RoleId} (CorrelationId: {CorrelationId})", 
                roleId, correlationId);
            throw;
        }
    }

    public async Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Deleting role {RoleId} (CorrelationId: {CorrelationId})", 
            roleId, correlationId);

        try
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return false;
            }

            // Check if role is a system role
            if (role.IsSystemRole)
            {
                throw new InvalidOperationException("Cannot delete system roles");
            }

            // Check if role is assigned to any users
            var hasUsers = await _context.UserRoles
                .AnyAsync(ur => ur.RoleId == roleId && ur.IsActive, cancellationToken);

            if (hasUsers)
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users");
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Role {RoleId} deleted successfully (CorrelationId: {CorrelationId})", 
                roleId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete role {RoleId} (CorrelationId: {CorrelationId})", 
                roleId, correlationId);
            throw;
        }
    }

    public async Task<bool> AssignPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Assigning permission {PermissionId} to role {RoleId} (CorrelationId: {CorrelationId})", 
            permissionId, roleId, correlationId);

        try
        {
            var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
            var permission = await _context.Permissions.FindAsync(new object[] { permissionId }, cancellationToken);

            if (role == null || permission == null)
            {
                return false;
            }

            // Check if permission is already assigned
            var existingRolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.IsActive, cancellationToken);

            if (existingRolePermission != null)
            {
                return true; // Already assigned
            }

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                IsActive = true
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Permission {PermissionId} assigned to role {RoleId} successfully (CorrelationId: {CorrelationId})", 
                permissionId, roleId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign permission {PermissionId} to role {RoleId} (CorrelationId: {CorrelationId})", 
                permissionId, roleId, correlationId);
            throw;
        }
    }

    public async Task<IEnumerable<PermissionResponse>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId && rp.IsActive)
                .Select(rp => rp.Permission)
                .ToListAsync(cancellationToken);

            return permissions.Adapt<IEnumerable<PermissionResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permissions for role {RoleId}", roleId);
            return Enumerable.Empty<PermissionResponse>();
        }
    }
}
