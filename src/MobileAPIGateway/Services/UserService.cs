using MobileAPIGateway.Authentication;
using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.User;

namespace MobileAPIGateway.Services;

/// <summary>
/// User service
/// </summary>
public class UserService : IUserService
{
    private readonly IUserClient _userClient;
    private readonly UserContextAccessor _userContextAccessor;
    private readonly ILogger<UserService> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class
    /// </summary>
    /// <param name="userClient">User client</param>
    /// <param name="userContextAccessor">User context accessor</param>
    /// <param name="logger">Logger</param>
    public UserService(
        IUserClient userClient,
        UserContextAccessor userContextAccessor,
        ILogger<UserService> logger)
    {
        _userClient = userClient;
        _userContextAccessor = userContextAccessor;
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public async Task<UserDetails> GetUserDetailsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user details for user {UserId}", userId);
            
            var userDetails = await _userClient.GetUserDetailsAsync(userId, cancellationToken);
            
            _logger.LogInformation("User details retrieved successfully for user {UserId}", userId);
            
            return userDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserDetails> GetUserDetailsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting user details for user with email {Email}", email);
            
            var userDetails = await _userClient.GetUserDetailsByEmailAsync(email, cancellationToken);
            
            _logger.LogInformation("User details retrieved successfully for user with email {Email}", email);
            
            return userDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for user with email {Email}", email);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserDetails> GetCurrentUserDetailsAsync(CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        return await GetUserDetailsByEmailAsync(userContext.Email, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<UserDetails> UpdateUserDetailsAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating user details for user {UserId}", userId);
            
            var userDetails = await _userClient.UpdateUserDetailsAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("User details updated successfully for user {UserId}", userId);
            
            return userDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user details for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<UserDetails> UpdateCurrentUserDetailsAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        return await UpdateUserDetailsAsync(userContext.UserId, request, cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Changing password for user {UserId}", userId);
            
            await _userClient.ChangePasswordAsync(userId, request, cancellationToken);
            
            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task ChangeCurrentUserPasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var userContext = _userContextAccessor.GetUserContext();
        
        if (!userContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }
        
        await ChangePasswordAsync(userContext.UserId, request, cancellationToken);
    }
}
