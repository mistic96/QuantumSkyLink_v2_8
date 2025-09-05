namespace MobileAPIGateway.Models.Compatibility.CardManagement;

/// <summary>
/// Update card compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class UpdateCardCompatibilityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card ID
    /// </summary>
    public string CardId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the masked card number
    /// </summary>
    public string MaskedCardNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card holder name
    /// </summary>
    public string CardHolderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the expiration month
    /// </summary>
    public int ExpirationMonth { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration year
    /// </summary>
    public int ExpirationYear { get; set; }
    
    /// <summary>
    /// Gets or sets the card type
    /// </summary>
    public string? CardType { get; set; }
    
    /// <summary>
    /// Gets or sets the card brand
    /// </summary>
    public string? CardBrand { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public CardAddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this is the default card
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the card status
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
}
