using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Data;
using UserService.Data.Entities;

namespace UserService.Tests;

public abstract class TestBase : IDisposable
{
    protected UserDbContext Context { get; private set; }
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new UserDbContext(options);
        SeedTestData();
    }

    protected virtual void SeedTestData()
    {
        // Seed basic test data
        var testUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            LogtoUserId = "test-logto-user-1",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsEmailVerified = true,
            IsPhoneVerified = true
        };

        var testRole = new Role
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "TestRole",
            Description = "Test Role for Unit Tests",
            IsSystemRole = false
        };

        var basicUserRole = new Role
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "BasicUser",
            Description = "Basic User Role",
            IsSystemRole = false
        };

        var marketSellerRole = new Role
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "MarketSeller",
            Description = "Market Seller Role",
            IsSystemRole = false
        };

        var testPermission = new Permission
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Resource = "TestResource",
            Action = "TestAction",
            Description = "Test Permission"
        };

        var testProfile = new UserProfile
        {
            UserId = testUser.Id,
            Country = "US",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var testWallet = new UserWallet
        {
            UserId = testUser.Id,
            WalletAddress = "0x1234567890123456789012345678901234567890",
            PublicKey = "test-public-key",
            IsVerified = true,
            Balance = 150000m,
            BalanceCurrency = "USD"
        };

        Context.Users.Add(testUser);
        Context.Roles.AddRange(testRole, basicUserRole, marketSellerRole);
        Context.Permissions.Add(testPermission);
        Context.UserProfiles.Add(testProfile);
        Context.UserWallets.Add(testWallet);

        Context.SaveChanges();
    }

    protected User GetTestUser() => Context.Users.First(u => u.Email == "test@example.com");
    protected Role GetTestRole() => Context.Roles.First(r => r.Name == "TestRole");
    protected Role GetBasicUserRole() => Context.Roles.First(r => r.Name == "BasicUser");
    protected Role GetMarketSellerRole() => Context.Roles.First(r => r.Name == "MarketSeller");
    protected Permission GetTestPermission() => Context.Permissions.First(p => p.Resource == "TestResource");

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
