using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAPIGateway.Models.Search;
using MobileAPIGateway.Services;

namespace MobileAPIGateway.Controllers;

/// <summary>
/// Controller for search operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : BaseController
{
    private readonly ISearchService _searchService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchController"/> class
    /// </summary>
    /// <param name="searchService">The search service</param>
    public SearchController(ISearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
    }
    
    /// <summary>
    /// Searches for items
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="category">The search category</param>
    /// <param name="page">The page number</param>
    /// <param name="pageSize">The page size</param>
    /// <param name="sortBy">The sort field</param>
    /// <param name="sortDirection">The sort direction</param>
    /// <returns>The search response</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] string category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = null,
        [FromQuery] string sortDirection = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query is required");
        }
        
        var request = new SearchRequest
        {
            Query = query,
            Category = category,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        
        var response = await _searchService.SearchAsync(request);
        return Ok(response);
    }
    
    /// <summary>
    /// Gets search suggestions
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="category">The search category</param>
    /// <param name="limit">The maximum number of suggestions to return</param>
    /// <returns>The search suggestions</returns>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string query,
        [FromQuery] string category = null,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Search query is required");
        }
        
        var suggestions = await _searchService.GetSuggestionsAsync(query, category, limit);
        return Ok(suggestions);
    }
    
    /// <summary>
    /// Gets search categories
    /// </summary>
    /// <returns>The search categories</returns>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _searchService.GetCategoriesAsync();
        return Ok(categories);
    }
}
