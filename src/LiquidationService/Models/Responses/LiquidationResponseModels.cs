using LiquidationService.Data.Entities;

namespace LiquidationService.Models.Responses;

/// <summary>
/// Response model for liquidation request details
/// </summary>
public class LiquidationRequestResponse
{
    /// <summary>
    /// Unique identifier for the liquidation request
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the user making the liquidation request
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Symbol of the asset to be liquidated
    /// </summary>
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the asset to be liquidated
    /// </summary>
    public decimal AssetAmount { get; set; }

    /// <summary>
    /// Type of output desired
    /// </summary>
    public LiquidationOutputType OutputType { get; set; }

    /// <summary>
    /// Symbol of the desired output currency/asset
    /// </summary>
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Type of destination for the liquidated funds
    /// </summary>
    public DestinationType DestinationType { get; set; }

    /// <summary>
    /// Destination address or account identifier
    /// </summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Additional destination details
    /// </summary>
    public string? DestinationDetails { get; set; }

    /// <summary>
    /// Current status of the liquidation request
    /// </summary>
    public LiquidationRequestStatus Status { get; set; }

    /// <summary>
    /// Market price of the asset at the time of request
    /// </summary>
    public decimal? MarketPriceAtRequest { get; set; }

    /// <summary>
    /// Estimated output amount based on market price
    /// </summary>
    public decimal? EstimatedOutputAmount { get; set; }

    /// <summary>
    /// Actual output amount after execution
    /// </summary>
    public decimal? ActualOutputAmount { get; set; }

    /// <summary>
    /// Fees charged for the liquidation
    /// </summary>
    public decimal? Fees { get; set; }

    /// <summary>
    /// Exchange rate used for the liquidation
    /// </summary>
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// ID of the assigned liquidity provider
    /// </summary>
    public Guid? LiquidityProviderId { get; set; }

    /// <summary>
    /// Liquidity provider details
    /// </summary>
    public LiquidityProviderSummaryResponse? LiquidityProvider { get; set; }

    /// <summary>
    /// KYC verification status
    /// </summary>
    public bool KycVerified { get; set; }

    /// <summary>
    /// Compliance check status
    /// </summary>
    public bool ComplianceApproved { get; set; }

    /// <summary>
    /// Asset eligibility verification status
    /// </summary>
    public bool AssetEligibilityVerified { get; set; }

    /// <summary>
    /// Risk level assessment
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Requires multi-signature approval
    /// </summary>
    public bool RequiresMultiSignature { get; set; }

    /// <summary>
    /// Multi-signature approval status
    /// </summary>
    public bool MultiSignatureApproved { get; set; }

    /// <summary>
    /// Blockchain transaction hash for the liquidation
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Reference to external payment/transfer transaction
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    /// <summary>
    /// Reason for rejection or failure
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the request was submitted
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the request was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the liquidation was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Date and time when the request expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Response model for liquidity provider details
/// </summary>
public class LiquidityProviderResponse
{
    /// <summary>
    /// Unique identifier for the liquidity provider
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID of the liquidity provider
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Business name or individual name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Business address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Country of operation
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the liquidity provider
    /// </summary>
    public LiquidityProviderStatus Status { get; set; }

    /// <summary>
    /// KYC verification status
    /// </summary>
    public bool KycVerified { get; set; }

    /// <summary>
    /// Date when KYC was completed
    /// </summary>
    public DateTime? KycCompletedAt { get; set; }

    /// <summary>
    /// Reserve verification status
    /// </summary>
    public bool ReserveVerified { get; set; }

    /// <summary>
    /// Date when reserves were last verified
    /// </summary>
    public DateTime? ReserveVerifiedAt { get; set; }

    /// <summary>
    /// Minimum transaction amount the provider accepts
    /// </summary>
    public decimal? MinimumTransactionAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount the provider accepts
    /// </summary>
    public decimal? MaximumTransactionAmount { get; set; }

    /// <summary>
    /// Supported asset symbols
    /// </summary>
    public string? SupportedAssets { get; set; }

    /// <summary>
    /// Supported output currencies
    /// </summary>
    public string? SupportedOutputCurrencies { get; set; }

