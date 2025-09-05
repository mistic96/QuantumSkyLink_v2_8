namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Order payment address
/// </summary>
public class OrderPaymentAddress
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the street address 1
    /// </summary>
    public string? StreetAddress1 { get; set; }
    
    /// <summary>
    /// Gets or sets the street address 2
    /// </summary>
    public string? StreetAddress2 { get; set; }
    
    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Gets or sets the state
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// Gets or sets the postal code
    /// </summary>
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Gets or sets the country
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Gets or sets the country alpha-2 code
    /// </summary>
    public string? CountryAlpha2Code { get; set; }
}
