using PaymentGatewayService.Models;
using PaymentGatewayService.Models.Requests;
using PaymentGatewayService.Models.Responses;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Services.Interfaces;

public interface IPaymentGatewayService
{
    // Core payment processing methods
    Task<PaymentResult> ProcessPaymentAsync(global::PaymentGatewayService.Models.Requests.ProcessPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> ProcessRefundAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<global::PaymentGatewayService.Models.PaymentStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);

    // Gateway management methods (missing from interface but implemented in service)
    Task<IEnumerable<PaymentGatewayResponse>> GetActiveGatewaysAsync();
    Task<PaymentGatewayResponse?> GetGatewayAsync(Guid gatewayId);
    Task<PaymentGatewayResponse?> GetBestGatewayAsync(decimal amount, string currency, global::PaymentGatewayService.Models.PaymentType paymentType, string? country = null);
    Task<PaymentGatewayResponse> CreateGatewayAsync(CreatePaymentGatewayRequest request);
    Task<PaymentGatewayResponse> UpdateGatewayAsync(UpdatePaymentGatewayRequest request);
    Task<PaymentGatewayResponse> SetGatewayStatusAsync(Guid gatewayId, bool isActive);
    Task<GatewayStatisticsResponse> GetGatewayStatisticsAsync(Guid gatewayId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<GatewayTestResponse> TestGatewayAsync(Guid gatewayId);
    Task<IEnumerable<string>> GetSupportedCurrenciesAsync(Guid gatewayId);
    Task<IEnumerable<string>> GetSupportedCountriesAsync(Guid gatewayId);
}
