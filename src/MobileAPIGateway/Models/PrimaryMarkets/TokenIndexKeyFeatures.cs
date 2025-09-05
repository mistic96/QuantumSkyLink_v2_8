using MobileAPIGateway.Models.Enums;

namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index key features
/// </summary>
public sealed class TokenIndexKeyFeatures
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
    /// Gets or sets the details
    /// </summary>
    public string? Details { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the feature is active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the type
    /// </summary>
    public KeyFeatureType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the sort order
    /// </summary>
    public int? SortOrder { get; set; }
}