    /// <summary>
    /// Fee percentage charged by the provider
    /// </summary>
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Provider's liquidity pool address
    /// </summary>
    public string? LiquidityPoolAddress { get; set; }

    /// <summary>
    /// Available liquidity amount
    /// </summary>
    public decimal? AvailableLiquidity { get; set; }

    /// <summary>
    /// Total liquidity provided to date
    /// </summary>
    public decimal TotalLiquidityProvided { get; set; }

    /// <summary>
    /// Total fees earned to date
    /// </summary>
    public decimal TotalFeesEarned { get; set; }

    /// <summary>
    /// Number of successful liquidations
    /// </summary>
    public int SuccessfulLiquidations { get; set; }

    /// <summary>
    /// Number of failed liquidations
    /// </summary>
    public int FailedLiquidations { get; set; }

    /// <summary>
    /// Average response time in minutes
    /// </summary>
    public double? AverageResponseTimeMinutes { get; set; }

    /// <summary>
    /// Provider rating (1-5 stars)
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Operating hours
    /// </summary>
    public string? OperatingHours { get; set; }

    /// <summary>
    /// Time zone of the provider
    /// </summary>
    public string? TimeZone { get; set; }

    /// <summary>
    /// Additional notes about the provider
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the provider was registered
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the provider was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Date and time when the provider was last active
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Date and time when the provider was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// Summary response model for liquidity provider
/// </summary>
public class LiquidityProviderSummaryResponse
{
    /// <summary>
    /// Unique identifier for the liquidity provider
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Business name or individual name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the liquidity provider
    /// </summary>
    public LiquidityProviderStatus Status { get; set; }

    /// <summary>
    /// Fee percentage charged by the provider
    /// </summary>
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Provider rating (1-5 stars)
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Average response time in minutes
    /// </summary>
    public double? AverageResponseTimeMinutes { get; set; }
}

/// <summary>
/// Response model for compliance check details
/// </summary>
public class ComplianceCheckResponse
{
    /// <summary>
    /// Unique identifier for the compliance check
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the associated liquidation request
    /// </summary>
    public Guid LiquidationRequestId { get; set; }

    /// <summary>
    /// Type of compliance check performed
    /// </summary>
    public ComplianceCheckType CheckType { get; set; }

    /// <summary>
    /// Result of the compliance check
    /// </summary>
    public ComplianceCheckResult Result { get; set; }

    /// <summary>
    /// External reference ID
    /// </summary>
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Provider that performed the check
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Risk score assigned by the check (0-100)
    /// </summary>
    public int? RiskScore { get; set; }

    /// <summary>
    /// Risk level determined by the check
    /// </summary>
    public RiskLevel? RiskLevel { get; set; }

    /// <summary>
    /// Reason for failure or requiring review
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Recommendations from the compliance check
    /// </summary>
    public string? Recommendations { get; set; }

    /// <summary>
    /// Whether manual review is required
    /// </summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// Date and time when manual review was completed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Manual review comments
    /// </summary>
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Whether the check was overridden by manual review
    /// </summary>
    public bool IsOverridden { get; set; }

    /// <summary>
    /// Date and time when the check was started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Date and time when the check was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Duration of the check in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Date and time when the compliance check was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the compliance check was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for asset eligibility details
/// </summary>
public class AssetEligibilityResponse
{
    /// <summary>
    /// Unique identifier for the asset eligibility record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Asset symbol
    /// </summary>
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the asset
    /// </summary>
    public string AssetName { get; set; } = string.Empty;

    /// <summary>
    /// Current eligibility status
    /// </summary>
    public AssetEligibilityStatus Status { get; set; }

    /// <summary>
    /// Whether the asset is currently enabled for liquidation
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Minimum amount that can be liquidated
    /// </summary>
    public decimal? MinimumLiquidationAmount { get; set; }

    /// <summary>
    /// Maximum amount that can be liquidated in a single transaction
    /// </summary>
    public decimal? MaximumLiquidationAmount { get; set; }

    /// <summary>
    /// Daily liquidation limit for this asset
    /// </summary>
    public decimal? DailyLiquidationLimit { get; set; }

