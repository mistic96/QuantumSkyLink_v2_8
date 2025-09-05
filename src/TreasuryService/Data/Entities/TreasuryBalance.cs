using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("TreasuryBalances")]
[Index(nameof(AccountId))]
[Index(nameof(BalanceDate))]
[Index(nameof(Currency))]
[Index(nameof(BalanceType))]
public class TreasuryBalance
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string BalanceType { get; set; } = string.Empty; // Opening, Closing, Intraday, RealTime

    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal AvailableBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal ReservedBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal PendingCredits { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal PendingDebits { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal DayChange { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal? DayChangePercentage { get; set; }

    public DateTime BalanceDate { get; set; }

    public DateTime AsOfTime { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? PreviousBalance { get; set; }

    public int TransactionCount { get; set; } = 0;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? HighestBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? LowestBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? AverageBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? InterestEarned { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? FeesCharged { get; set; }

    public bool IsReconciled { get; set; }

    public DateTime? ReconciledAt { get; set; }

    public Guid? ReconciledBy { get; set; }

    [MaxLength(500)]
    public string? ReconciliationNotes { get; set; }

    [MaxLength(100)]
    public string? ExternalBalanceId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ExternalBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? VarianceAmount { get; set; }

    [MaxLength(500)]
    public string? VarianceReason { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ChangeAmount { get; set; }

    [MaxLength(500)]
    public string? ChangeReason { get; set; }

    public Guid? TransactionId { get; set; }

    [MaxLength(100)]
    public string? ReconciliationReference { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON for additional data

    // Navigation Properties
    [ForeignKey(nameof(AccountId))]
    public TreasuryAccount Account { get; set; } = null!;
}
