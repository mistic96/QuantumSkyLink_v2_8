using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GovernanceService.Data;

namespace GovernanceService.Data.Entities;

[Table("Votes")]
[Index(nameof(ProposalId))]
[Index(nameof(VoterId))]
[Index(nameof(CastAt))]
[Index(nameof(ProposalId), nameof(VoterId), IsUnique = true)]
public class Vote : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("Proposal")]
    public Guid ProposalId { get; set; }

    [ForeignKey("Voter")]
    public Guid VoterId { get; set; }

    [Required]
    public VoteChoice Choice { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,8)")]
    public decimal VotingPower { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [Required]
    public DateTime CastAt { get; set; }

    [MaxLength(500)]
    public string? TransactionHash { get; set; }

    public bool IsDelegated { get; set; } = false;

    public Guid? DelegatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Proposal Proposal { get; set; } = null!;
}

public enum VoteChoice
{
    Approve = 1,
    Reject = 2,
    Abstain = 3
}
