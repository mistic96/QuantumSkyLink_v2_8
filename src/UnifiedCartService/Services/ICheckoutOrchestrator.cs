using UnifiedCartService.Models.Entities;

namespace UnifiedCartService.Services;

/// <summary>
/// Orchestrates the checkout process across Primary and Secondary markets
/// </summary>
public interface ICheckoutOrchestrator
{
    /// <summary>
    /// Processes checkout for a cart
    /// </summary>
    Task<CheckoutResult> CheckoutCartAsync(
        string userId, 
        string cartId, 
        CheckoutRequest request, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Checkout request details
/// </summary>
public class CheckoutRequest
{
    public string PaymentMethodId { get; set; }
    public string? WalletCurrency { get; set; }
    public string WalletAddress { get; set; }
    public string PaymentCurrency { get; set; } = "USD";
    public bool ExpressCheckout { get; set; }
}

/// <summary>
/// Checkout result with order details
/// </summary>
public class CheckoutResult
{
    public bool Success { get; set; }
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
    public List<string> PrimaryMarketOrderIds { get; set; } = new();
    public List<string> SecondaryMarketOrderIds { get; set; } = new();
    public List<string> EscrowIds { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}