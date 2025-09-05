using System.Collections.Generic;

namespace MobileAPIGateway.Models.Search;

/// <summary>
/// Represents a search response
/// </summary>
public class SearchResponse
{
    /// <summary>
    /// Gets or sets the search results
    /// </summary>
    public List<SearchResult> Results { get; set; } = new List<SearchResult>();
    
    /// <summary>
    /// Gets or sets the total number of results
    /// </summary>
    public int TotalResults { get; set; }
    
    /// <summary>
    /// Gets or sets the current page
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    public string Query { get; set; }
    
    /// <summary>
    /// Gets or sets the search category
    /// </summary>
    public string Category { get; set; }
    
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
    
    /// <summary>
    /// Gets or sets the facets
    /// </summary>
    public Dictionary<string, List<FacetValue>> Facets { get; set; }
}

/// <summary>
/// Represents a facet value
/// </summary>
public class FacetValue
{
    /// <summary>
    /// Gets or sets the facet value
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// Gets or sets the facet count
    /// </summary>
    public int Count { get; set; }
}
