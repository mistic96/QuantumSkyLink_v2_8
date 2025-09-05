using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;

namespace GovernanceService.Services.Interfaces;

public interface IGovernanceValidationService
{
    // Validation and business logic
    Task<bool> ValidateProposalAsync(CreateProposalRequest request);
    Task<bool> CanUserCreateProposalAsync(Guid userId, ProposalType proposalType);
    Task ValidateProposalCreationAsync(CreateProposalRequest request, GovernanceRule rule);

    // Governance rule operations
    Task<GovernanceRule> GetApplicableGovernanceRuleAsync(ProposalType proposalType);
    Task<bool> ValidateGovernanceRuleAsync(GovernanceRule rule);
    Task<bool> IsGovernanceRuleActiveAsync(Guid ruleId);

    // Permission and eligibility checking
    Task<bool> HasSufficientTokensForProposalAsync(Guid userId, ProposalType proposalType);
    Task<bool> HasSufficientVotingPowerAsync(Guid userId, decimal minimumPower);
    Task<bool> IsUserEligibleForGovernanceAsync(Guid userId);

    // Business rule validation
    Task<bool> ValidateProposalTimingAsync(CreateProposalRequest request);
    Task<bool> ValidateExecutionParametersAsync(string? executionParameters, ProposalType proposalType);
    Task<bool> ValidateProposalAmountAsync(decimal? amount, string? currency, ProposalType proposalType);
}
