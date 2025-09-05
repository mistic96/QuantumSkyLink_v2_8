using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GovernanceService.Data;

namespace GovernanceService.Data.Entities;

[Table("Proposals")]
[Index(nameof(CreatorId))]
[Index(nameof(Status))]
[Index(nameof(Type))]
[Index(nameof(VotingStartTime))]
[Index(nameof(VotingEndTime))]
public class Proposal : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ProposalType Type { get; set; }

    [Required]
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    [ForeignKey("Creator")]
    public Guid CreatorId { get; set; }

    [Required]
    public DateTime VotingStartTime { get; set; }

    [Required]
    public DateTime VotingEndTime { get; set; }

    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal QuorumPercentage { get; set; }

    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ApprovalThreshold { get; set; }

    [MaxLength(2000)]
    public string? ExecutionParameters { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? RequestedAmount { get; set; }

    [MaxLength(10)]
    public string? RequestedCurrency { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<ProposalExecution> Executions { get; set; } = new List<ProposalExecution>();
}

public enum ProposalType
{
    Constitutional = 1,
    Treasury = 2,
    Parameter = 3,
    Upgrade = 4,
    Emergency = 5,
    General = 6
}

public enum ProposalStatus
{
    Pending = 1,
    Active = 2,
    Approved = 3,
    Rejected = 4,
    Executed = 5,
    Cancelled = 6,
    Expired = 7
}
