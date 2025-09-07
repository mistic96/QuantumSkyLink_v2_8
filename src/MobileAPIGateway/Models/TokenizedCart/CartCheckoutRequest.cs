using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a request to checkout a unified cart
/// Supports checkout for both Primary Market (company products) and Secondary Market (P2P listings)
/// </summary>
public class CartCheckoutRequest
{
    /// <summary>
    /// Gets or sets the payment method ID (wallet, card, etc.)
    /// </summary>
    [Required]
    public string PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the wallet currency to use (if paying from wallet)
    /// </summary>
    public string? WalletCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address ID
    /// </summary>
    public string? BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the wallet address to receive purchased assets
    /// </summary>
    [Required]
    public string WalletAddress { get; set; }

    /// <summary>
    /// Gets or sets the payment currency for the transaction
    /// </summary>
    [Required]
    public string PaymentCurrency { get; set; } = "USD";
    
    /// <summary>
    /// Gets or sets any additional notes for the checkout
    /// </summary>
    public string Notes { get; set; }
    
    /// <summary>
    /// Gets or sets whether to save the payment method for future use
    /// </summary>
    public bool SavePaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets whether to use express checkout (skip confirmations)
    /// </summary>
    public bool ExpressCheckout { get; set; }
    
    /// <summary>
    /// Gets or sets escrow preferences for Secondary Market items
    /// </summary>
    public EscrowPreferences? EscrowPreferences { get; set; }
    
    /// <summary>
    /// Gets or sets split payment instructions if using multiple payment methods
    /// </summary>
    public SplitPaymentInstructions? SplitPayment { get; set; }
}

/// <summary>
/// Escrow preferences for Secondary Market transactions
/// </summary>
public class EscrowPreferences
{
    /// <summary>
    /// Gets or sets whether to auto-release on delivery confirmation
    /// </summary>
    public bool AutoReleaseOnDelivery { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum escrow duration in days
    /// </summary>
    public int MaxEscrowDays { get; set; } = 7;
    
    /// <summary>
    /// Gets or sets dispute resolution preference
    /// </summary>
    public string DisputeResolution { get; set; } = "Platform";
}

/// <summary>
/// Split payment instructions for using multiple payment methods
/// </summary>
public class SplitPaymentInstructions
{
    /// <summary>
    /// Gets or sets the primary payment method
    /// </summary>
    public PaymentSplit PrimaryPayment { get; set; }
    
    /// <summary>
    /// Gets or sets additional payment methods
    /// </summary>
    public List<PaymentSplit> AdditionalPayments { get; set; } = new();
}

/// <summary>
/// Individual payment split details
/// </summary>
public class PaymentSplit
{
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the amount for this payment method
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Gets or sets the currency for this payment
    /// </summary>
    public string Currency { get; set; }
}
