using System.Threading;
using System.Threading.Tasks;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Square;

namespace PaymentGatewayService.Services.Integrations;

public interface ISquareService
{
    /// <summary>
    /// Create a payment with Square using a source token (card or digital wallet).
    /// Returns internal mapped status and gateway transaction id if available.
    /// </summary>
    Task<(PaymentStatus Status, string? GatewayTransactionId, string? Error)> CreatePaymentAsync(
        SquarePaymentRequest request,
        CancellationToken ct);

    /// <summary>
    /// Get payment status by Square payment id.
    /// </summary>
    Task<(PaymentStatus Status, string? Error)> GetPaymentStatusAsync(
        string gatewayPaymentId,
        CancellationToken ct);

    /// <summary>
    /// Create a refund for a given payment id with the specified amount in minor units.
    /// </summary>
    Task<(PaymentStatus Status, string? GatewayRefundId, string? Error)> CreateRefundAsync(
        string paymentId,
        long amountMinor,
        string idempotencyKey,
        string? reason,
        CancellationToken ct);

    /// <summary>
    /// Verify Square webhook signature using configured secret.
    /// </summary>
    Task<(bool Valid, string? Error)> VerifyWebhookSignatureAsync(
        string signatureHeader,
        string requestBody,
        string requestUrl,
        CancellationToken ct);

    /// <summary>
    /// Create a hosted payment link (Square Payment Links / Checkout) and return URL+expiry.
    /// </summary>
    Task<(string? CheckoutUrl, DateTime? ExpiresAt, string? Error)> CreatePaymentLinkAsync(
        long amountMinor,
        string currency,
        string referenceId,
        string? email,
        CancellationToken ct);
}
