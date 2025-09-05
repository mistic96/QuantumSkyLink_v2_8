namespace MobileAPIGateway.Models.CardManagement;

/// <summary>
/// Represents a request to add a new payment card
/// </summary>
public class AddCardRequest
{
    /// <summary>
    /// Gets or sets the card number
    /// </summary>
    public required string CardNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the cardholder name
    /// </summary>
    public required string CardholderName { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration month (1-12)
    /// </summary>
    public int ExpirationMonth { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration year (e.g., 2025)
    /// </summary>
    public int ExpirationYear { get; set; }
    
    /// <summary>
    /// Gets or sets the card verification value (CVV)
    /// </summary>
    public required string Cvv { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address ID
    /// </summary>
    public required string BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets whether this card should be set as the default payment method
    /// </summary>
    public bool SetAsDefault { get; set; }
}
