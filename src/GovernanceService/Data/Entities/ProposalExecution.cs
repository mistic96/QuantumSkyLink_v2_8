using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GovernanceService.Data;

namespace GovernanceService.Data.Entities;

[Table("ProposalExecutions")]
[Index(nameof(ProposalId))]
[Index(nameof(ExecutedAt))]
[Index(nameof(Status))]
public class ProposalExecution : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("Proposal")]
    public Guid ProposalId { get; set; }

    [Required]
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;

    [Required]
    public DateTime ScheduledAt { get; set; }

    public DateTime? ExecutedAt { get; set; }

    [ForeignKey("ExecutedBy")]
    public Guid? ExecutedById { get; set; }

    [MaxLength(2000)]
    public string? ExecutionParameters { get; set; }

    [MaxLength(5000)]
    public string? ExecutionResult { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(500)]
    public string? TransactionHash { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? GasUsed { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ExecutionCost { get; set; }

    public int RetryCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 3;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Proposal Proposal { get; set; } = null!;
}

public enum ExecutionStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Retrying = 6
}
