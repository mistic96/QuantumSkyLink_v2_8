using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("AllocationRules")]
[Index(nameof(RuleName), IsUnique = true)]
[Index(nameof(RuleType))]
[Index(nameof(Status))]
[Index(nameof(Priority))]
public class AllocationRule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RuleType { get; set; } = string.Empty; // Percentage, FixedAmount, Threshold, Surplus

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Active, Inactive, Suspended

    public int Priority { get; set; } = 1;

    [Column(TypeName = "decimal(5,4)")]
    public decimal? DefaultPercentage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? FixedAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ThresholdAmount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TriggerCondition { get; set; } // BalanceExceeds, BalanceBelow, TimeInterval, Manual

    [Column(TypeName = "decimal(18,8)")]
    public decimal? TriggerValue { get; set; }

    [MaxLength(50)]
    public string? RecurrencePattern { get; set; } // Daily, Weekly, Monthly, Quarterly, Yearly

    public int? RecurrenceInterval { get; set; } = 1;

    public TimeOnly? ExecutionTime { get; set; }

    public bool RequiresApproval { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [MaxLength(1000)]
    public string? Conditions { get; set; } // JSON string for complex conditions

    [MaxLength(1000)]
    public string? Actions { get; set; } // JSON string for actions to execute

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? LastExecuted { get; set; }

    public DateTime? NextExecution { get; set; }

    public int ExecutionCount { get; set; } = 0;

    public int? MaxExecutions { get; set; }

    // Navigation Properties
    public ICollection<FundAllocation> FundAllocations { get; set; } = new List<FundAllocation>();
}
