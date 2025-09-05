using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GovernanceService.Data;

namespace GovernanceService.Data.Entities;

[Table("GovernanceRules")]
[Index(nameof(ApplicableType), IsUnique = true)]
[Index(nameof(IsActive))]
public class GovernanceRule : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ProposalType ApplicableType { get; set; }

    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal MinimumQuorum { get; set; }

    [Required]
    [Range(0, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal ApprovalThreshold { get; set; }

    [Required]
    public TimeSpan VotingPeriod { get; set; }

    [Required]
    public TimeSpan ExecutionDelay { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumTokensRequired { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ProposalDeposit { get; set; }

    public bool RequiresMultiSig { get; set; } = false;

    public int? RequiredSignatures { get; set; }

    public bool AllowDelegation { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }

    [ForeignKey("UpdatedBy")]
    public Guid? UpdatedById { get; set; }
}
