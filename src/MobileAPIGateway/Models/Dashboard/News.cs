namespace MobileAPIGateway.Models.Dashboard;

/// <summary>
/// News
/// </summary>
public sealed class News
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the date
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Gets or sets the report
    /// </summary>
    public string Report { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the image URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the source
    /// </summary>
    public string Source { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the tag
    /// </summary>
    public string Tag { get; set; } = string.Empty;
}
