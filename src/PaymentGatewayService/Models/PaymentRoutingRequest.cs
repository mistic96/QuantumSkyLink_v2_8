using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models;

public class PaymentRoutingRequest
{
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethodType { get; set; } = string.Empty;
    public string? PaymentMethodId { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public string? PreferredGateway { get; set; }
    public bool RequireInstantSettlement { get; set; } = false;
    public string? CountryCode { get; set; }
    public int? MaxRetryAttempts { get; set; }
    public PaymentType PaymentType { get; set; } = PaymentType.Deposit;
    public PaymentGatewayType? PreferredProvider { get; set; }
}
