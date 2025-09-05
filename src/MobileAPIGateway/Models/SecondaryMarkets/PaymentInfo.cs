using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Payment information
/// </summary>
public class PaymentInfo
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the fiat card payment ID
    /// </summary>
    public Guid FiatCardPaymentId { get; set; }
    
    /// <summary>
    /// Gets or sets the crypto payment ID
    /// </summary>
    public Guid CryptoPaymentId { get; set; }
    
    /// <summary>
    /// Gets or sets the wallet payment ID
    /// </summary>
    public string? WalletPaymentId { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public CloudCheckoutCurrency Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the total amount
    /// </summary>
    public decimal TotalAmount { get; set; }
}
