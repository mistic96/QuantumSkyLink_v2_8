using Refit;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for PaymentGatewayService communication
/// Payment processing with signature validation integration
/// </summary>
public interface IPaymentGatewayServiceClient
{
    [Post("/api/payments/process")]
    Task<PaymentProcessingResult> ProcessPaymentAsync(
        [Body] PaymentProcessingRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/payments/{paymentId}/status")]
    Task<PaymentStatusResponse> GetPaymentStatusAsync(
        string paymentId,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for payment processing
/// </summary>
public class PaymentProcessingRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; } = string.Empty;
    public string ToAccountId { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of payment processing
/// </summary>
public class PaymentProcessingResult
{
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}

/// <summary>
/// Payment status response
/// </summary>
public class PaymentStatusResponse
{
    public string PaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
