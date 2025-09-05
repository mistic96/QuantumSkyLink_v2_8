using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.CardManagement;

/// <summary>
/// Add card compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class AddCardCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card number
    /// </summary>
    [Required]
    public string CardNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card holder name
    /// </summary>
    [Required]
    public string CardHolderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the expiration month
    /// </summary>
    [Required]
    public int ExpirationMonth { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration year
    /// </summary>
    [Required]
    public int ExpirationYear { get; set; }
    
    /// <summary>
    /// Gets or sets the CVV
    /// </summary>
    [Required]
    public string CVV { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the card type
    /// </summary>
    public string? CardType { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public CardAddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this is the default card
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Card address compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class CardAddressCompatibility
{
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Gets or sets the company name
    /// </summary>
    public string? CompanyName { get; set; }
    
    /// <summary>
    /// Gets or sets the street address line 1
    /// </summary>
    public string? StreetAddress1 { get; set; }
    
    /// <summary>
    /// Gets or sets the street address line 2
    /// </summary>
    public string? StreetAddress2 { get; set; }
    
    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Gets or sets the state or province
    /// </summary>
    public string? StateOrProvince { get; set; }
    
    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Gets or sets the country
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Gets or sets the phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string? EmailAddress { get; set; }
}
