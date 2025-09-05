using Refit;
using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Clients;

/// <summary>
/// Client interface for the Search service
/// </summary>
public interface ISearchClient
{
    /// <summary>
    /// Searches for items
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="category">The search category</param>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="sortBy">The sort field</param>
    /// <param name="sortDirection">The sort direction</param>
    /// <param name="filters">The filters</param>
    /// <returns>The search response</returns>
    [Get("/api/search")]
    Task<SearchResponse> SearchAsync(
        [Query] string query,
        [Query] string category = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        [Query] string sortBy = null,
        [Query] string sortDirection = null,
        [Query] Dictionary<string, string> filters = null);
    
    /// <summary>
    /// Gets search suggestions
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="category">The search category</param>
    /// <param name="limit">The maximum number of suggestions to return</param>
    /// <returns>The search suggestions</returns>
    [Get("/api/search/suggestions")]
    Task<List<string>> GetSuggestionsAsync(
        [Query] string query,
        [Query] string category = null,
        [Query] int limit = 10);
    
    /// <summary>
    /// Gets search categories
    /// </summary>
    /// <returns>The search categories</returns>
    [Get("/api/search/categories")]
    Task<List<string>> GetCategoriesAsync();
}
