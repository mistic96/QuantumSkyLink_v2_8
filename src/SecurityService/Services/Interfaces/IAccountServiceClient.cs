using Refit;

namespace SecurityService.Services.Interfaces;

public interface IAccountServiceClient
{
    [Get("/api/accounts/{accountId}")]
    Task<AccountResponse> GetAccountAsync(Guid accountId);

    [Get("/api/accounts/user/{userId}")]
    Task<List<AccountResponse>> GetUserAccountsAsync(Guid userId);

    [Post("/api/accounts/{accountId}/freeze")]
    Task FreezeAccountAsync(Guid accountId, [Body] FreezeAccountRequest request);

    [Post("/api/accounts/{accountId}/unfreeze")]
    Task UnfreezeAccountAsync(Guid accountId, [Body] UnfreezeAccountRequest request);

    [Get("/api/accounts/{accountId}/transactions")]
    Task<List<AccountTransactionResponse>> GetAccountTransactionsAsync(Guid accountId, int page = 1, int pageSize = 50);
}

// Response models for AccountService integration
public class AccountResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AccountTransactionResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FreezeAccountRequest
{
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

public class UnfreezeAccountRequest
{
    public string Reason { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
