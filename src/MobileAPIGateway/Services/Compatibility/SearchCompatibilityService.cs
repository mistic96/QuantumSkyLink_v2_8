//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.Search;
//using MobileAPIGateway.Models.Search;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the search compatibility service
///// </summary>
//public class SearchCompatibilityService : ISearchCompatibilityService
//{
//    private readonly ISearchService _searchService;
//    private readonly ILogger<SearchCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="SearchCompatibilityService"/> class
//    /// </summary>
//    /// <param name="searchService">The search service</param>
//    /// <param name="logger">The logger</param>
//    public SearchCompatibilityService(ISearchService searchService, ILogger<SearchCompatibilityService> logger)
//    {
//        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<SearchCompatibilityResponse> SearchAsync(SearchCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Searching for query: {Query}", request.Query);
            
//            // Map from compatibility request to new request model
//            var searchRequest = new SearchRequest
//            {
//                Query = request.Query,
//                Page = request.Page,
//                PageSize = request.PageSize,
//                Category = request.Category,
//                SortBy = request.SortBy,
//                SortAscending = request.SortAscending,
//                Filters = request.Filters
//            };
            
//            // Call the new service
//            var response = await _searchService.SearchAsync(searchRequest);
            
//            // Map from new response model to compatibility response
//            return new SearchCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Search completed successfully",
//                TotalCount = response.TotalCount,
//                Page = response.Page,
//                PageSize = response.PageSize,
//                TotalPages = response.TotalPages,
//                Results = response.Results?.Select(result => new SearchResultCompatibility
//                {
//                    Id = result.Id,
//                    Title = result.Title,
//                    Description = result.Description,
//                    Category = result.Category,
//                    Url = result.Url,
//                    ImageUrl = result.ImageUrl,
//                    Price = result.Price,
//                    Currency = result.Currency,
//                    Rating = result.Rating,
//                    ReviewCount = result.ReviewCount,
//                    Metadata = result.Metadata,
//                    CreatedDate = result.CreatedDate,
//                    LastUpdatedDate = result.LastUpdatedDate
//                }).ToList(),
//                Categories = response.Categories,
//                AvailableFilters = response.AvailableFilters
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error searching for query: {Query}", request.Query);
//            return new SearchCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error searching: {ex.Message}"
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<List<string>> GetSuggestionsAsync(string query, int maxSuggestions = 10)
//    {
//        try
//        {
//            _logger.LogInformation("Getting suggestions for query: {Query}", query);
            
//            // Call the new service
//            return await _searchService.GetSuggestionsAsync(query, maxSuggestions);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting suggestions for query: {Query}", query);
//            return new List<string>();
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<List<string>> GetCategoriesAsync()
//    {
//        try
//        {
//            _logger.LogInformation("Getting search categories");
            
//            // Call the new service
//            return await _searchService.GetCategoriesAsync();
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting search categories");
//            return new List<string>();
//        }
//    }
//}
