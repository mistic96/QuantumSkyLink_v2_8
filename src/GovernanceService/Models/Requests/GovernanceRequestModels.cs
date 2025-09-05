using System.ComponentModel.DataAnnotations;
using GovernanceService.Data.Entities;

namespace GovernanceService.Models.Requests;

public class CreateProposalRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public ProposalType Type { get; set; }

    [Required]
    public Guid CreatorId { get; set; }

    [MaxLength(2000)]
    public string? ExecutionParameters { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? RequestedAmount { get; set; }

    [MaxLength(10)]
    public string? RequestedCurrency { get; set; }

    public DateTime? CustomVotingStartTime { get; set; }

    public DateTime? CustomVotingEndTime { get; set; }

    [Range(0, 100)]
    public decimal? CustomQuorumPercentage { get; set; }

    [Range(0, 100)]
    public decimal? CustomApprovalThreshold { get; set; }
}

public class UpdateProposalRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(2000)]
    public string? ExecutionParameters { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? RequestedAmount { get; set; }

    [MaxLength(10)]
    public string? RequestedCurrency { get; set; }

    public ProposalStatus? Status { get; set; }
}

public class CastVoteRequest
{
    [Required]
    public Guid ProposalId { get; set; }

    [Required]
    public Guid VoterId { get; set; }

    [Required]
    public VoteChoice Choice { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CustomVotingPower { get; set; }
}

public class CreateGovernanceRuleRequest
{
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
    public decimal MinimumQuorum { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal ApprovalThreshold { get; set; }

    [Required]
    public TimeSpan VotingPeriod { get; set; }

    [Required]
    public TimeSpan ExecutionDelay { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumTokensRequired { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ProposalDeposit { get; set; }

    public bool RequiresMultiSig { get; set; } = false;

    [Range(1, 20)]
    public int? RequiredSignatures { get; set; }

    public bool AllowDelegation { get; set; } = true;

    [Required]
    public Guid CreatedById { get; set; }
}

public class UpdateGovernanceRuleRequest
{
    [MaxLength(100)]
    public string? RuleName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 100)]
    public decimal? MinimumQuorum { get; set; }

    [Range(0, 100)]
    public decimal? ApprovalThreshold { get; set; }

    public TimeSpan? VotingPeriod { get; set; }

    public TimeSpan? ExecutionDelay { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinimumTokensRequired { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ProposalDeposit { get; set; }

    public bool? RequiresMultiSig { get; set; }

    [Range(1, 20)]
    public int? RequiredSignatures { get; set; }

    public bool? AllowDelegation { get; set; }

    public bool? IsActive { get; set; }

    [Required]
    public Guid UpdatedById { get; set; }
}

public class DelegateVoteRequest
{
    [Required]
    public Guid DelegatorId { get; set; }

    [Required]
    public Guid DelegateId { get; set; }

    public ProposalType? SpecificType { get; set; }

    [MaxLength(1000)]
    public string? DelegationReason { get; set; }

    [Range(0, 100)]
    public decimal? MaxDelegationPercentage { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool AutoRenew { get; set; } = false;
}

public class GetProposalsRequest
{
    public ProposalType? Type { get; set; }
    public ProposalStatus? Status { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetVotesRequest
{
    [Required]
    public Guid ProposalId { get; set; }
    public VoteChoice? Choice { get; set; }
    public Guid? VoterId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "CastAt";
    public bool SortDescending { get; set; } = true;
}

public class GetDelegationsRequest
{
    public Guid? DelegatorId { get; set; }
    public Guid? DelegateId { get; set; }
    public ProposalType? SpecificType { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetAnalyticsRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public ProposalType? ProposalType { get; set; }
    public bool IncludeVotingPowerDistribution { get; set; } = true;
    public bool IncludeParticipationMetrics { get; set; } = true;
}
