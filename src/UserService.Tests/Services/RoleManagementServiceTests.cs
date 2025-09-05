using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserService.Data.Entities;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Services;

public class RoleManagementServiceTests : TestBase
{
    private RoleManagementService CreateService()
    {
        return new RoleManagementService(Context, CreateMockLogger<RoleManagementService>().Object);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldAssignRole_WhenUserAndRoleExist()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role = GetTestRole();

        // Act
        var result = await service.AssignRoleAsync(user.Id, role.Id);

        // Assert
        result.Should().BeTrue();
        
        var userRole = await Context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        
        userRole.Should().NotBeNull();
        userRole!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var service = CreateService();
        var nonExistentUserId = Guid.NewGuid();
        var role = GetTestRole();

        // Act
        var result = await service.AssignRoleAsync(nonExistentUserId, role.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AssignRoleWithExpirationAsync_ShouldAssignRoleWithExpiration()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role = GetTestRole();
        var expirationDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = await service.AssignRoleWithExpirationAsync(
            user.Id, role.Id, expirationDate, "TestAdmin", "Test assignment");

        // Assert
        result.Should().BeTrue();
        
        var userRole = await Context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        
        userRole.Should().NotBeNull();
        userRole!.ExpiresAt.Should().BeCloseTo(expirationDate, TimeSpan.FromSeconds(1));
        userRole.AssignedBy.Should().Be("TestAdmin");
        userRole.AssignmentReason.Should().Be("Test assignment");
    }

    [Fact]
    public async Task RemoveRoleAsync_ShouldDeactivateRole_WhenRoleExists()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role = GetTestRole();

        // First assign the role
        await service.AssignRoleAsync(user.Id, role.Id);

        // Act
        var result = await service.RemoveRoleAsync(user.Id, role.Id);

        // Assert
        result.Should().BeTrue();
        
        var userRole = await Context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        
        userRole.Should().NotBeNull();
        userRole!.IsActive.Should().BeFalse();
        userRole.RemovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task QualifiesForMarketSellerAsync_ShouldReturnTrue_WhenUserMeetsAllCriteria()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Act
        var result = await service.QualifiesForMarketSellerAsync(user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task QualifiesForLiquidityProviderAsync_ShouldReturnTrue_WhenUserMeetsAllCriteria()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Act
        var result = await service.QualifiesForLiquidityProviderAsync(user.Id);

        // Assert
        result.Should().BeTrue(); // Test user has 150K balance which meets the 100K requirement
    }

    [Fact]
    public async Task QualifiesForLiquidityProviderAsync_ShouldReturnFalse_WhenBalanceIsTooLow()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        
        // Update wallet balance to be below threshold
        var wallet = await Context.UserWallets.FirstAsync(w => w.UserId == user.Id);
        wallet.Balance = 50000m; // Below 100K threshold
        await Context.SaveChangesAsync();

        // Act
        var result = await service.QualifiesForLiquidityProviderAsync(user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetSellerTierAsync_ShouldReturnCorrectTier_BasedOnBalance()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Test different balance scenarios
        var testCases = new[]
        {
            (Balance: 50000m, ExpectedTier: "Basic"),
            (Balance: 150000m, ExpectedTier: "Bronze"),
            (Balance: 600000m, ExpectedTier: "Silver"),
            (Balance: 1500000m, ExpectedTier: "Gold")
        };

        foreach (var (balance, expectedTier) in testCases)
        {
            // Update wallet balance
            var wallet = await Context.UserWallets.FirstAsync(w => w.UserId == user.Id);
            wallet.Balance = balance;
            await Context.SaveChangesAsync();

            // Act
            var tier = await service.GetSellerTierAsync(user.Id);

            // Assert
            tier.Should().Be(expectedTier, $"Balance {balance} should result in {expectedTier} tier");
        }
    }

    [Fact]
    public async Task GetLiquidityTierAsync_ShouldReturnCorrectTier_BasedOnBalance()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Test different balance scenarios
        var testCases = new[]
        {
            (Balance: 100000m, ExpectedTier: "Basic"),
            (Balance: 750000m, ExpectedTier: "Standard"),
            (Balance: 1500000m, ExpectedTier: "Premium"),
            (Balance: 6000000m, ExpectedTier: "Institutional")
        };

        foreach (var (balance, expectedTier) in testCases)
        {
            // Update wallet balance
            var wallet = await Context.UserWallets.FirstAsync(w => w.UserId == user.Id);
            wallet.Balance = balance;
            await Context.SaveChangesAsync();

            // Act
            var tier = await service.GetLiquidityTierAsync(user.Id);

            // Assert
            tier.Should().Be(expectedTier, $"Balance {balance} should result in {expectedTier} tier");
        }
    }

