using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Services.Interfaces;

public interface IGovernanceAnalyticsService
{
    // Analytics and reporting
    Task<VotingAnalyticsResponse> GetVotingAnalyticsAsync(Guid proposalId);
    Task<GovernanceStatsResponse> GetGovernanceStatsAsync(GetAnalyticsRequest? request = null);
    Task<List<ProposalResponse>> GetActiveProposalsAsync();
    Task<List<ProposalResponse>> GetProposalsRequiringActionAsync();

    // Proposal status checking
    Task<bool> IsProposalActiveAsync(Guid proposalId);
    Task<bool> IsQuorumMetAsync(Guid proposalId);
    Task<bool> IsApprovalThresholdMetAsync(Guid proposalId);

    // Advanced analytics calculations
    Task<decimal> CalculateCurrentQuorumAsync(Guid proposalId);
    Task<decimal> CalculateCurrentApprovalAsync(Guid proposalId);
    Task<ParticipationMetrics> CalculateParticipationMetricsAsync(Guid proposalId);
    Task<List<VotingPowerDistribution>> CalculateVotingPowerDistributionAsync(Guid proposalId);
}
