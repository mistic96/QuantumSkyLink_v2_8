using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Core marketplace service interface for high-level marketplace operations
/// </summary>
public interface IMarketplaceService
{
    /// <summary>
    /// Get marketplace overview statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Marketplace overview data</returns>
    Task<MarketplaceOverviewDto> GetMarketplaceOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get featured listings for the marketplace homepage
    /// </summary>
    /// <param name="limit">Maximum number of featured listings to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of featured listings</returns>
    Task<IEnumerable<MarketListingDto>> GetFeaturedListingsAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search listings with advanced filtering and sorting
    /// </summary>
    /// <param name="searchRequest">Search criteria and filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated search results</returns>
    Task<PaginatedListingResponse> SearchListingsAsync(SearchListingsRequest searchRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get marketplace categories and their listing counts
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with counts</returns>
    Task<IEnumerable<MarketplaceCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trending listings based on recent activity
    /// </summary>
    /// <param name="limit">Maximum number of trending listings to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of trending listings</returns>
    Task<IEnumerable<MarketListingDto>> GetTrendingListingsAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recently added listings
    /// </summary>
    /// <param name="limit">Maximum number of recent listings to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of recent listings</returns>
    Task<IEnumerable<MarketListingDto>> GetRecentListingsAsync(int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get marketplace health status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Marketplace health information</returns>
    Task<MarketplaceHealthDto> GetMarketplaceHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for marketplace service
/// </summary>
public class MarketplaceOverviewDto
{
    public int TotalActiveListings { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalVolumeTraded { get; set; }
    public decimal Volume24h { get; set; }
    public int Orders24h { get; set; }
    public int PrimaryMarketListings { get; set; }
    public int SecondaryMarketListings { get; set; }
}

public class MarketListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MarketType MarketType { get; set; }
    public AssetType AssetType { get; set; }
    public string? AssetSymbol { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal? BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public PricingStrategy PricingStrategy { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVerified { get; set; }
    public long ViewCount { get; set; }
    public int OrderCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Tags { get; set; }
}

public class SearchListingsRequest
{
    public string? SearchTerm { get; set; }
    public MarketType? MarketType { get; set; }
    public AssetType? AssetType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Currency { get; set; }
    public PricingStrategy? PricingStrategy { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsVerified { get; set; }
    public string? Tags { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "DESC";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PaginatedListingResponse
{
    public IEnumerable<MarketListingDto> Listings { get; set; } = new List<MarketListingDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class MarketplaceCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ListingCount { get; set; }
    public AssetType AssetType { get; set; }
}

public class MarketplaceHealthDto
{
    public bool IsHealthy { get; set; }
    public int ActiveListings { get; set; }
    public int PendingOrders { get; set; }
    public int ActiveEscrows { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? Issues { get; set; }
}
