using Refit;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for internal multisig operations
/// </summary>
public interface IInternalMultisigClient
{
    /// <summary>
    /// Create a new multisig wallet
    /// </summary>
    [Post("/api/multisig/wallets")]
    Task<MultisigWalletResponse> CreateWalletAsync([Body] CreateMultisigWalletRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get multisig wallet details
    /// </summary>
    [Get("/api/multisig/wallets/{walletId}")]
    Task<MultisigWalletResponse> GetWalletAsync(string walletId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submit a transaction for multisig approval
    /// </summary>
    [Post("/api/multisig/transactions")]
    Task<MultisigTransactionResponse> SubmitTransactionAsync([Body] SubmitMultisigTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve a multisig transaction
    /// </summary>
    [Post("/api/multisig/transactions/{transactionId}/approve")]
    Task<MultisigTransactionResponse> ApproveTransactionAsync(string transactionId, [Body] ApproveTransactionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a multisig transaction
    /// </summary>
    [Post("/api/multisig/transactions/{transactionId}/execute")]
    Task<MultisigTransactionResponse> ExecuteTransactionAsync(string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction status
    /// </summary>
    [Get("/api/multisig/transactions/{transactionId}")]
    Task<MultisigTransactionResponse> GetTransactionStatusAsync(string transactionId, CancellationToken cancellationToken = default);

    // Legacy methods for backwards compatibility (accept CancellationToken)
    [Post("/api/multisig/test-generate")]
    Task<object> TestGenerateAsync([Body] object request, CancellationToken cancellationToken = default);
    
    [Post("/api/multisig/persist")]
    Task<object> PersistAsync([Body] object request, CancellationToken cancellationToken = default);
    
    [Post("/api/multisig/publish-sets")]
    Task<object> PublishSetsAsync([Body] object request, CancellationToken cancellationToken = default);
    
    [Post("/api/multisig/ingest")]
    Task<object> IngestAsync([Body] object request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to create a multisig wallet
/// </summary>
public class CreateMultisigWalletRequest
{
    public string Name { get; set; } = string.Empty;
    public List<string> Signers { get; set; } = new();
    public int RequiredSignatures { get; set; }
    public string Network { get; set; } = string.Empty;
}

/// <summary>
/// Multisig wallet response
/// </summary>
public class MultisigWalletResponse
{
    public string WalletId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Signers { get; set; } = new();
    public int RequiredSignatures { get; set; }
    public string Network { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to submit a multisig transaction
/// </summary>
public class SubmitMultisigTransactionRequest
{
    public string WalletId { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Multisig transaction response
/// </summary>
public class MultisigTransactionResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string WalletId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public List<string> Approvals { get; set; } = new();
    public int RequiredApprovals { get; set; }
    public string? TxHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
}

/// <summary>
/// Request to approve a transaction
/// </summary>
public class ApproveTransactionRequest
{
    public string SignerId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
