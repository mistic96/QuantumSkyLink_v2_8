namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Client interface for communicating with QuantumLedger.Hub
/// </summary>
public interface IQuantumLedgerHubClient
{
    /// <summary>
    /// Records a deposit code creation in the quantum ledger
    /// </summary>
    Task<bool> RecordDepositCodeCreationAsync(DepositCodeLedgerEntry entry);

    /// <summary>
    /// Records a deposit code usage in the quantum ledger
    /// </summary>
    Task<bool> RecordDepositCodeUsageAsync(DepositCodeUsageLedgerEntry entry);

    /// <summary>
    /// Records a deposit code rejection in the quantum ledger
    /// </summary>
    Task<bool> RecordDepositCodeRejectionAsync(DepositCodeRejectionLedgerEntry entry);

    /// <summary>
    /// Validates a deposit code exists in the ledger
    /// </summary>
    Task<DepositCodeLedgerValidation?> ValidateDepositCodeAsync(string depositCode);

    /// <summary>
    /// Gets all ledger entries for a specific deposit code
    /// </summary>
    Task<List<DepositCodeLedgerHistory>> GetDepositCodeHistoryAsync(string depositCode);

    /// <summary>
    /// Records a payment transaction in the ledger
    /// </summary>
    Task<string?> RecordPaymentTransactionAsync(PaymentLedgerEntry entry);

    /// <summary>
    /// Gets the balance for a specific account
    /// </summary>
    Task<decimal> GetAccountBalanceAsync(string accountAddress);
}

/// <summary>
/// Deposit code creation entry for the ledger
/// </summary>
public class DepositCodeLedgerEntry
{
    public string DepositCode { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Deposit code usage entry for the ledger
/// </summary>
public class DepositCodeUsageLedgerEntry
{
    public string DepositCode { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Deposit code rejection entry for the ledger
/// </summary>
public class DepositCodeRejectionLedgerEntry
{
    public string DepositCode { get; set; } = string.Empty;
    public Guid? PaymentId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public decimal FeeAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Deposit code validation result from the ledger
/// </summary>
public class DepositCodeLedgerValidation
{
    public bool Exists { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Deposit code history entry from the ledger
/// </summary>
public class DepositCodeLedgerHistory
{
    public string TransactionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Payment transaction entry for the ledger
/// </summary>
public class PaymentLedgerEntry
{
    public Guid PaymentId { get; set; }
    public string? UserId { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DepositCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}