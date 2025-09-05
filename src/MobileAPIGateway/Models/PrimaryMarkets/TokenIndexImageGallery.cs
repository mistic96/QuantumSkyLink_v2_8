namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index image gallery
/// </summary>
public sealed class TokenIndexImageGallery
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the URL
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Gets or sets the sort order
    /// </summary>
    public int? SortOrder { get; set; }
}
