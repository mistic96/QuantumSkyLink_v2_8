using PaymentGatewayService.Models.PIXBrazil;

namespace PaymentGatewayService.Services.Integrations;

/// <summary>
/// Interface for PIX Brazil payment integration service
/// </summary>
public interface IPIXBrazilService
{
    /// <summary>
    /// Creates a PIX payout (money out) transaction
    /// </summary>
    /// <param name="request">PIX payout request details</param>
    /// <returns>Transaction response with QR code and transaction details</returns>
    Task<PIXTransactionResponse> CreatePayoutAsync(PIXPayoutRequest request);

    /// <summary>
    /// Creates a PIX charge (money in) transaction with QR code
    /// </summary>
    /// <param name="request">PIX charge request details</param>
    /// <returns>Transaction response with QR code for payment</returns>
    Task<PIXTransactionResponse> CreateChargeAsync(PIXChargeRequest request);

    /// <summary>
    /// Gets the current status of a PIX transaction
    /// </summary>
    /// <param name="transactionId">The PIX transaction ID</param>
    /// <returns>Current transaction details and status</returns>
    Task<PIXTransactionResponse> GetTransactionStatusAsync(string transactionId);

    /// <summary>
    /// Processes webhook notifications from PIX provider
    /// </summary>
    /// <param name="payload">Webhook payload from PIX provider</param>
    /// <returns>True if webhook was processed successfully</returns>
    Task<bool> ProcessWebhookAsync(PIXWebhookPayload payload);

    /// <summary>
    /// Validates a PIX key (CPF, email, phone, or random)
    /// </summary>
    /// <param name="pixKey">The PIX key to validate</param>
    /// <param name="keyType">Type of PIX key (cpf, email, phone, random)</param>
    /// <returns>True if the PIX key is valid</returns>
    Task<bool> ValidatePixKeyAsync(string pixKey, string keyType);

    /// <summary>
    /// Generates a static PIX QR code for receiving payments
    /// </summary>
    /// <param name="amount">Amount in cents (optional for static QR)</param>
    /// <param name="description">Payment description</param>
    /// <returns>QR code data and base64 image</returns>
    Task<PIXQRCodeResponse> GenerateStaticQRCodeAsync(int? amount = null, string description = null);
}