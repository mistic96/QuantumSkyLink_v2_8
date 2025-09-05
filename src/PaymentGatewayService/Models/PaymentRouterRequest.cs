namespace PaymentGatewayService.Models;

public class PaymentRouterRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Provider { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
