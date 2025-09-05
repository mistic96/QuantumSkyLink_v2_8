namespace MobileAPIGateway.Models.CardManagement;

/// <summary>
/// Represents a request to update an existing payment card
/// </summary>
public class UpdateCardRequest
{
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
    /// Gets or sets the billing address ID
    /// </summary>
    public string BillingAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets whether this card should be set as the default payment method
    /// </summary>
    public bool SetAsDefault { get; set; }
}
