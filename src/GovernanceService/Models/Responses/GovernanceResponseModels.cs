using GovernanceService.Data.Entities;

namespace GovernanceService.Models.Responses;

public class ProposalResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProposalType Type { get; set; }
    public ProposalStatus Status { get; set; }
    public Guid CreatorId { get; set; }
    public DateTime VotingStartTime { get; set; }
    public DateTime VotingEndTime { get; set; }
    public decimal QuorumPercentage { get; set; }
    public decimal ApprovalThreshold { get; set; }
    public string? ExecutionParameters { get; set; }
    public decimal? RequestedAmount { get; set; }
    public string? RequestedCurrency { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Voting statistics
    public int TotalVotes { get; set; }
    public decimal TotalVotingPower { get; set; }
    public decimal ApprovalVotingPower { get; set; }
    public decimal RejectionVotingPower { get; set; }
    public decimal AbstainVotingPower { get; set; }
    public decimal CurrentQuorum { get; set; }
    public decimal CurrentApproval { get; set; }
    public bool IsQuorumMet { get; set; }
    public bool IsApprovalMet { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

public class VoteResponse
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Guid VoterId { get; set; }
    public VoteChoice Choice { get; set; }
    public decimal VotingPower { get; set; }
    public string? Reason { get; set; }
    public DateTime CastAt { get; set; }
    public string? TransactionHash { get; set; }
    public bool IsDelegated { get; set; }
    public Guid? DelegatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GovernanceRuleResponse
{
    public Guid Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProposalType ApplicableType { get; set; }
    public decimal MinimumQuorum { get; set; }
    public decimal ApprovalThreshold { get; set; }
    public TimeSpan VotingPeriod { get; set; }
    public TimeSpan ExecutionDelay { get; set; }
    public decimal? MinimumTokensRequired { get; set; }
    public decimal? ProposalDeposit { get; set; }
    public bool RequiresMultiSig { get; set; }
    public int? RequiredSignatures { get; set; }
    public bool AllowDelegation { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}

public class VotingDelegationResponse
{
    public Guid Id { get; set; }
    public Guid DelegatorId { get; set; }
    public Guid DelegateId { get; set; }
    public ProposalType? SpecificType { get; set; }
    public string? DelegationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedById { get; set; }
    public string? RevocationReason { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? TransactionHash { get; set; }
    public decimal? MaxDelegationPercentage { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AutoRenew { get; set; }
}

public class ProposalExecutionResponse
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? ExecutedById { get; set; }
    public string? ExecutionParameters { get; set; }
    public string? ExecutionResult { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionHash { get; set; }
    public decimal? GasUsed { get; set; }
    public decimal? ExecutionCost { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class VotingAnalyticsResponse
{
    public Guid ProposalId { get; set; }
    public string ProposalTitle { get; set; } = string.Empty;
    public ProposalType ProposalType { get; set; }
    public ProposalStatus ProposalStatus { get; set; }
    
    // Voting statistics
    public int TotalVoters { get; set; }
    public decimal TotalVotingPower { get; set; }
    public decimal QuorumRequired { get; set; }
    public decimal CurrentQuorum { get; set; }
    public decimal ApprovalThreshold { get; set; }
    public decimal CurrentApproval { get; set; }
    
    // Vote breakdown
    public VoteBreakdown VoteBreakdown { get; set; } = new();
    
    // Participation metrics
    public ParticipationMetrics Participation { get; set; } = new();
    
    // Voting power distribution
    public List<VotingPowerDistribution> VotingPowerDistribution { get; set; } = new();
    
    // Timeline
    public DateTime VotingStartTime { get; set; }
    public DateTime VotingEndTime { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

public class VoteBreakdown
{
    public int ApprovalVotes { get; set; }
    public int RejectionVotes { get; set; }
    public int AbstainVotes { get; set; }
    public decimal ApprovalPower { get; set; }
    public decimal RejectionPower { get; set; }
    public decimal AbstainPower { get; set; }
    public decimal ApprovalPercentage { get; set; }
    public decimal RejectionPercentage { get; set; }
    public decimal AbstainPercentage { get; set; }
}

public class ParticipationMetrics
{
    public int EligibleVoters { get; set; }
    public int ActualVoters { get; set; }
    public decimal ParticipationRate { get; set; }
    public int DelegatedVotes { get; set; }
    public decimal DelegationRate { get; set; }
    public decimal AverageVotingPower { get; set; }
    public decimal MedianVotingPower { get; set; }
}

public class VotingPowerDistribution
{
    public string Range { get; set; } = string.Empty;
    public int VoterCount { get; set; }
    public decimal TotalPower { get; set; }
    public decimal Percentage { get; set; }
}

public class GovernanceStatsResponse
{
    public int TotalProposals { get; set; }
    public int ActiveProposals { get; set; }
    public int ApprovedProposals { get; set; }
    public int RejectedProposals { get; set; }
    public int ExecutedProposals { get; set; }
    
    public decimal TotalVotingPower { get; set; }
    public int TotalVoters { get; set; }
    public int ActiveDelegations { get; set; }
    
    public Dictionary<ProposalType, int> ProposalsByType { get; set; } = new();
    public Dictionary<string, decimal> ParticipationByMonth { get; set; } = new();
    
    public DateTime? LastProposalDate { get; set; }
    public DateTime? NextScheduledExecution { get; set; }
    
    public List<TopVoter> TopVoters { get; set; } = new();
    public List<TopDelegate> TopDelegates { get; set; } = new();
}

public class TopVoter
{
    public Guid VoterId { get; set; }
    public int ProposalsVoted { get; set; }
    public decimal TotalVotingPower { get; set; }
    public decimal ParticipationRate { get; set; }
}

public class TopDelegate
{
    public Guid DelegateId { get; set; }
    public int DelegationsReceived { get; set; }
    public decimal TotalDelegatedPower { get; set; }
    public int ProposalsVotedOn { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
