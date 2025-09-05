using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Services.Interfaces;

public interface IGovernanceExecutionService
{
    // Proposal execution
    Task<ProposalExecutionResponse> ScheduleProposalExecutionAsync(Guid proposalId);
    Task<ProposalExecutionResponse> ExecuteProposalAsync(Guid proposalId, Guid executorId);
    Task<List<ProposalExecutionResponse>> GetPendingExecutionsAsync();
    Task<ProposalExecutionResponse?> GetProposalExecutionAsync(Guid proposalId);

    // Background processing for execution
    Task ProcessScheduledExecutionsAsync();
    Task<bool> IsProposalReadyForExecutionAsync(Guid proposalId);
    Task<bool> ValidateExecutionParametersAsync(Guid proposalId);
    Task RetryFailedExecutionAsync(Guid executionId);
}
