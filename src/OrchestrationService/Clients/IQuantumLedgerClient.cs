using Refit;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for QuantumLedger.Hub communication
/// Transaction validation and recording with cryptographic security
/// </summary>
public interface IQuantumLedgerHubClient
{
    [Post("/api/transactions/validate")]
    Task<QuantumLedgerValidationResult> ValidateTransactionAsync(
        [Body] QuantumLedgerValidationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/transactions/record")]
    Task<QuantumLedgerRecordResult> RecordTransactionAsync(
        [Body] QuantumLedgerRecordRequest request,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for QuantumLedger transaction validation
/// </summary>
public class QuantumLedgerValidationRequest
{
    public string Operation { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FromAccount { get; set; } = string.Empty;
    public string ToAccount { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of QuantumLedger validation
/// </summary>
public class QuantumLedgerValidationResult
{
    public bool IsValid { get; set; }
    public string ValidationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; }
}

/// <summary>
/// Request for QuantumLedger transaction recording
/// </summary>
public class QuantumLedgerRecordRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public object TransactionData { get; set; } = new();
    public string SignatureValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of QuantumLedger recording
/// </summary>
public class QuantumLedgerRecordResult
{
    public string RecordId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
