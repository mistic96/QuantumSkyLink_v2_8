namespace MobileAPIGateway.Models.Users;

/// <summary>
/// Country
/// </summary>
public class Country
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the alpha-2 code
    /// </summary>
    public string? Alpha2Code { get; set; }
    
    /// <summary>
    /// Gets or sets the alpha-3 code
    /// </summary>
    public string? Alpha3Code { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the country is active
    /// </summary>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether joining is allowed
    /// </summary>
    public bool? AllowedJoin { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether buying is allowed
    /// </summary>
    public bool? AllowedBuy { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether viewing is allowed
    /// </summary>
    public bool? AllowedView { get; set; }
    
    /// <summary>
    /// Gets or sets the continent ID
    /// </summary>
    public int? ContinentId { get; set; }
    
    /// <summary>
    /// Gets or sets the calling code
    /// </summary>
    public string? CallingCode { get; set; }
    
    /// <summary>
    /// Gets or sets the time zones
    /// </summary>
    public ICollection<UserTimeZone>? TimeZones { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the flag
    /// </summary>
    public string? Flag { get; set; }
}
