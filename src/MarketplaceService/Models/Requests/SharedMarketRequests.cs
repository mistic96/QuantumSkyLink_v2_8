using System.ComponentModel.DataAnnotations;
using MarketplaceService.Data.Entities;

namespace MarketplaceService.Models.Requests;

/// <summary>
/// Request to search across all marketplace listings
/// </summary>
public class SearchMarketplaceRequest
{
    /// <summary>
    /// Search query for title/description
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Market type filter (Primary/Secondary)
    /// </summary>
    public MarketType? MarketType { get; set; }

    /// <summary>
    /// Asset type filter
    /// </summary>
    public AssetType? AssetType { get; set; }

    /// <summary>
    /// Asset identifier filter (token ID or symbol)
    /// </summary>
    public string? AssetIdentifier { get; set; }

    /// <summary>
    /// Pricing strategy filter
    /// </summary>
    public PricingStrategy? PricingStrategy { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Currency filter
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Tags filter (comma-separated)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Listing status filter
    /// </summary>
    public ListingStatus? Status { get; set; }

    /// <summary>
    /// Seller ID filter
    /// </summary>
    public Guid? SellerId { get; set; }

    /// <summary>
    /// Date range filter - from date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Date range filter - to date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "desc";

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    [Range(1, 100)]
    public int? PageSize { get; set; } = 20;
}

/// <summary>
/// Request to get marketplace analytics
/// </summary>
public class GetMarketplaceAnalyticsRequest
{
    /// <summary>
    /// Market type filter (Primary/Secondary)
    /// </summary>
    public MarketType? MarketType { get; set; }

    /// <summary>
    /// Asset type filter
    /// </summary>
    public AssetType? AssetType { get; set; }

    /// <summary>
    /// Asset identifier filter
    /// </summary>
    public string? AssetIdentifier { get; set; }

    /// <summary>
    /// Time period for analytics (1h, 24h, 7d, 30d)
    /// </summary>
    public string TimePeriod { get; set; } = "24h";

    /// <summary>
    /// Start date for custom time range
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for custom time range
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Include price history data
    /// </summary>
    public bool IncludePriceHistory { get; set; } = true;

    /// <summary>
    /// Include volume history data
    /// </summary>
    public bool IncludeVolumeHistory { get; set; } = true;

    /// <summary>
    /// Include order book data
    /// </summary>
    public bool IncludeOrderBook { get; set; } = false;
}

/// <summary>
/// Request to manage escrow for an order
/// </summary>
public class EscrowManagementRequest
{
    /// <summary>
    /// Order ID for escrow management
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Escrow action to perform
    /// </summary>
    [Required]
    public EscrowAction Action { get; set; }

    /// <summary>
    /// Reason for the action
    /// </summary>
    [StringLength(1000)]
    public string? Reason { get; set; }

    /// <summary>
    /// Additional metadata for the action
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Request to get user's marketplace activity
/// </summary>
public class GetUserMarketplaceActivityRequest
{
    /// <summary>
    /// User ID (optional, defaults to current user)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Activity type filter
    /// </summary>
    public MarketplaceActivityType? ActivityType { get; set; }

    /// <summary>
    /// Market type filter
    /// </summary>
    public MarketType? MarketType { get; set; }

    /// <summary>
    /// Date range filter - from date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Date range filter - to date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    [Range(1, 100)]
    public int? PageSize { get; set; } = 20;
}

/// <summary>
/// Request to get pricing information
/// </summary>
public class GetPricingRequest
{
    /// <summary>
    /// Listing ID
    /// </summary>
    [Required]
    public Guid ListingId { get; set; }

    /// <summary>
    /// Quantity for pricing calculation
    /// </summary>
    [Required]
    [Range(0.000001, double.MaxValue)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Include fee breakdown
    /// </summary>
    public bool IncludeFees { get; set; } = true;

    /// <summary>
    /// Include escrow details
    /// </summary>
    public bool IncludeEscrowDetails { get; set; } = false;
}

/// <summary>
/// Request to report a listing or user
/// </summary>
public class ReportListingRequest
{
    /// <summary>
    /// Listing ID being reported
    /// </summary>
    [Required]
    public Guid ListingId { get; set; }

    /// <summary>
    /// Reason for the report
    /// </summary>
    [Required]
    public ReportReason Reason { get; set; }

    /// <summary>
    /// Additional details about the report
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Supporting evidence (URLs, file references, etc.)
    /// </summary>
    public List<string>? Evidence { get; set; }
}

/// <summary>
/// Escrow actions that can be performed
/// </summary>
public enum EscrowAction
{
    Create = 1,
    Lock = 2,
    Release = 3,
    Cancel = 4,
    Dispute = 5,
    Resolve = 6
}

/// <summary>
/// Types of marketplace activities
/// </summary>
public enum MarketplaceActivityType
{
    ListingCreated = 1,
    ListingUpdated = 2,
    ListingActivated = 3,
    ListingDeactivated = 4,
    OrderCreated = 5,
    OrderCompleted = 6,
    OrderCancelled = 7,
    PaymentReceived = 8,
    PaymentSent = 9,
    EscrowCreated = 10,
    EscrowReleased = 11
}

/// <summary>
/// Reasons for reporting listings
/// </summary>
public enum ReportReason
{
    Fraud = 1,
    Scam = 2,
    FakeAsset = 3,
    InappropriateContent = 4,
    PriceManipulation = 5,
    Spam = 6,
    ViolatesTerms = 7,
    Other = 8
}
