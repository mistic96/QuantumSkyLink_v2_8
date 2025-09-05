using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("FundAllocations")]
[Index(nameof(AllocationRuleId))]
[Index(nameof(SourceAccountId))]
[Index(nameof(TargetAccountId))]
[Index(nameof(Status))]
[Index(nameof(AllocationDate))]
public class FundAllocation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AllocationRuleId { get; set; }

    [Required]
    public Guid SourceAccountId { get; set; }

    [Required]
    public Guid TargetAccountId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,4)")]
    public decimal AllocationPercentage { get; set; }

    [Required]
    [MaxLength(50)]
    public string AllocationType { get; set; } = string.Empty; // Percentage, FixedAmount, Surplus, Deficit

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Pending, Approved, Executed, Failed, Cancelled

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    public DateTime AllocationDate { get; set; }

    public DateTime? ExecutedDate { get; set; }

    [MaxLength(1000)]
    public string? ExecutionDetails { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    public int Priority { get; set; } = 1;

    public bool IsRecurring { get; set; }

    [MaxLength(50)]
    public string? RecurrencePattern { get; set; } // Daily, Weekly, Monthly, Quarterly, Yearly

    public DateTime? NextAllocationDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(AllocationRuleId))]
    public AllocationRule AllocationRule { get; set; } = null!;

    [ForeignKey(nameof(SourceAccountId))]
    [InverseProperty("SourceAllocations")]
    public TreasuryAccount SourceAccount { get; set; } = null!;

    [ForeignKey(nameof(TargetAccountId))]
    [InverseProperty("TargetAllocations")]
    public TreasuryAccount TargetAccount { get; set; } = null!;

    public ICollection<TreasuryTransaction> Transactions { get; set; } = new List<TreasuryTransaction>();
}
