using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for PaymentGatewayService integration
/// </summary>
public interface IPaymentGatewayServiceClient
{
    /// <summary>
    /// Create escrow account for marketplace transaction
    /// </summary>
    [Post("/api/escrow/create")]
    Task<EscrowAccountResponse> CreateEscrowAccountAsync(
        [Body] CreateEscrowAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fund escrow account with buyer's payment
    /// </summary>
    [Post("/api/escrow/{escrowId}/fund")]
    Task<EscrowFundingResponse> FundEscrowAccountAsync(
        Guid escrowId,
        [Body] FundEscrowAccountRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release escrow funds to seller
    /// </summary>
    [Post("/api/escrow/{escrowId}/release")]
    Task<EscrowReleaseResponse> ReleaseEscrowFundsAsync(
        Guid escrowId,
        [Body] ReleaseEscrowFundsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund escrow funds to buyer
    /// </summary>
    [Post("/api/escrow/{escrowId}/refund")]
    Task<EscrowRefundResponse> RefundEscrowFundsAsync(
        Guid escrowId,
        [Body] RefundEscrowFundsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get escrow account status
    /// </summary>
    [Get("/api/escrow/{escrowId}")]
    Task<EscrowAccountStatusResponse> GetEscrowAccountStatusAsync(
        Guid escrowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process direct payment (non-escrow)
    /// </summary>
    [Post("/api/payments/process")]
    Task<PaymentProcessingResponse> ProcessPaymentAsync(
        [Body] ProcessPaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create payment intent for marketplace transaction
    /// </summary>
    [Post("/api/payments/intent")]
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(
        [Body] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm payment intent
    /// </summary>
    [Post("/api/payments/intent/{paymentIntentId}/confirm")]
    Task<PaymentConfirmationResponse> ConfirmPaymentIntentAsync(
        string paymentIntentId,
        [Body] ConfirmPaymentIntentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment status
    /// </summary>
    [Get("/api/payments/{paymentId}")]
    Task<PaymentStatusResponse> GetPaymentStatusAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request models for PaymentGatewayService integration
/// </summary>
public class CreateEscrowAccountRequest
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class FundEscrowAccountRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public Guid BuyerId { get; set; }
    public string? PaymentIntentId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ReleaseEscrowFundsRequest
{
    public Guid SellerId { get; set; }
    public decimal? Amount { get; set; } // If null, release full amount
    public string ReleaseReason { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class RefundEscrowFundsRequest
{
    public Guid BuyerId { get; set; }
    public decimal? Amount { get; set; } // If null, refund full amount
    public string RefundReason { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ProcessPaymentRequest
{
    public Guid PayerId { get; set; }
    public Guid PayeeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethodId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class CreatePaymentIntentRequest
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public bool CaptureMethod { get; set; } = true; // true = automatic, false = manual
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ConfirmPaymentIntentRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Response models for PaymentGatewayService integration
/// </summary>
public class EscrowAccountResponse
{
    public Guid EscrowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class EscrowFundingResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PaymentIntentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime FundedAt { get; set; }
}

public class EscrowReleaseResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal AmountReleased { get; set; }
    public string? TransactionId { get; set; }
    public DateTime ReleasedAt { get; set; }
}

public class EscrowRefundResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal AmountRefunded { get; set; }
    public string? RefundId { get; set; }
    public DateTime RefundedAt { get; set; }
}

public class EscrowAccountStatusResponse
{
    public Guid EscrowId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public bool IsFunded { get; set; }
    public bool IsReleased { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FundedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class PaymentProcessingResponse
{
    public Guid PaymentId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class PaymentIntentResponse
{
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentConfirmationResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime ConfirmedAt { get; set; }
}

public class PaymentStatusResponse
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public Guid PayerId { get; set; }
    public Guid PayeeId { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
