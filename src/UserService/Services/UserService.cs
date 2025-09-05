using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using UserService.Data;
using UserService.Data.Entities;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _context;
    private readonly ILogtoUserService _logtoUserService;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public UserService(
        UserDbContext context,
        ILogtoUserService logtoUserService,
        ILogger<UserService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logtoUserService = logtoUserService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<UserResponse> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting user registration for email: {Email} (CorrelationId: {CorrelationId})", 
            request.Email, correlationId);

        try
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create user in Logto
                var logtoResponse = await _logtoUserService.CreateUserAsync(request, cancellationToken);
                if (!logtoResponse.Success || logtoResponse.Data == null)
                {
                    throw new InvalidOperationException($"Failed to create user in Logto: {logtoResponse.Error?.Message}");
                }

                // Create user entity
                var user = new User
                {
                    LogtoUserId = logtoResponse.Data.Id,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    IsEmailVerified = logtoResponse.Data.IsEmailVerified,
                    IsPhoneVerified = logtoResponse.Data.IsPhoneVerified
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);

                // Create user profile
                var profile = new UserProfile
                {
                    UserId = user.Id,
                    Country = request.Country,
                    City = request.City,
                    PreferredLanguage = request.PreferredLanguage,
                    PreferredCurrency = request.PreferredCurrency
                };

                _context.UserProfiles.Add(profile);

                // Create security settings
                var securitySettings = new UserSecuritySettings
                {
                    UserId = user.Id
                };

                _context.UserSecuritySettings.Add(securitySettings);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Attempt to create default accounts for the new user (non-blocking for registration)
                await CreateDefaultAccountsAsync(user.Id, cancellationToken);

                _logger.LogInformation("User registration completed successfully for email: {Email} (CorrelationId: {CorrelationId})", 
                    request.Email, correlationId);

                // Load the complete user with related data
                var completeUser = await GetUserWithRelatedDataAsync(user.Id, cancellationToken);
                return completeUser.Adapt<UserResponse>();
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user (CorrelationId: {CorrelationId})", correlationId);
            throw;
        }
    }

    public async Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithRelatedDataAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Wallet)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with email {email} not found");
        }

        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> GetUserByLogtoIdAsync(string logtoUserId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Wallet)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.LogtoUserId == logtoUserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with Logto ID {logtoUserId} not found");
        }

        return user.Adapt<UserResponse>();
    }

    public async Task<UserResponse> UpdateUserAsync(Guid userId, RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetUserWithRelatedDataAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Update user properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;

        // Update profile if exists
        if (user.Profile != null)
        {
            user.Profile.Country = request.Country;
            user.Profile.City = request.City;
            user.Profile.PreferredLanguage = request.PreferredLanguage;
            user.Profile.PreferredCurrency = request.PreferredCurrency;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return user.Adapt<UserResponse>();
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Soft delete - just deactivate
        user.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<UserResponse>> GetUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Wallet)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return users.Adapt<IEnumerable<UserResponse>>();
    }

    public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);

        if (user == null || role == null)
        {
            return false;
        }

        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (existingUserRole != null)
        {
            return true; // Already assigned
        }

        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId
        };

        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (userRole == null)
        {
            return false;
        }

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<UserRoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .ToListAsync(cancellationToken);

        return userRoles.Adapt<IEnumerable<UserRoleResponse>>();
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<User?> GetUserWithRelatedDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.Wallet)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    private async Task CreateDefaultAccountsAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AccountService");

            // Define the two default accounts to create for a new user
            var requests = new[]
            {
                new
                {
                    UserId = userId,
                    AccountType = 1, // Individual
                    Description = "Primary individual account",
                    Currency = "USD",
                    DailyLimit = (decimal?)null,
                    MonthlyLimit = (decimal?)null,
                    AutoVerify = true,
                    Metadata = new Dictionary<string, object?>
                    {
                        ["source"] = "userservice",
                        ["purpose"] = "onboarding"
                    }
                },
                new
                {
                    UserId = userId,
                    AccountType = 5, // Savings
                    Description = "Default savings account",
                    Currency = "USD",
                    DailyLimit = (decimal?)null,
                    MonthlyLimit = (decimal?)null,
                    AutoVerify = true,
                    Metadata = new Dictionary<string, object?>
                    {
                        ["source"] = "userservice",
                        ["purpose"] = "onboarding"
                    }
                }
            };

            foreach (var req in requests)
            {
                var response = await client.PostAsJsonAsync("/api/accounts/internal", req, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Failed to create account for user {UserId}. Status={Status}; Body={Body}", userId, response.StatusCode, body);
                }
                else
                {
                    _logger.LogInformation("Created default account for user {UserId} with type {Type}", userId, req.AccountType);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default accounts for user {UserId}", userId);
        }
    }
}
