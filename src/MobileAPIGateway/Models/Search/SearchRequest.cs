namespace MobileAPIGateway.Models.Search;

/// <summary>
/// Represents a search request
/// </summary>
public class SearchRequest
{
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    public string Query { get; set; }
    
    /// <summary>
    /// Gets or sets the search category
    /// </summary>
    public string Category { get; set; }
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the sort field
    /// </summary>
    public string SortBy { get; set; }
    
    /// <summary>
    /// Gets or sets the sort direction
    /// </summary>
    public string SortDirection { get; set; }
    
    /// <summary>
    /// Gets or sets the filters
    /// </summary>
    public Dictionary<string, string> Filters { get; set; }
}
