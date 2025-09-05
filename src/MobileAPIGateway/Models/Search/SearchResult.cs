namespace MobileAPIGateway.Models.Search;

/// <summary>
/// Represents a search result item
/// </summary>
public class SearchResult
{
    /// <summary>
    /// Gets or sets the result ID
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the result title
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the result description
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the result URL
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Gets or sets the result image URL
    /// </summary>
    public string ImageUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the result category
    /// </summary>
    public string Category { get; set; }
    
    /// <summary>
    /// Gets or sets the result type
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Gets or sets the result created date
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the result updated date
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the result metadata
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; }
}
