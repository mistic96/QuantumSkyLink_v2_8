using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using GovernanceService.Data;

namespace GovernanceService.Data.Entities;

[Table("VotingDelegations")]
[Index(nameof(DelegatorId))]
[Index(nameof(DelegateId))]
[Index(nameof(IsActive))]
[Index(nameof(DelegatorId), nameof(DelegateId), nameof(SpecificType), IsUnique = true)]
public class VotingDelegation : ITimestampEntity
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey("Delegator")]
    public Guid DelegatorId { get; set; }

    [ForeignKey("Delegate")]
    public Guid DelegateId { get; set; }

    public ProposalType? SpecificType { get; set; }

    [MaxLength(1000)]
    public string? DelegationReason { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    [ForeignKey("RevokedBy")]
    public Guid? RevokedById { get; set; }

    [MaxLength(500)]
    public string? RevocationReason { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime UpdatedAt { get; set; }

    [MaxLength(500)]
    public string? TransactionHash { get; set; }

    // Delegation limits
    [Column(TypeName = "decimal(5,2)")]
    public decimal? MaxDelegationPercentage { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool AutoRenew { get; set; } = false;
}
