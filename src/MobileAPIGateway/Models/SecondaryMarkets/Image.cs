namespace MobileAPIGateway.Models.SecondaryMarkets;

/// <summary>
/// Image
/// </summary>
public class Image
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the URL
    /// </summary>
    public string? Url { get; set; }
}
