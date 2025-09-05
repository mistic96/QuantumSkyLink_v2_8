namespace MobileAPIGateway.Models.TokenizedCart;

/// <summary>
/// Represents a request to checkout a tokenized cart
/// </summary>
public class CartCheckoutRequest
{
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address ID
    /// </summary>
    public string BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the wallet address to receive the tokens
    /// </summary>
    public string WalletAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the payment currency
    /// </summary>
    public string PaymentCurrency { get; set; }
    
    /// <summary>
    /// Gets or sets any additional notes for the checkout
    /// </summary>
    public string Notes { get; set; }
    
    /// <summary>
    /// Gets or sets whether to save the payment method for future use
    /// </summary>
    public bool SavePaymentMethod { get; set; }
}
