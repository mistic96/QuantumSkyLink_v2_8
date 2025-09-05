using System;

namespace MobileAPIGateway.Models.CardManagement;

/// <summary>
/// Represents a payment card in the system
/// </summary>
public class PaymentCard
{
    /// <summary>
    /// Gets or sets the card ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID who owns the card
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the card type (Visa, Mastercard, etc.)
    /// </summary>
    public string CardType { get; set; }
    
    /// <summary>
    /// Gets or sets the masked card number (e.g., **** **** **** 1234)
    /// </summary>
    public string MaskedNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the cardholder name
    /// </summary>
    public string CardholderName { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration month (1-12)
    /// </summary>
    public int ExpirationMonth { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration year (e.g., 2025)
    /// </summary>
    public int ExpirationYear { get; set; }
    
    /// <summary>
    /// Gets or sets whether the card is the default payment method
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address ID associated with this card
    /// </summary>
    public string BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the card status (Active, Expired, Suspended, etc.)
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the card was added
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the card was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the payment processor token for this card
    /// </summary>
    public string ProcessorToken { get; set; }
}
