namespace MobileAPIGateway.Models.Compatibility.Search;

/// <summary>
/// Search compatibility request model for compatibility with the old MobileOrchestrator
/// </summary>
public class SearchCompatibilityRequest
{
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the page number
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the category
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Gets or sets the sort by field
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to sort in ascending order
    /// </summary>
    public bool SortAscending { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the filters
    /// </summary>
    public Dictionary<string, string>? Filters { get; set; }
}
