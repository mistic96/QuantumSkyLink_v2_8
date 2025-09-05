using Refit;

namespace PaymentGatewayService.Clients;

/// <summary>
/// Client interface for communicating with the Treasury Service
/// </summary>
public interface ITreasuryServiceClient
{
    /// <summary>
    /// Get wallet balance for a specific user and currency
    /// </summary>
    [Get("/api/wallet/{userId}/balance/{currency}")]
    Task<WalletBalanceResponse> GetWalletBalanceAsync(string userId, string currency);

    /// <summary>
    /// Create a wallet transaction
    /// </summary>
    [Post("/api/wallet/transaction")]
    Task<TransactionResponse> CreateTransactionAsync([Body] CreateTransactionRequest request);

    /// <summary>
    /// Get transaction status
    /// </summary>
    [Get("/api/wallet/transaction/{transactionId}")]
    Task<TransactionResponse> GetTransactionAsync(string transactionId);
}

/// <summary>
/// Response model for wallet balance queries
/// </summary>
public class WalletBalanceResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PendingBalance { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Request model for creating transactions
/// </summary>
public class CreateTransactionRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty; // "deposit", "withdrawal", "transfer"
    public string Reference { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Response model for transaction operations
/// </summary>
public class TransactionResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}