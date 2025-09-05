using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Market
{
    public class PriceHistoryPoint
    {
        public DateTime Timestamp { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal Volume { get; set; }
        
        public decimal High { get; set; }
        
        public decimal Low { get; set; }
    }

    public class ProductStatistics
    {
        public decimal AvgPrice { get; set; }
        
        public decimal MaxPrice { get; set; }
        
        public decimal MinPrice { get; set; }
        
        public decimal TotalVolume { get; set; }
    }

    public class ProductStatsResponse
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public List<PriceHistoryPoint> PriceHistory { get; set; } = new();
        
        public ProductStatistics Statistics { get; set; } = new();
    }

    public class ProductStatsDataResponse
    {
        public int Status { get; set; } = 200;
        public ProductStatsResponse Data { get; set; } = new();
    }

    public class MarketSearchRequest
    {
        [Required]
        public string SearchTerm { get; set; } = string.Empty;
        
        public MarketSearchFilters Filters { get; set; } = new();
        
        public PaginationRequest Pagination { get; set; } = new();
    }

    public class MarketSearchFilters
    {
        public PriceRange? PriceRange { get; set; }
        
        public MarketCapRange? MarketCapRange { get; set; }
        
        public List<string> Categories { get; set; } = new();
        
        public string SortBy { get; set; } = string.Empty;
        
        public string SortOrder { get; set; } = string.Empty;
    }

    public class PriceRange
    {
        public decimal Min { get; set; }
        
        public decimal Max { get; set; }
    }

    public class MarketCapRange
    {
        public decimal Min { get; set; }
        
        public decimal Max { get; set; }
    }

    public class PaginationRequest
    {
        public int Skip { get; set; }
        
        public int Take { get; set; }
    }

    public class MarketSearchResult
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public decimal MarketCap { get; set; }
        
        public decimal Volume24h { get; set; }
        
        public decimal Change24h { get; set; }
    }

    public class MarketSearchResponse
    {
        public List<MarketSearchResult> Results { get; set; } = new();
        
        public int TotalCount { get; set; }
        
        public bool HasMore { get; set; }
    }

    public class MarketSearchDataResponse
    {
        public int Status { get; set; } = 200;
        public MarketSearchResponse Data { get; set; } = new();
    }

    public class CreateListingRequest
    {
        [Required]
        public string Cryptocurrency { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        public decimal PricePerUnit { get; set; }
        
        public decimal TotalValue { get; set; }
        
        [Required]
        public string ListingType { get; set; } = string.Empty;
        
        public List<string> PaymentMethods { get; set; } = new();
        
        public string Description { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateListingResponse
    {
        [Required]
        public string ListingId { get; set; } = string.Empty;
        
        public bool Success { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class CreateListingDataResponse
    {
        public int Status { get; set; } = 200;
        public CreateListingResponse Data { get; set; } = new();
    }

    public class ListingDetailsResponse
    {
        [Required]
        public string ListingId { get; set; } = string.Empty;
        
        [Required]
        public string SellerId { get; set; } = string.Empty;
        
        [Required]
        public string Cryptocurrency { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        public decimal PricePerUnit { get; set; }
        
        public decimal TotalValue { get; set; }
        
        public List<string> PaymentMethods { get; set; } = new();
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime ExpiresAt { get; set; }
    }

    public class ListingDetailsDataResponse
    {
        public int Status { get; set; } = 200;
        public ListingDetailsResponse Data { get; set; } = new();
    }

    /// <summary>
    /// Response model for current market profile
    /// </summary>
    public class CurrentMarketProfileResponse
    {
        [Required]
        public string ProfileId { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public decimal SellerRating { get; set; }
        
        public int TotalSales { get; set; }
        
        public int CompletedTransactions { get; set; }
        
        [Required]
        public string VerificationStatus { get; set; } = string.Empty;
        
        public List<string> PaymentMethods { get; set; } = new();
        
        public List<string> SupportedCurrencies { get; set; } = new();
    }

    /// <summary>
    /// Standard response wrapper for current market profile
    /// </summary>
    public class CurrentMarketProfileDataResponse
    {
        public int Status { get; set; } = 200;
        public CurrentMarketProfileResponse Data { get; set; } = new();
    }

    /// <summary>
    /// Request model for creating market profile
    /// </summary>
    public class CreateMarketProfileRequest
    {
        [Required]
        public string ProfileName { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public List<string> PaymentMethods { get; set; } = new();
        
        public List<string> SupportedCurrencies { get; set; } = new();
        
        [Required]
        public string BusinessType { get; set; } = string.Empty;
        
        public List<string> VerificationDocuments { get; set; } = new();
    }

    /// <summary>
    /// Response model for creating market profile
    /// </summary>
    public class CreateMarketProfileResponse
    {
        [Required]
        public string ProfileId { get; set; } = string.Empty;
        
        public bool Success { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Standard response wrapper for creating market profile
    /// </summary>
    public class CreateMarketProfileDataResponse
    {
        public int Status { get; set; } = 200;
        public CreateMarketProfileResponse Data { get; set; } = new();
    }
}
