using MobileAPIGateway.Models.Compatibility.Payment;

namespace MobileAPIGateway.Services.Compatibility
{
    public interface IPaymentCompatibilityService
    {
        Task<PaymentRequestDataResponse> GeneratePaymentRequestByCheckoutIdAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
        Task<CryptoChargeDataResponse> GenerateCryptoChargeAsync(string emailAddress, string clientIpAddress, CancellationToken cancellationToken = default);
    }
}