    /// <summary>
    /// Monthly liquidation limit for this asset
    /// </summary>
    public decimal? MonthlyLiquidationLimit { get; set; }

    /// <summary>
    /// Lock-up period in days
    /// </summary>
    public int? LockupPeriodDays { get; set; }

    /// <summary>
    /// Minimum holding period before liquidation is allowed (in days)
    /// </summary>
    public int? MinimumHoldingPeriodDays { get; set; }

    /// <summary>
    /// Cooling-off period between liquidations (in hours)
    /// </summary>
    public int? CoolingOffPeriodHours { get; set; }

    /// <summary>
    /// Whether KYC verification is required for this asset
    /// </summary>
    public bool RequiresKyc { get; set; }

    /// <summary>
    /// Whether enhanced due diligence is required
    /// </summary>
    public bool RequiresEnhancedDueDiligence { get; set; }

    /// <summary>
    /// Whether multi-signature approval is required
    /// </summary>
    public bool RequiresMultiSignature { get; set; }

    /// <summary>
    /// Threshold amount above which multi-signature is required
    /// </summary>
    public decimal? MultiSignatureThreshold { get; set; }

    /// <summary>
    /// Risk level associated with this asset
    /// </summary>
    public RiskLevel RiskLevel { get; set; }

    /// <summary>
    /// Supported output currencies
    /// </summary>
    public string? SupportedOutputCurrencies { get; set; }

    /// <summary>
    /// Restricted countries
    /// </summary>
    public string? RestrictedCountries { get; set; }

    /// <summary>
    /// Fee percentage for liquidating this asset
    /// </summary>
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Fixed fee amount for liquidating this asset
    /// </summary>
    public decimal? FixedFee { get; set; }

    /// <summary>
    /// Estimated processing time in minutes
    /// </summary>
    public int? EstimatedProcessingTimeMinutes { get; set; }

    /// <summary>
    /// Whether this asset is a stablecoin
    /// </summary>
    public bool IsStablecoin { get; set; }

    /// <summary>
    /// Whether this asset is a privacy coin
    /// </summary>
    public bool IsPrivacyCoin { get; set; }

    /// <summary>
    /// Blockchain network the asset operates on
    /// </summary>
    public string? BlockchainNetwork { get; set; }

    /// <summary>
    /// Contract address (for tokens)
    /// </summary>
    public string? ContractAddress { get; set; }

    /// <summary>
    /// Date and time when the asset was first made eligible
    /// </summary>
    public DateTime? FirstEligibleAt { get; set; }

    /// <summary>
    /// Date and time when the asset was last reviewed
    /// </summary>
    public DateTime? LastReviewedAt { get; set; }

    /// <summary>
    /// Date and time when the next review is due
    /// </summary>
    public DateTime? NextReviewDue { get; set; }

    /// <summary>
    /// Date and time when the asset eligibility was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the asset eligibility was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Response model for market price snapshot
/// </summary>
public class MarketPriceSnapshotResponse
{
    /// <summary>
    /// Unique identifier for the price snapshot
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Asset symbol
    /// </summary>
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Output currency/asset symbol
    /// </summary>
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Current market price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Bid price
    /// </summary>
    public decimal? BidPrice { get; set; }

    /// <summary>
    /// Ask price
    /// </summary>
    public decimal? AskPrice { get; set; }

    /// <summary>
    /// 24-hour trading volume
    /// </summary>
    public decimal? Volume24h { get; set; }

    /// <summary>
    /// 24-hour price change percentage
    /// </summary>
    public decimal? Change24hPercent { get; set; }

    /// <summary>
    /// Source of the price data
    /// </summary>
    public string PriceSource { get; set; } = string.Empty;

    /// <summary>
    /// Exchange or platform where the price was obtained
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Confidence level of the price data (0-100)
    /// </summary>
    public int? ConfidenceLevel { get; set; }

    /// <summary>
    /// Whether this price is suitable for liquidation
    /// </summary>
    public bool IsSuitableForLiquidation { get; set; }

    /// <summary>
    /// Estimated slippage for large transactions
    /// </summary>
    public decimal? EstimatedSlippage { get; set; }

    /// <summary>
    /// Date and time when the price snapshot was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the price expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Paginated response model
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// List of items
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }
}
