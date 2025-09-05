using Refit;

namespace OrchestrationService.Clients;

/// <summary>
/// Client interface for TreasuryService communication
/// Treasury operations with multi-signature approval
/// </summary>
public interface ITreasuryServiceClient
{
    [Post("/api/treasury/operations")]
    Task<TreasuryOperationResult> ExecuteTreasuryOperationAsync(
        [Body] TreasuryOperationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/api/treasury/operations/{operationId}/status")]
    Task<TreasuryOperationStatusResponse> GetTreasuryOperationStatusAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for NotificationService communication
/// Multi-channel notification delivery
/// </summary>
public interface INotificationServiceClient
{
    [Post("/api/notifications/send")]
    Task<NotificationResult> SendNotificationAsync(
        [Body] NotificationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for IdentityVerificationService communication
/// KYC and identity verification workflows
/// </summary>
public interface IIdentityVerificationServiceClient
{
    [Post("/api/kyc/basic")]
    Task<KycResult> PerformBasicKycAsync(
        [Body] BasicKycRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/kyc/enhanced")]
    Task<KycResult> PerformEnhancedKycAsync(
        [Body] EnhancedKycRequest request,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Client interface for MarketplaceService communication
/// Marketplace operations including listings, orders, escrow, and analytics
/// </summary>
public interface IMarketplaceServiceClient
{
    [Post("/api/listings")]
    Task<ListingCreationResult> CreateListingAsync(
        [Body] ListingCreationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/listings/{listingId}/validate")]
    Task<ListingValidationResult> ValidateListingAsync(
        string listingId,
        [Body] ListingValidationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/orders")]
    Task<OrderCreationResult> CreateOrderAsync(
        [Body] OrderCreationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/orders/{orderId}/verify")]
    Task<OrderVerificationResult> VerifyOrderAsync(
        string orderId,
        [Body] OrderVerificationRequest request,
        CancellationToken cancellationToken = default);

    [Put("/api/orders/{orderId}/status")]
    Task<OrderStatusUpdateResult> UpdateOrderStatusAsync(
        string orderId,
        [Body] OrderStatusUpdateRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/listings")]
    Task<AnalyticsDataResult> GetListingAnalyticsAsync(
        [Body] AnalyticsDataRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/orders")]
    Task<AnalyticsDataResult> GetOrderAnalyticsAsync(
        [Body] AnalyticsDataRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/calculate-trends")]
    Task<TrendCalculationResult> CalculateTrendsAsync(
        [Body] TrendCalculationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/volume-analysis")]
    Task<VolumeAnalysisResult> PerformVolumeAnalysisAsync(
        [Body] VolumeAnalysisRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/liquidity-analysis")]
    Task<LiquidityAnalysisResult> PerformLiquidityAnalysisAsync(
        [Body] LiquidityAnalysisRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/aggregate")]
    Task<AnalyticsAggregationResult> AggregateAnalyticsAsync(
        [Body] AnalyticsAggregationRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/cache/update")]
    Task<CacheUpdateResult> UpdateAnalyticsCacheAsync(
        [Body] CacheUpdateRequest request,
        CancellationToken cancellationToken = default);

    [Post("/api/analytics/report")]
    Task<ReportGenerationResult> GenerateReportAsync(
        [Body] ReportGenerationRequest request,
        CancellationToken cancellationToken = default);

    [Get("/health")]
    Task<object> GetHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for treasury operation
/// </summary>
public class TreasuryOperationRequest
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SignatureValidationId { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Result of treasury operation
/// </summary>
public class TreasuryOperationResult
{
    public string OperationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}

/// <summary>
/// Treasury operation status response
/// </summary>
public class TreasuryOperationStatusResponse
{
    public string OperationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Request for notification
/// </summary>
public class NotificationRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Result of notification
/// </summary>
public class NotificationResult
{
    public string NotificationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

/// <summary>
/// Request for basic KYC
/// </summary>
public class BasicKycRequest
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Documents { get; set; } = new();
}

/// <summary>
/// Request for enhanced KYC
/// </summary>
public class EnhancedKycRequest
{
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Documents { get; set; } = new();
    public Dictionary<string, object> AdditionalVerification { get; set; } = new();
}

/// <summary>
/// Result of KYC process
/// </summary>
public class KycResult
{
    public string KycId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
}

// MarketplaceService Models

/// <summary>
/// Request for listing creation
/// </summary>
public class ListingCreationRequest
{
    public string TokenId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal BasePrice { get; set; }
    public string PricingModel { get; set; } = string.Empty;
    public string ListingType { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
    public string ComplianceValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of listing creation
/// </summary>
public class ListingCreationResult
{
    public string ListingId { get; set; } = string.Empty;
    public string TokenId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}

/// <summary>
/// Request for listing validation
/// </summary>
public class ListingValidationRequest
{
    public decimal Quantity { get; set; }
    public string BuyerId { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of listing validation
/// </summary>
public class ListingValidationResult
{
    public bool IsValid { get; set; }
    public string ValidationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request for order creation
/// </summary>
public class OrderCreationRequest
{
    public string ListingId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string EscrowId { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
    public string ListingValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of order creation
/// </summary>
public class OrderCreationResult
{
    public string OrderId { get; set; } = string.Empty;
    public string ListingId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}

/// <summary>
/// Request for order verification
/// </summary>
public class OrderVerificationRequest
{
    public string EscrowId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of order verification
/// </summary>
public class OrderVerificationResult
{
    public bool IsValid { get; set; }
    public string ValidationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request for order status update
/// </summary>
public class OrderStatusUpdateRequest
{
    public string EscrowId { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string EscrowAction { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string SignatureValidationId { get; set; } = string.Empty;
}

/// <summary>
/// Result of order status update
/// </summary>
public class OrderStatusUpdateResult
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request for analytics data
/// </summary>
public class AnalyticsDataRequest
{
    public string TimeRange { get; set; } = string.Empty;
    public string AnalyticsType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
}

/// <summary>
/// Result of analytics data collection
/// </summary>
public class AnalyticsDataResult
{
    public bool Success { get; set; }
    public int RecordCount { get; set; }
    public object Data { get; set; } = new();
}

/// <summary>
/// Request for trend calculation
/// </summary>
public class TrendCalculationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object ListingData { get; set; } = new();
    public object OrderData { get; set; } = new();
    public object TokenData { get; set; } = new();
    public string TimeRange { get; set; } = string.Empty;
    public bool IncludePricing { get; set; }
}

/// <summary>
/// Result of trend calculation
/// </summary>
public class TrendCalculationResult
{
    public bool Success { get; set; }
    public object TrendData { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Request for volume analysis
/// </summary>
public class VolumeAnalysisRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object OrderData { get; set; } = new();
    public string TimeRange { get; set; } = string.Empty;
}

/// <summary>
/// Result of volume analysis
/// </summary>
public class VolumeAnalysisResult
{
    public bool Success { get; set; }
    public object VolumeData { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Request for liquidity analysis
/// </summary>
public class LiquidityAnalysisRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object ListingData { get; set; } = new();
    public object OrderData { get; set; } = new();
    public string TimeRange { get; set; } = string.Empty;
}

/// <summary>
/// Result of liquidity analysis
/// </summary>
public class LiquidityAnalysisResult
{
    public bool Success { get; set; }
    public object LiquidityData { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Request for analytics aggregation
/// </summary>
public class AnalyticsAggregationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object PriceData { get; set; } = new();
    public object VolumeData { get; set; } = new();
    public object LiquidityData { get; set; } = new();
    public object FeeData { get; set; } = new();
    public string AnalyticsType { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
}

/// <summary>
/// Result of analytics aggregation
/// </summary>
public class AnalyticsAggregationResult
{
    public bool Success { get; set; }
    public object AggregatedData { get; set; } = new();
    public int TotalDataPoints { get; set; }
    public DateTime AggregatedAt { get; set; }
}

/// <summary>
/// Request for cache update
/// </summary>
public class CacheUpdateRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object AnalyticsResults { get; set; } = new();
    public string AnalyticsType { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string CacheExpiry { get; set; } = string.Empty;
}

/// <summary>
/// Result of cache update
/// </summary>
public class CacheUpdateResult
{
    public bool Success { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Request for report generation
/// </summary>
public class ReportGenerationRequest
{
    public string RequestId { get; set; } = string.Empty;
    public object AggregatedData { get; set; } = new();
    public string AnalyticsType { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public bool IncludeCharts { get; set; }
    public bool IncludeRawData { get; set; }
}

/// <summary>
/// Result of report generation
/// </summary>
public class ReportGenerationResult
{
    public bool Success { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public object ReportData { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
