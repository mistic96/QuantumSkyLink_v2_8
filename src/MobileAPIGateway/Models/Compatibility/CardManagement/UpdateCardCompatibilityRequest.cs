using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.CardManagement;

/// <summary>
/// Update card compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class UpdateCardCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the card ID
    /// </summary>
    [Required]
    public string CardId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card holder name
    /// </summary>
    public string? CardHolderName { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration month
    /// </summary>
    public int? ExpirationMonth { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration year
    /// </summary>
    public int? ExpirationYear { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public CardAddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this is the default card
    /// </summary>
    public bool? IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
