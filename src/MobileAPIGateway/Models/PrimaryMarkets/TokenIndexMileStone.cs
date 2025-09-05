namespace MobileAPIGateway.Models.PrimaryMarkets;

/// <summary>
/// Token index milestone
/// </summary>
public sealed class TokenIndexMileStone
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
    /// Gets or sets the start date
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Gets or sets the end date
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Gets or sets the index milestone measurement
    /// </summary>
    public TokenIndexMileStoneMeasurement? IndexMileStoneMeasurement { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the project is completed
    /// </summary>
    public bool? IsProjectCompleted { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the milestone is in production
    /// </summary>
    public bool? IsInProduction { get; set; }
    
    /// <summary>
    /// Gets or sets the sort order
    /// </summary>
    public int? SortOrder { get; set; }
}
