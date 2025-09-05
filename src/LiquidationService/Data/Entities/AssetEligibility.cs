using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents asset eligibility rules and restrictions for liquidation
/// </summary>
[Table("AssetEligibilities")]
[Index(nameof(AssetSymbol), IsUnique = true)]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class AssetEligibility : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the asset eligibility record
    /// </summary>
    [Key]
    public Guid Id { get; set; }

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
    public AssetEligibilityStatus Status { get; set; } = AssetEligibilityStatus.Eligible;

    /// <summary>
    /// Whether the asset is currently enabled for liquidation
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Minimum amount that can be liquidated
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumLiquidationAmount { get; set; }

    /// <summary>
    /// Maximum amount that can be liquidated in a single transaction
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaximumLiquidationAmount { get; set; }

    /// <summary>
    /// Daily liquidation limit for this asset
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? DailyLiquidationLimit { get; set; }

    /// <summary>
    /// Monthly liquidation limit for this asset
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
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
    [Column(TypeName = "decimal(18,8)")]
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
    /// Allowed countries (comma-separated country codes, if restricted)
    /// </summary>
    [MaxLength(1000)]
    public string? AllowedCountries { get; set; }

    /// <summary>
    /// Fee percentage for liquidating this asset
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Fixed fee amount for liquidating this asset
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? FixedFee { get; set; }

    /// <summary>
    /// Estimated processing time in minutes
    /// </summary>
    public int? EstimatedProcessingTimeMinutes { get; set; }

    /// <summary>
    /// Maximum processing time in minutes before timeout
    /// </summary>
    public int? MaxProcessingTimeMinutes { get; set; }

    /// <summary>
    /// Whether this asset requires special handling
    /// </summary>
    public bool RequiresSpecialHandling { get; set; } = false;

    /// <summary>
    /// Special handling instructions
    /// </summary>
    [MaxLength(2000)]
    public string? SpecialHandlingInstructions { get; set; }

    /// <summary>
    /// Compliance notes for this asset
    /// </summary>
    [MaxLength(2000)]
    public string? ComplianceNotes { get; set; }

    /// <summary>
    /// Regulatory classification of the asset
    /// </summary>
    [MaxLength(100)]
    public string? RegulatoryClassification { get; set; }

    /// <summary>
    /// Whether this asset is considered a security
    /// </summary>
    public bool? IsSecurityToken { get; set; }

    /// <summary>
    /// Whether this asset is a stablecoin
    /// </summary>
    public bool IsStablecoin { get; set; } = false;

    /// <summary>
    /// Whether this asset is a privacy coin
    /// </summary>
    public bool IsPrivacyCoin { get; set; } = false;

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
    /// Number of decimal places for the asset
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// Date and time when the asset was first made eligible
    /// </summary>
    public DateTime? FirstEligibleAt { get; set; }

    /// <summary>
    /// Date and time when the asset was last reviewed
    /// </summary>
    public DateTime? LastReviewedAt { get; set; }

    /// <summary>
    /// ID of the user who last reviewed the asset
    /// </summary>
    public Guid? LastReviewedByUserId { get; set; }

    /// <summary>
    /// Date and time when the next review is due
    /// </summary>
    public DateTime? NextReviewDue { get; set; }

    /// <summary>
    /// Review frequency in days
    /// </summary>
    public int ReviewFrequencyDays { get; set; } = 90;

    /// <summary>
    /// Reason for current status
    /// </summary>
    [MaxLength(1000)]
    public string? StatusReason { get; set; }

    /// <summary>
    /// Date and time when status was last changed
    /// </summary>
    public DateTime? StatusChangedAt { get; set; }

    /// <summary>
    /// ID of the user who changed the status
    /// </summary>
    public Guid? StatusChangedByUserId { get; set; }

    /// <summary>
    /// Additional metadata (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Administrative notes
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the asset eligibility was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the asset eligibility was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the asset eligibility expires (if applicable)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
