using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Services.Interfaces;

public interface IGovernanceService
{
    // Proposal management
    Task<ProposalResponse> CreateProposalAsync(CreateProposalRequest request);
    Task<ProposalResponse> GetProposalAsync(Guid proposalId);
    Task<PagedResponse<ProposalResponse>> GetProposalsAsync(GetProposalsRequest request);
    Task<ProposalResponse> UpdateProposalAsync(Guid proposalId, UpdateProposalRequest request);
    Task<bool> DeleteProposalAsync(Guid proposalId);
    Task<ProposalResponse> CancelProposalAsync(Guid proposalId, Guid userId);

    // Voting operations
    Task<VoteResponse> CastVoteAsync(CastVoteRequest request);
    Task<VoteResponse?> GetVoteAsync(Guid proposalId, Guid voterId);
    Task<PagedResponse<VoteResponse>> GetVotesAsync(GetVotesRequest request);
    Task<bool> HasUserVotedAsync(Guid proposalId, Guid voterId);
    Task<decimal> CalculateVotingPowerAsync(Guid userId, Guid? proposalId = null);
    Task<decimal> CalculateEffectiveVotingPowerAsync(Guid userId, Guid proposalId);

    // Governance rules
    Task<GovernanceRuleResponse> CreateGovernanceRuleAsync(CreateGovernanceRuleRequest request);
    Task<GovernanceRuleResponse?> GetGovernanceRuleAsync(ProposalType proposalType);
    Task<List<GovernanceRuleResponse>> GetAllGovernanceRulesAsync();
    Task<GovernanceRuleResponse> UpdateGovernanceRuleAsync(Guid ruleId, UpdateGovernanceRuleRequest request);
    Task<bool> DeactivateGovernanceRuleAsync(Guid ruleId, Guid userId);

    // Delegation management
    Task<VotingDelegationResponse> DelegateVotingPowerAsync(DelegateVoteRequest request);
    Task<bool> RevokeVotingDelegationAsync(Guid delegationId, Guid userId, string? reason = null);
    Task<PagedResponse<VotingDelegationResponse>> GetDelegationsAsync(GetDelegationsRequest request);
    Task<List<VotingDelegationResponse>> GetUserDelegationsAsync(Guid userId, bool? isActive = null);
    Task<List<VotingDelegationResponse>> GetDelegationsToUserAsync(Guid delegateId, bool? isActive = null);

    // Proposal execution
    Task<ProposalExecutionResponse> ScheduleProposalExecutionAsync(Guid proposalId);
    Task<ProposalExecutionResponse> ExecuteProposalAsync(Guid proposalId, Guid executorId);
    Task<List<ProposalExecutionResponse>> GetPendingExecutionsAsync();
    Task<ProposalExecutionResponse?> GetProposalExecutionAsync(Guid proposalId);

    // Analytics and reporting
    Task<VotingAnalyticsResponse> GetVotingAnalyticsAsync(Guid proposalId);
    Task<GovernanceStatsResponse> GetGovernanceStatsAsync(GetAnalyticsRequest? request = null);
    Task<List<ProposalResponse>> GetActiveProposalsAsync();
    Task<List<ProposalResponse>> GetProposalsRequiringActionAsync();

    // Validation and business logic
    Task<bool> ValidateProposalAsync(CreateProposalRequest request);
    Task<bool> CanUserCreateProposalAsync(Guid userId, ProposalType proposalType);
    Task<bool> CanUserVoteAsync(Guid userId, Guid proposalId);
    Task<bool> IsProposalActiveAsync(Guid proposalId);
    Task<bool> IsQuorumMetAsync(Guid proposalId);
    Task<bool> IsApprovalThresholdMetAsync(Guid proposalId);

    // Background processing
    Task ProcessExpiredProposalsAsync();
    Task ProcessScheduledExecutionsAsync();
    Task UpdateProposalStatusesAsync();
    Task ProcessDelegationExpirationsAsync();

    // Token service integration
    Task<decimal> GetUserTokenBalanceAsync(Guid userId);
    Task<List<TokenHolding>> GetUserTokenHoldingsAsync(Guid userId);
    Task<decimal> CalculateTotalVotingPowerAsync();
}

// Supporting models for token integration
public class TokenHolding
{
    public Guid TokenId { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal VotingWeight { get; set; }
    public bool IsGovernanceToken { get; set; }
}
