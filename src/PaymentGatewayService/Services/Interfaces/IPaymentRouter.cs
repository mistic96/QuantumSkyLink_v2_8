using PaymentGatewayService.Models;

namespace PaymentGatewayService.Services.Interfaces;

public interface IPaymentRouter
{
    Task<PaymentResult> RouteAsync(PaymentRouterRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> RoutePaymentAsync(ProcessPaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundResult> RouteRefundAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetStatusAsync(string transactionId, string provider, CancellationToken cancellationToken = default);
}
