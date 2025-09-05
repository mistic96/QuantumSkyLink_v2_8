using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a registered liquidity provider
/// </summary>
[Table("LiquidityProviders")]
[Index(nameof(UserId), IsUnique = true)]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
public class LiquidityProvider : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the liquidity provider
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// User ID of the liquidity provider
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

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
    /// Current status of the liquidity provider
    /// </summary>
    [Required]
    public LiquidityProviderStatus Status { get; set; } = LiquidityProviderStatus.Pending;

    /// <summary>
    /// KYC verification status
    /// </summary>
    public bool KycVerified { get; set; } = false;

    /// <summary>
    /// Date when KYC was completed
    /// </summary>
    public DateTime? KycCompletedAt { get; set; }

    /// <summary>
    /// Reserve verification status
    /// </summary>
    public bool ReserveVerified { get; set; } = false;

    /// <summary>
    /// Date when reserves were last verified
    /// </summary>
    public DateTime? ReserveVerifiedAt { get; set; }

    /// <summary>
    /// Minimum transaction amount the provider accepts
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumTransactionAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount the provider accepts
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
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
    [Column(TypeName = "decimal(5,4)")]
    public decimal? FeePercentage { get; set; }

    /// <summary>
    /// Provider's liquidity pool address
    /// </summary>
    [MaxLength(500)]
    public string? LiquidityPoolAddress { get; set; }

    /// <summary>
    /// Available liquidity amount
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? AvailableLiquidity { get; set; }

    /// <summary>
    /// Total liquidity provided to date
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalLiquidityProvided { get; set; } = 0;

    /// <summary>
    /// Total fees earned to date
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalFeesEarned { get; set; } = 0;

    /// <summary>
    /// Number of successful liquidations
    /// </summary>
    public int SuccessfulLiquidations { get; set; } = 0;

    /// <summary>
    /// Number of failed liquidations
    /// </summary>
    public int FailedLiquidations { get; set; } = 0;

    /// <summary>
    /// Average response time in minutes
    /// </summary>
    public double? AverageResponseTimeMinutes { get; set; }

    /// <summary>
    /// Provider rating (1-5 stars)
    /// </summary>
    [Range(1, 5)]
    public decimal? Rating { get; set; }

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

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

    /// <summary>
    /// Date and time when the provider was registered
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the provider was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the provider was last active
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Date and time when the provider was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Date and time when the provider was suspended (if applicable)
    /// </summary>
    public DateTime? SuspendedAt { get; set; }

    /// <summary>
    /// Reason for suspension or rejection
    /// </summary>
    [MaxLength(1000)]
    public string? SuspensionReason { get; set; }
}
