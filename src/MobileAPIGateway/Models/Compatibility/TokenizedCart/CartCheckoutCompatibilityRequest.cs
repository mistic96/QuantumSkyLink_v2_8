using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.TokenizedCart;

/// <summary>
/// Cart checkout compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CartCheckoutCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the cart ID
    /// </summary>
    [Required]
    public string CartId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the payment method ID
    /// </summary>
    public string? PaymentMethodId { get; set; }
    
    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string? PaymentMethodType { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public AddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping address
    /// </summary>
    public AddressCompatibility? ShippingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to use the same address for billing and shipping
    /// </summary>
    public bool UseSameAddressForBillingAndShipping { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping method
    /// </summary>
    public string? ShippingMethod { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping cost
    /// </summary>
    public decimal? ShippingCost { get; set; }
    
    /// <summary>
    /// Gets or sets the discount code
    /// </summary>
    public string? DiscountCode { get; set; }
    
    /// <summary>
    /// Gets or sets the notes
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Address compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class AddressCompatibility
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
