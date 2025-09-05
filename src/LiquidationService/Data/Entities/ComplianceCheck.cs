using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LiquidationService.Data.Entities;

/// <summary>
/// Represents a compliance check performed on a liquidation request
/// </summary>
[Table("ComplianceChecks")]
[Index(nameof(LiquidationRequestId))]
[Index(nameof(CheckType))]
[Index(nameof(Result))]
[Index(nameof(CreatedAt))]
public class ComplianceCheck : ITimestampEntity
{
    /// <summary>
    /// Unique identifier for the compliance check
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the associated liquidation request
    /// </summary>
    [Required]
    public Guid LiquidationRequestId { get; set; }

    /// <summary>
    /// Type of compliance check performed
    /// </summary>
    [Required]
    public ComplianceCheckType CheckType { get; set; }

    /// <summary>
    /// Result of the compliance check
    /// </summary>
    [Required]
    public ComplianceCheckResult Result { get; set; } = ComplianceCheckResult.Pending;

    /// <summary>
    /// External reference ID (e.g., ComplyCube transaction ID)
    /// </summary>
    [MaxLength(100)]
    public string? ExternalReferenceId { get; set; }

    /// <summary>
    /// Provider that performed the check (e.g., ComplyCube)
    /// </summary>
    [MaxLength(100)]
    public string? Provider { get; set; }

    /// <summary>
    /// Risk score assigned by the check (0-100)
    /// </summary>
    [Range(0, 100)]
    public int? RiskScore { get; set; }

    /// <summary>
    /// Risk level determined by the check
    /// </summary>
    public RiskLevel? RiskLevel { get; set; }

    /// <summary>
    /// Detailed check results (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? CheckDetails { get; set; }

    /// <summary>
    /// Reason for failure or requiring review
    /// </summary>
    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Recommendations from the compliance check
    /// </summary>
    [MaxLength(2000)]
    public string? Recommendations { get; set; }

    /// <summary>
    /// Whether manual review is required
    /// </summary>
    public bool RequiresManualReview { get; set; } = false;

    /// <summary>
    /// ID of the user who performed manual review
    /// </summary>
    public Guid? ReviewedByUserId { get; set; }

    /// <summary>
    /// Date and time when manual review was completed
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Manual review comments
    /// </summary>
    [MaxLength(2000)]
    public string? ReviewComments { get; set; }

    /// <summary>
    /// Whether the check was overridden by manual review
    /// </summary>
    public bool IsOverridden { get; set; } = false;

    /// <summary>
    /// Original result before override
    /// </summary>
    public ComplianceCheckResult? OriginalResult { get; set; }

    /// <summary>
    /// Reason for override
    /// </summary>
    [MaxLength(1000)]
    public string? OverrideReason { get; set; }

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
    /// Number of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; } = 0;

    /// <summary>
    /// Maximum number of retries allowed
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Date and time when the next retry should be attempted
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Error message if the check failed due to technical issues
    /// </summary>
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata (JSON format)
    /// </summary>
    [Column(TypeName = "text")]
    public string? Metadata { get; set; }

    /// <summary>
    /// Date and time when the compliance check was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the compliance check was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the check expires (for time-sensitive checks)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Associated liquidation request
    /// </summary>
    [ForeignKey(nameof(LiquidationRequestId))]
    public LiquidationRequest? LiquidationRequest { get; set; }
}
