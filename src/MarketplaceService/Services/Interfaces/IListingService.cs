using MarketplaceService.Data.Entities;

namespace MarketplaceService.Services.Interfaces;

/// <summary>
/// Service interface for managing marketplace listings
/// </summary>
public interface IListingService
{
    /// <summary>
    /// Create a new marketplace listing
    /// </summary>
    /// <param name="request">Listing creation request</param>
    /// <param name="userId">User ID creating the listing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created listing details</returns>
    Task<MarketListingDetailDto> CreateListingAsync(CreateListingRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific listing by ID
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="userId">User ID requesting the listing (for access control)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Listing details or null if not found</returns>
    Task<MarketListingDetailDto?> GetListingAsync(Guid listingId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing listing
    /// </summary>
    /// <param name="listingId">Listing ID to update</param>
    /// <param name="request">Update request</param>
    /// <param name="userId">User ID performing the update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated listing details</returns>
    Task<MarketListingDetailDto> UpdateListingAsync(Guid listingId, UpdateListingRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete/cancel a listing
    /// </summary>
    /// <param name="listingId">Listing ID to delete</param>
    /// <param name="userId">User ID performing the deletion</param>
    /// <param name="reason">Reason for deletion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<bool> DeleteListingAsync(Guid listingId, Guid userId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get listings for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated user listings</returns>
    Task<PaginatedListingResponse> GetUserListingsAsync(Guid userId, ListingStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a draft listing
    /// </summary>
    /// <param name="listingId">Listing ID to activate</param>
    /// <param name="userId">User ID performing the activation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activated listing details</returns>
    Task<MarketListingDetailDto> ActivateListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause an active listing
    /// </summary>
    /// <param name="listingId">Listing ID to pause</param>
    /// <param name="userId">User ID performing the pause</param>
    /// <param name="reason">Reason for pausing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paused listing details</returns>
    Task<MarketListingDetailDto> PauseListingAsync(Guid listingId, Guid userId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a paused listing
    /// </summary>
    /// <param name="listingId">Listing ID to resume</param>
    /// <param name="userId">User ID performing the resume</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Resumed listing details</returns>
    Task<MarketListingDetailDto> ResumeListingAsync(Guid listingId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update listing quantity (for partial sales)
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="quantitySold">Quantity that was sold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated listing details</returns>
    Task<MarketListingDetailDto> UpdateListingQuantityAsync(Guid listingId, decimal quantitySold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get listing price history
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Price history records</returns>
    Task<IEnumerable<PriceHistoryDto>> GetListingPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment listing view count
    /// </summary>
    /// <param name="listingId">Listing ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result</returns>
    Task<bool> IncrementViewCountAsync(Guid listingId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer objects for listing service
/// </summary>
public class CreateListingRequest
{
    public Guid? TokenId { get; set; }
    public string? AssetSymbol { get; set; }
    public AssetType AssetType { get; set; }
    public MarketType MarketType { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal MinimumPurchaseQuantity { get; set; } = 1;
    public decimal? MaximumPurchaseQuantity { get; set; }
    public decimal? BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string PricingConfiguration { get; set; } = "{}";
    public string? Tags { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}

public class UpdateListingRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? BasePrice { get; set; }
    public string? PricingConfiguration { get; set; }
    public string? Tags { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}

public class MarketListingDetailDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public Guid? TokenId { get; set; }
    public string? AssetSymbol { get; set; }
    public AssetType AssetType { get; set; }
    public MarketType MarketType { get; set; }
    public ListingStatus Status { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal MinimumPurchaseQuantity { get; set; }
    public decimal? MaximumPurchaseQuantity { get; set; }
    public decimal? BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string PricingConfiguration { get; set; } = "{}";
    public decimal ListingFee { get; set; }
    public decimal CommissionPercentage { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long ViewCount { get; set; }
    public int OrderCount { get; set; }
    public decimal VolumeSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public string? Tags { get; set; }
    public string? ContactInfo { get; set; }
    public string? DocumentationUrl { get; set; }
    public string? RoadmapUrl { get; set; }
    public string? WhitepaperUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PriceHistoryDto
{
    public Guid Id { get; set; }
    public PricingStrategy PricingStrategy { get; set; }
    public decimal PricePerToken { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? MarketPrice { get; set; }
    public decimal? MarginAmount { get; set; }
    public decimal? MarginPercentage { get; set; }
    public string? ChangeReason { get; set; }
    public bool IsAutomaticUpdate { get; set; }
    public DateTime CreatedAt { get; set; }
}
