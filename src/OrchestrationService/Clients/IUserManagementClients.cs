using Refit;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for UserService communication
/// User profile management and authentication
/// </summary>
public interface IUserServiceClient
{
    [Post("/api/users")]
    Task<UserCreationResult> CreateUserAsync(
        [Body] UserCreationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/users/{userId}")]
    Task<UserProfileResponse> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for AccountService communication
/// Account creation and balance management
/// </summary>
public interface IAccountServiceClient
{
    [Post("/api/accounts")]
    Task<AccountCreationResult> CreateAccountAsync(
        [Body] AccountCreationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/accounts/{accountId}/balance")]
    Task<AccountBalanceResponse> GetAccountBalanceAsync(
        string accountId,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for user creation
/// </summary>
public class UserCreationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public Dictionary<string, object> Profile { get; set; } = new();
}

/// <summary>
/// Result of user creation
/// </summary>
public class UserCreationResult
{
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// User profile response
/// </summary>
public class UserProfileResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public Dictionary<string, object> Profile { get; set; } = new();
}

/// <summary>
/// Request for account creation
/// </summary>
public class AccountCreationRequest
{
    public string AccountId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of account creation
/// </summary>
public class AccountCreationResult
{
    public string AccountId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Account balance response
/// </summary>
public class AccountBalanceResponse
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
