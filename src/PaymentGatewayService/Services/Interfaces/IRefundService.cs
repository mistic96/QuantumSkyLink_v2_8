namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Service interface for handling refunds and return-to-sender operations
/// </summary>
public interface IRefundService
{
    /// <summary>
    /// Initiates a refund for a rejected deposit
    /// </summary>
    Task<RefundResult> InitiateRefundAsync(RefundRequest request);

    /// <summary>
    /// Gets the status of a refund
    /// </summary>
    Task<RefundStatus> GetRefundStatusAsync(Guid refundId);

    /// <summary>
    /// Processes a return-to-sender for a specific payment gateway
    /// </summary>
    Task<ReturnToSenderResult> ProcessReturnToSenderAsync(Guid paymentId, decimal refundAmount, string reason);
}

/// <summary>
/// Request to initiate a refund
/// </summary>
public class RefundRequest
{
    public Guid PaymentId { get; set; }
    public decimal RefundAmount { get; set; }
    public string RefundReason { get; set; } = string.Empty;
    public RefundType RefundType { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Result of a refund initiation
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public Guid? RefundId { get; set; }
    public string? RefundTransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public RefundStatus Status { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
}

/// <summary>
/// Types of refunds
/// </summary>
public enum RefundType
{
    Full,
    Partial,
    ReturnToSender,
    RejectionRefund
}

/// <summary>
/// Refund status information
/// </summary>
public class RefundStatus
{
    public Guid RefundId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? GatewayRefundId { get; set; }
    public string? FailureReason { get; set; }
}

/// <summary>
/// Result of return-to-sender operation
/// </summary>
public class ReturnToSenderResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public decimal AmountReturned { get; set; }
    public decimal FeesDeducted { get; set; }
    public string? ErrorMessage { get; set; }
    public string GatewayType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}