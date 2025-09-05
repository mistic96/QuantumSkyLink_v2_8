using System.Threading.Tasks;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Services.Interfaces;

/// <summary>
/// Abstraction for gateway-specific refund processing handlers.
/// Implementations should call the respective provider SDK/API and map responses to GatewayRefundResult.
/// </summary>
public interface IGatewayRefundHandler
{
    /// <summary>
    /// Process a refund for a given original payment using the gateway's API.
    /// </summary>
    /// <param name="payment">The original payment that is being refunded</param>
    /// <param name="refund">The refund entity containing amount, reason, etc.</param>
    /// <returns>Mapped gateway refund result</returns>
    Task<GatewayRefundResult> ProcessRefundAsync(Payment payment, Refund refund);
}
