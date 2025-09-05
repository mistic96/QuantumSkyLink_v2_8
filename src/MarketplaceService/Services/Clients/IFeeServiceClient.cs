using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for FeeService integration
/// </summary>
public interface IFeeServiceClient
{
    /// <summary>
    /// Calculate marketplace listing fee
    /// </summary>
    [Post("/api/fees/calculate/listing")]
    Task<FeeCalculationResponse> CalculateListingFeeAsync(
        [Body] CalculateListingFeeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate marketplace transaction fee
    /// </summary>
    [Post("/api/fees/calculate/transaction")]
    Task<FeeCalculationResponse> CalculateTransactionFeeAsync(
        [Body] CalculateTransactionFeeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate marketplace commission
    /// </summary>
    [Post("/api/fees/calculate/commission")]
    Task<FeeCalculationResponse> CalculateCommissionAsync(
        [Body] CalculateCommissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process fee collection for marketplace transaction
    /// </summary>
    [Post("/api/fees/collect")]
    Task<FeeCollectionResponse> CollectFeesAsync(
        [Body] CollectFeesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fee structure for marketplace operations
    /// </summary>
    [Get("/api/fees/structure/marketplace")]
    Task<MarketplaceFeeStructureResponse> GetMarketplaceFeeStructureAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's fee tier information
    /// </summary>
    [Get("/api/fees/tier/{userId}")]
    Task<UserFeeTierResponse> GetUserFeeTierAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Distribute collected fees to stakeholders
    /// </summary>
    [Post("/api/fees/distribute")]
    Task<FeeDistributionResponse> DistributeFeesAsync(
        [Body] DistributeFeesRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request models for FeeService integration
/// </summary>
public class CalculateListingFeeRequest
{
    public Guid UserId { get; set; }
    public string AssetType { get; set; } = string.Empty; // "PlatformToken", "ExternalCrypto"
    public string MarketType { get; set; } = string.Empty; // "Primary", "Secondary"
    public decimal ListingValue { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsFeatured { get; set; }
    public int ListingDurationDays { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CalculateTransactionFeeRequest
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public decimal TransactionAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string AssetType { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CalculateCommissionRequest
{
    public Guid SellerId { get; set; }
    public decimal SaleAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string AssetType { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public decimal VolumeThisMonth { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CollectFeesRequest
{
    public Guid TransactionId { get; set; }
    public Guid PayerId { get; set; }
    public List<FeeItem> Fees { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}

public class DistributeFeesRequest
{
    public Guid TransactionId { get; set; }
    public List<FeeDistributionItem> Distributions { get; set; } = new();
    public Dictionary<string, object>? Metadata { get; set; }
}

public class FeeItem
{
    public string FeeType { get; set; } = string.Empty; // "Listing", "Transaction", "Commission"
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
}

public class FeeDistributionItem
{
    public Guid RecipientId { get; set; }
    public string RecipientType { get; set; } = string.Empty; // "Platform", "Referrer", "Stakeholder"
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Response models for FeeService integration
/// </summary>
public class FeeCalculationResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal TotalFee { get; set; }
    public string Currency { get; set; } = "USD";
    public List<FeeBreakdown> FeeBreakdown { get; set; } = new();
    public string FeeTier { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class FeeBreakdown
{
    public string FeeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsDiscounted { get; set; }
    public decimal OriginalAmount { get; set; }
}

public class FeeCollectionResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid FeeCollectionId { get; set; }
    public decimal TotalCollected { get; set; }
    public string Currency { get; set; } = "USD";
    public List<CollectedFeeItem> CollectedFees { get; set; } = new();
    public DateTime CollectedAt { get; set; }
}

public class CollectedFeeItem
{
    public string FeeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // "Collected", "Failed", "Pending"
    public string? TransactionId { get; set; }
}

public class MarketplaceFeeStructureResponse
{
    public List<FeeStructureItem> ListingFees { get; set; } = new();
    public List<FeeStructureItem> TransactionFees { get; set; } = new();
    public List<FeeStructureItem> CommissionRates { get; set; } = new();
    public List<FeeTierInfo> FeeTiers { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class FeeStructureItem
{
    public string Category { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string MarketType { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public string RateType { get; set; } = string.Empty; // "Percentage", "Fixed"
    public decimal MinimumFee { get; set; }
    public decimal MaximumFee { get; set; }
    public string Currency { get; set; } = "USD";
}

public class FeeTierInfo
{
    public string TierName { get; set; } = string.Empty;
    public decimal MinimumVolume { get; set; }
    public decimal DiscountPercentage { get; set; }
    public List<string> Benefits { get; set; } = new();
}

public class UserFeeTierResponse
{
    public Guid UserId { get; set; }
    public string CurrentTier { get; set; } = string.Empty;
    public decimal CurrentMonthVolume { get; set; }
    public decimal LifetimeVolume { get; set; }
    public decimal CurrentDiscountPercentage { get; set; }
    public string? NextTier { get; set; }
    public decimal VolumeToNextTier { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class FeeDistributionResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DistributionId { get; set; }
    public decimal TotalDistributed { get; set; }
    public string Currency { get; set; } = "USD";
    public List<DistributionResult> Distributions { get; set; } = new();
    public DateTime DistributedAt { get; set; }
}

public class DistributionResult
{
    public Guid RecipientId { get; set; }
    public string RecipientType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // "Distributed", "Failed", "Pending"
    public string? TransactionId { get; set; }
}
