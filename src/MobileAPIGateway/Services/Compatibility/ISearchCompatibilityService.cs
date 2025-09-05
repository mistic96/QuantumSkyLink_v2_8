using MobileAPIGateway.Models.Compatibility.Search;

namespace MobileAPIGateway.Services.Compatibility;

/// <summary>
/// Interface for the search compatibility service
/// </summary>
public interface ISearchCompatibilityService
{
    /// <summary>
    /// Searches asynchronously
    /// </summary>
    /// <param name="request">The search request</param>
    /// <returns>The search response</returns>
    Task<SearchCompatibilityResponse> SearchAsync(SearchCompatibilityRequest request);
    
    /// <summary>
    /// Gets search suggestions asynchronously
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="maxSuggestions">The maximum number of suggestions to return</param>
    /// <returns>The list of search suggestions</returns>
    Task<List<string>> GetSuggestionsAsync(string query, int maxSuggestions = 10);
    
    /// <summary>
    /// Gets search categories asynchronously
    /// </summary>
    /// <returns>The list of search categories</returns>
    Task<List<string>> GetCategoriesAsync();
}
