using System.ComponentModel.DataAnnotations;
using LiquidationService.Data.Entities;

namespace LiquidationService.Models.Requests;

/// <summary>
/// Request model for creating a new liquidation request
/// </summary>
public class CreateLiquidationRequestModel
{
    /// <summary>
    /// Symbol of the asset to be liquidated (e.g., BTC, ETH)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the asset to be liquidated
    /// </summary>
    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Asset amount must be greater than 0")]
    public decimal AssetAmount { get; set; }

    /// <summary>
    /// Type of output desired (Fiat, Stablecoin, Cryptocurrency)
    /// </summary>
    [Required]
    public LiquidationOutputType OutputType { get; set; }

    /// <summary>
    /// Symbol of the desired output currency/asset
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string OutputSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Type of destination for the liquidated funds
    /// </summary>
    [Required]
    public DestinationType DestinationType { get; set; }

    /// <summary>
    /// Destination address or account identifier
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Additional destination details (e.g., bank account details)
    /// </summary>
    [MaxLength(1000)]
    public string? DestinationDetails { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for updating liquidation request status
/// </summary>
public class UpdateLiquidationStatusModel
{
    /// <summary>
    /// New status for the liquidation request
    /// </summary>
    [Required]
    public LiquidationRequestStatus Status { get; set; }

    /// <summary>
    /// Reason for the status change
    /// </summary>
    [MaxLength(1000)]
    public string? Reason { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for liquidation request filters
/// </summary>
public class LiquidationRequestFilterModel
{
    /// <summary>
    /// Filter by user ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Filter by asset symbol
    /// </summary>
    [MaxLength(20)]
    public string? AssetSymbol { get; set; }

    /// <summary>
    /// Filter by output symbol
    /// </summary>
    [MaxLength(20)]
    public string? OutputSymbol { get; set; }

    /// <summary>
    /// Filter by status
    /// </summary>
    public LiquidationRequestStatus? Status { get; set; }

    /// <summary>
    /// Filter by liquidity provider
    /// </summary>
    public Guid? LiquidityProviderId { get; set; }

    /// <summary>
    /// Filter by minimum amount
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Filter by maximum amount
    /// </summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Filter by creation date from
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Filter by creation date to
    /// </summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Page number for pagination
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string? SortDirection { get; set; } = "desc";
}

/// <summary>
/// Request model for registering a liquidity provider
/// </summary>
public class RegisterLiquidityProviderModel
{
    /// <summary>
    /// Business name or individual name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number
    /// </summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Business address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Country of operation
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Minimum transaction amount the provider accepts
    /// </summary>
    public decimal? MinimumTransactionAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount the provider accepts
    /// </summary>
    public decimal? MaximumTransactionAmount { get; set; }

    /// <summary>
    /// Supported asset symbols (comma-separated)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedAssets { get; set; }

    /// <summary>
    /// Supported output currencies (comma-separated)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedOutputCurrencies { get; set; }

    /// <summary>
    /// Fee percentage charged by the provider
    /// </summary>
    [Range(0, 100)]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Provider's liquidity pool address
    /// </summary>
    [MaxLength(500)]
    public string? LiquidityPoolAddress { get; set; }

    /// <summary>
    /// Operating hours (JSON format)
    /// </summary>
    [MaxLength(1000)]
    public string? OperatingHours { get; set; }

    /// <summary>
    /// Time zone of the provider
    /// </summary>
    [MaxLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Additional notes about the provider
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for updating liquidity provider information
/// </summary>
public class UpdateLiquidityProviderModel
{
    /// <summary>
    /// Business name or individual name
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Business address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Minimum transaction amount the provider accepts
    /// </summary>
    public decimal? MinimumTransactionAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount the provider accepts
    /// </summary>
    public decimal? MaximumTransactionAmount { get; set; }

    /// <summary>
    /// Supported asset symbols (comma-separated)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedAssets { get; set; }

    /// <summary>
    /// Supported output currencies (comma-separated)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedOutputCurrencies { get; set; }

    /// <summary>
    /// Fee percentage charged by the provider
    /// </summary>
    [Range(0, 100)]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Provider's liquidity pool address
    /// </summary>
    [MaxLength(500)]
    public string? LiquidityPoolAddress { get; set; }

    /// <summary>
    /// Available liquidity amount
    /// </summary>
    public decimal? AvailableLiquidity { get; set; }

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool? IsAvailable { get; set; }

    /// <summary>
    /// Operating hours (JSON format)
    /// </summary>
    [MaxLength(1000)]
    public string? OperatingHours { get; set; }

    /// <summary>
    /// Time zone of the provider
    /// </summary>
    [MaxLength(100)]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Additional notes about the provider
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for asset eligibility configuration
/// </summary>
public class ConfigureAssetEligibilityModel
{
    /// <summary>
    /// Asset symbol (e.g., BTC, ETH)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AssetSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the asset
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string AssetName { get; set; } = string.Empty;

    /// <summary>
    /// Current eligibility status
    /// </summary>
    [Required]
    public AssetEligibilityStatus Status { get; set; }

    /// <summary>
    /// Whether the asset is currently enabled for liquidation
    /// </summary>
    public bool IsEnabled { get; set; } = true;

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
    /// Lock-up period in days (if applicable)
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
    public bool RequiresKyc { get; set; } = true;

    /// <summary>
    /// Whether enhanced due diligence is required
    /// </summary>
    public bool RequiresEnhancedDueDiligence { get; set; } = false;

    /// <summary>
    /// Whether multi-signature approval is required
    /// </summary>
    public bool RequiresMultiSignature { get; set; } = false;

    /// <summary>
    /// Threshold amount above which multi-signature is required
    /// </summary>
    public decimal? MultiSignatureThreshold { get; set; }

    /// <summary>
    /// Risk level associated with this asset
    /// </summary>
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;

    /// <summary>
    /// Supported output currencies (comma-separated)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedOutputCurrencies { get; set; }

    /// <summary>
    /// Restricted countries (comma-separated country codes)
    /// </summary>
    [MaxLength(1000)]
    public string? RestrictedCountries { get; set; }

    /// <summary>
    /// Fee percentage for liquidating this asset
    /// </summary>
    [Range(0, 100)]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Fixed fee amount for liquidating this asset
    /// </summary>
    public decimal? FixedFee { get; set; }

    /// <summary>
    /// Blockchain network the asset operates on
    /// </summary>
    [MaxLength(100)]
    public string? BlockchainNetwork { get; set; }

    /// <summary>
    /// Contract address (for tokens)
    /// </summary>
    [MaxLength(100)]
    public string? ContractAddress { get; set; }

    /// <summary>
    /// Whether this asset is a stablecoin
    /// </summary>
    public bool IsStablecoin { get; set; } = false;

    /// <summary>
    /// Whether this asset is a privacy coin
    /// </summary>
    public bool IsPrivacyCoin { get; set; } = false;

    /// <summary>
    /// Administrative notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
}
