using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("ReserveRequirements")]
[Index(nameof(AccountId))]
[Index(nameof(RequirementType))]
[Index(nameof(Status))]
[Index(nameof(EffectiveDate))]
[Index(nameof(Currency))]
public class ReserveRequirement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RequirementType { get; set; } = string.Empty; // Regulatory, Operational, Risk, Liquidity

    [Required]
    [MaxLength(100)]
    public string RequirementName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal RequiredAmount { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal? RequiredPercentage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? BaseAmount { get; set; } // Amount the percentage is calculated on

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentReserve { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Shortfall { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Excess { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Met, Shortfall, Excess, Suspended

    [Required]
    [MaxLength(20)]
    public string ComplianceStatus { get; set; } = string.Empty; // Compliant, NonCompliant, Warning

    public DateTime EffectiveDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime LastCalculated { get; set; }

    public DateTime? NextCalculation { get; set; }

    [MaxLength(50)]
    public string? CalculationFrequency { get; set; } // Daily, Weekly, Monthly, Quarterly

    [MaxLength(100)]
    public string? RegulatoryReference { get; set; }

    [MaxLength(100)]
    public string? Authority { get; set; } // Central Bank, SEC, CFTC, etc.

    [Column(TypeName = "decimal(5,4)")]
    public decimal? PenaltyRate { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? PenaltyAmount { get; set; }

    public bool AutoAdjust { get; set; } = true;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? BufferAmount { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal? BufferPercentage { get; set; }

    [MaxLength(50)]
    public string? AlertThreshold { get; set; } // Percentage or amount before alert

    public bool AlertsEnabled { get; set; } = true;

    public DateTime? LastAlertSent { get; set; }

    public int AlertCount { get; set; } = 0;

    [MaxLength(1000)]
    public string? CalculationMethod { get; set; } // JSON for complex calculation rules

    [MaxLength(1000)]
    public string? ComplianceNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(500)]
    public string? ReviewNotes { get; set; }

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON for additional data

    // Navigation Properties
    [ForeignKey(nameof(AccountId))]
    public TreasuryAccount Account { get; set; } = null!;
}
