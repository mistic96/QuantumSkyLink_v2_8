namespace PaymentGatewayService.Models;

public class ProcessPaymentRequest
{
    public string PaymentMethodId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
