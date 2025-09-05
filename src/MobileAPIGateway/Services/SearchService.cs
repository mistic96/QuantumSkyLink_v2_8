using MobileAPIGateway.Clients;
using MobileAPIGateway.Models.Search;

namespace MobileAPIGateway.Services;

/// <summary>
/// Service implementation for search operations
/// </summary>
public class SearchService : ISearchService
{
    private readonly ISearchClient _searchClient;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService"/> class
    /// </summary>
    /// <param name="searchClient">The search client</param>
    public SearchService(ISearchClient searchClient)
    {
        _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
    }
    
    /// <inheritdoc/>
    public async Task<SearchResponse> SearchAsync(SearchRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        return await _searchClient.SearchAsync(
            request.Query,
            request.Category,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection,
            request.Filters);
    }
    
    /// <inheritdoc/>
    public async Task<List<string>> GetSuggestionsAsync(string query, string category = null, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query cannot be null or empty", nameof(query));
        }
        
        return await _searchClient.GetSuggestionsAsync(query, category, limit);
    }
    
    /// <inheritdoc/>
    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _searchClient.GetCategoriesAsync();
    }
}
