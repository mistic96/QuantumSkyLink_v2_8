using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Carts;

/// <summary>
/// Checkout cart compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class CheckoutCartCompatibilityRequest
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
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the payment method type
    /// </summary>
    public string? PaymentMethodType { get; set; }
    
    /// <summary>
    /// Gets or sets the shipping address
    /// </summary>
    public ShippingAddressCompatibility? ShippingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the billing address
    /// </summary>
    public BillingAddressCompatibility? BillingAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Shipping address compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class ShippingAddressCompatibility
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

/// <summary>
/// Billing address compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class BillingAddressCompatibility
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
