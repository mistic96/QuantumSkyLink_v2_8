using Refit;

namespace TreasuryService.Services.Interfaces;

[Headers("Accept: application/json", "X-API-Version: 1.0")]
public interface IAccountServiceClient
{
    /// <summary>
    /// Get account information
    /// </summary>
    [Get("/api/accounts/{accountId}")]
    Task<AccountInfo> GetAccountAsync(Guid accountId);

    /// <summary>
    /// Get account balance
    /// </summary>
    [Get("/api/accounts/{accountId}/balance")]
    Task<AccountBalanceInfo> GetAccountBalanceAsync(Guid accountId);

    /// <summary>
    /// Create account transaction
    /// </summary>
    [Post("/api/accounttransaction")]
    Task<TransactionResult> CreateTransactionAsync([Body] CreateTransactionRequest request);

    /// <summary>
    /// Get account transactions
    /// </summary>
    [Get("/api/accounttransaction/account/{accountId}")]
    Task<List<TransactionInfo>> GetAccountTransactionsAsync(Guid accountId, [Query] int page = 1, [Query] int pageSize = 20);

    /// <summary>
    /// Validate account limits
    /// </summary>
    [Post("/api/accountlimit/validate")]
    Task<LimitValidationResult> ValidateLimitAsync([Body] ValidateLimitRequest request);
}

public class AccountInfo
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountBalanceInfo
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class CreateTransactionRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class TransactionResult
{
    public Guid TransactionId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TransactionInfo
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ValidateLimitRequest
{
    public Guid AccountId { get; set; }
    public string LimitType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
}

public class LimitValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal RemainingLimit { get; set; }
}
