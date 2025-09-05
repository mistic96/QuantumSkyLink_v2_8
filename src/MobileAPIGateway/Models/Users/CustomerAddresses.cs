namespace MobileAPIGateway.Models.Users;

/// <summary>
/// Customer addresses
/// </summary>
public class CustomerAddresses
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the street address
    /// </summary>
    public string? StreetAddress { get; set; }
    
    /// <summary>
    /// Gets or sets the additional address information
    /// </summary>
    public string? AdditionalAddressInfo { get; set; }
    
    /// <summary>
    /// Gets or sets the state, province, or region
    /// </summary>
    public string? StateProvinceRegion { get; set; }
    
    /// <summary>
    /// Gets or sets the city
    /// </summary>
    public string? City { get; set; }
    
    /// <summary>
    /// Gets or sets the ZIP or postal code
    /// </summary>
    public string? ZipPostal { get; set; }
    
    /// <summary>
    /// Gets or sets the country
    /// </summary>
    public Country? Country { get; set; }
}