    [Fact]
    public async Task UpgradeUserRoleAsync_ShouldUpgradeToMarketSeller_WhenQualified()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Act
        var result = await service.UpgradeUserRoleAsync(user.Id, "MarketSeller");

        // Assert
        result.Should().BeTrue();
        
        var userRole = await Context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.Role.Name == "MarketSeller");
        
        userRole.Should().NotBeNull();
        userRole!.IsActive.Should().BeTrue();
        userRole.AssignedBy.Should().Be("System");
        userRole.AssignmentReason.Should().Be("Automatic upgrade");
    }

    [Fact]
    public async Task UpgradeUserRoleAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();

        // Act
        var result = await service.UpgradeUserRoleAsync(user.Id, "NonExistentRole");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredRolesAsync_ShouldDeactivateExpiredRoles()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role = GetTestRole();

        // Assign role with past expiration
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        await service.AssignRoleWithExpirationAsync(user.Id, role.Id, expiredDate);

        // Act
        await service.CleanupExpiredRolesAsync();

        // Assert
        var userRole = await Context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
        
        userRole.Should().NotBeNull();
        userRole!.IsActive.Should().BeFalse();
        userRole.RemovedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SeedSystemRolesAsync_ShouldCreateSystemRoles()
    {
        // Arrange
        var service = CreateService();
        
        // Clear existing roles
        Context.Roles.RemoveRange(Context.Roles);
        await Context.SaveChangesAsync();

        // Act
        var result = await service.SeedSystemRolesAsync();

        // Assert
        result.Should().BeTrue();
        
        var roles = await Context.Roles.ToListAsync();
        roles.Should().HaveCountGreaterThan(5);
        
        var systemRoles = roles.Where(r => r.IsSystemRole).ToList();
        systemRoles.Should().Contain(r => r.Name == "SuperAdmin");
        systemRoles.Should().Contain(r => r.Name == "SystemOperator");
        systemRoles.Should().Contain(r => r.Name == "ComplianceOfficer");
        
        var userRoles = roles.Where(r => !r.IsSystemRole).ToList();
        userRoles.Should().Contain(r => r.Name == "BasicUser");
        userRoles.Should().Contain(r => r.Name == "MarketSeller");
        userRoles.Should().Contain(r => r.Name == "LiquidityProvider");
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnActiveRoles()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role1 = GetTestRole();
        var role2 = GetBasicUserRole();

        // Assign roles
        await service.AssignRoleAsync(user.Id, role1.Id);
        await service.AssignRoleAsync(user.Id, role2.Id);

        // Act
        var userRoles = await service.GetUserRolesAsync(user.Id);

        // Assert
        userRoles.Should().HaveCount(2);
        userRoles.Should().Contain(ur => ur.RoleId == role1.Id);
        userRoles.Should().Contain(ur => ur.RoleId == role2.Id);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldNotReturnExpiredRoles()
    {
        // Arrange
        var service = CreateService();
        var user = GetTestUser();
        var role = GetTestRole();

        // Assign expired role
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        await service.AssignRoleWithExpirationAsync(user.Id, role.Id, expiredDate);

        // Act
        var userRoles = await service.GetUserRolesAsync(user.Id);

        // Assert
        userRoles.Should().BeEmpty();
    }
}
