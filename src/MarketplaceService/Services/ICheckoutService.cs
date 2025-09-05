namespace MarketplaceService.Services;

public interface ICheckoutService
{
    Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<CheckoutStatus> GetCheckoutStatusAsync(string checkoutId, CancellationToken cancellationToken = default);
}

public class CheckoutRequest
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class CheckoutResult
{
    public string CheckoutId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CheckoutStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
