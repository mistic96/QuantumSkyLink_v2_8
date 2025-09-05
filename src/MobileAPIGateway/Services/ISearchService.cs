using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service interface for search operations
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches for items
    /// </summary>
    /// <param name="request">The search request</param>
    /// <returns>The search response</returns>
    Task<SearchResponse> SearchAsync(SearchRequest request);
    
    /// <summary>
    /// Gets search suggestions
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="category">The search category</param>
    /// <param name="limit">The maximum number of suggestions to return</param>
    /// <returns>The search suggestions</returns>
    Task<List<string>> GetSuggestionsAsync(string query, string category = null, int limit = 10);
    
    /// <summary>
    /// Gets search categories
    /// </summary>
    /// <returns>The search categories</returns>
    Task<List<string>> GetCategoriesAsync();
}
