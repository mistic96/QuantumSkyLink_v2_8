using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Services.Interfaces;

public interface IGovernanceVotingService
{
    // Voting operations
    Task<VoteResponse> CastVoteAsync(CastVoteRequest request);
    Task<VoteResponse?> GetVoteAsync(Guid proposalId, Guid voterId);
    Task<PagedResponse<VoteResponse>> GetVotesAsync(GetVotesRequest request);
    Task<bool> HasUserVotedAsync(Guid proposalId, Guid voterId);
    Task<decimal> CalculateVotingPowerAsync(Guid userId, Guid? proposalId = null);
    Task<decimal> CalculateEffectiveVotingPowerAsync(Guid userId, Guid proposalId);

    // Delegation management
    Task<VotingDelegationResponse> DelegateVotingPowerAsync(DelegateVoteRequest request);
    Task<bool> RevokeVotingDelegationAsync(Guid delegationId, Guid userId, string? reason = null);
    Task<PagedResponse<VotingDelegationResponse>> GetDelegationsAsync(GetDelegationsRequest request);
    Task<List<VotingDelegationResponse>> GetUserDelegationsAsync(Guid userId, bool? isActive = null);
    Task<List<VotingDelegationResponse>> GetDelegationsToUserAsync(Guid delegateId, bool? isActive = null);

    // Token service integration for voting
    Task<decimal> GetUserTokenBalanceAsync(Guid userId);
    Task<List<TokenHolding>> GetUserTokenHoldingsAsync(Guid userId);
    Task<decimal> CalculateTotalVotingPowerAsync();

    // Voting validation and business logic
    Task<bool> CanUserVoteAsync(Guid userId, Guid proposalId);
    Task ValidateVoteEligibilityAsync(CastVoteRequest request);
    Task UpdateProposalStatusIfNeededAsync(Guid proposalId);
    Task InvalidateVotingCacheAsync(Guid proposalId);
}
