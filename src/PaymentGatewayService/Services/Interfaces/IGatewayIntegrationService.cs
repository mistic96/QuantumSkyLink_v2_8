using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Models.Responses;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Gateway integration service interface
/// Handles integration with payment gateways (Square, Stripe)
/// </summary>
public interface IGatewayIntegrationService
{
    /// <summary>
    /// Executes a payment through the appropriate gateway
    /// </summary>
    /// <param name="payment">The payment to execute</param>
    /// <returns>The gateway execution result</returns>
    Task<GatewayExecutionResult> ExecutePaymentAsync(Payment payment);

    /// <summary>
    /// Processes a refund through the gateway
    /// </summary>
    /// <param name="refund">The refund to process</param>
    /// <returns>The gateway refund result</returns>
    Task<GatewayRefundResult> ProcessRefundAsync(Refund refund);

    /// <summary>
    /// Verifies a payment method with the gateway
    /// </summary>
    /// <param name="paymentMethod">The payment method to verify</param>
    /// <returns>The verification result</returns>
    Task<PaymentMethodVerificationResult> VerifyPaymentMethodAsync(PaymentMethod paymentMethod);

    /// <summary>
    /// Gets the health status of all gateways
    /// </summary>
    /// <returns>The gateway health status</returns>
    Task<GatewayHealthResponse> GetGatewayHealthAsync();

    /// <summary>
    /// Selects the best gateway for a payment
    /// </summary>
    /// <param name="payment">The payment to process</param>
    /// <returns>The selected gateway</returns>
    Task<PaymentGateway> SelectGatewayAsync(Payment payment);
}

/// <summary>
/// Gateway execution result
/// </summary>
public class GatewayExecutionResult
{
    public global::PaymentGatewayService.Data.Entities.PaymentStatus Status { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionUrl { get; set; }
}

/// <summary>
/// Gateway refund result
/// </summary>
public class GatewayRefundResult
{
    public Data.Entities.RefundStatus Status { get; set; }
    public string? GatewayRefundId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment method verification result
/// </summary>
public class PaymentMethodVerificationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
