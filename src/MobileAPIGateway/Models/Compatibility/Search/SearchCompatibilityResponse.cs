namespace MobileAPIGateway.Models.Compatibility.Search;

/// <summary>
/// Search compatibility response model for compatibility with the old MobileOrchestrator
/// </summary>
public class SearchCompatibilityResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// Gets or sets the message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the total count
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the total pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Gets or sets the results
    /// </summary>
    public List<SearchResultCompatibility>? Results { get; set; }
    
    /// <summary>
    /// Gets or sets the categories
    /// </summary>
    public List<string>? Categories { get; set; }
    
    /// <summary>
    /// Gets or sets the available filters
    /// </summary>
    public Dictionary<string, List<string>>? AvailableFilters { get; set; }
}

/// <summary>
/// Search result compatibility model for compatibility with the old MobileOrchestrator
/// </summary>
public class SearchResultCompatibility
{
    /// <summary>
    /// Gets or sets the ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the category
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Gets or sets the image URL
    /// </summary>
    public string? ImageUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the price
    /// </summary>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Gets or sets the currency
    /// </summary>
    public string? Currency { get; set; }
    
    /// <summary>
    /// Gets or sets the rating
    /// </summary>
    public decimal? Rating { get; set; }
    
    /// <summary>
    /// Gets or sets the review count
    /// </summary>
    public int? ReviewCount { get; set; }
    
    /// <summary>
    /// Gets or sets the metadata
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the last updated date
    /// </summary>
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
}
