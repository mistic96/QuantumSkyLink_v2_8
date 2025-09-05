using GovernanceService.Data;
using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;
using GovernanceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovernanceService.Services;

public class GovernanceValidationService : IGovernanceValidationService
{
    private readonly GovernanceDbContext _context;
    private readonly IGovernanceVotingService _votingService;
    private readonly ILogger<GovernanceValidationService> _logger;

    public GovernanceValidationService(
        GovernanceDbContext context,
        IGovernanceVotingService votingService,
        ILogger<GovernanceValidationService> logger)
    {
        _context = context;
        _votingService = votingService;
        _logger = logger;
    }

    #region Validation and Business Logic

    public async Task<bool> ValidateProposalAsync(CreateProposalRequest request)
    {
        _logger.LogInformation("Validating proposal creation request for user {CreatorId}", request.CreatorId);

        try
        {
            // Get applicable governance rule
            var rule = await GetApplicableGovernanceRuleAsync(request.Type);

            // Validate against governance rule
            await ValidateProposalCreationAsync(request, rule);

            // Validate timing
            if (!await ValidateProposalTimingAsync(request))
            {
                return false;
            }

            // Validate execution parameters
            if (!await ValidateExecutionParametersAsync(request.ExecutionParameters, request.Type))
            {
                return false;
            }

            // Validate amount if specified
            if (request.RequestedAmount.HasValue && 
                !await ValidateProposalAmountAsync(request.RequestedAmount, request.RequestedCurrency, request.Type))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating proposal for user {CreatorId}", request.CreatorId);
            return false;
        }
    }

    public async Task<bool> CanUserCreateProposalAsync(Guid userId, ProposalType proposalType)
    {
        _logger.LogInformation("Checking if user {UserId} can create proposal of type {ProposalType}", userId, proposalType);

        try
        {
            // Check if user is eligible for governance
            if (!await IsUserEligibleForGovernanceAsync(userId))
            {
                return false;
            }

            // Get governance rule for proposal type
            var rule = await GetApplicableGovernanceRuleAsync(proposalType);

            // Check minimum token requirements
            if (!await HasSufficientTokensForProposalAsync(userId, proposalType))
            {
                return false;
            }

            // Check if user has sufficient voting power
            var minimumPower = rule.MinimumTokensRequired ?? 0m;
            if (!await HasSufficientVotingPowerAsync(userId, minimumPower))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can create proposal of type {ProposalType}", userId, proposalType);
            return false;
        }
    }

    public async Task ValidateProposalCreationAsync(CreateProposalRequest request, GovernanceRule rule)
    {
        _logger.LogInformation("Validating proposal creation against governance rule {RuleId}", rule.Id);

        // Check if user can create this type of proposal
        if (!await CanUserCreateProposalAsync(request.CreatorId, request.Type))
        {
            throw new UnauthorizedAccessException($"User {request.CreatorId} is not authorized to create {request.Type} proposals");
        }

        // Validate proposal deposit if required
        if (rule.ProposalDeposit.HasValue && rule.ProposalDeposit.Value > 0)
        {
            var userBalance = await _votingService.GetUserTokenBalanceAsync(request.CreatorId);
            if (userBalance < rule.ProposalDeposit.Value)
            {
                throw new InvalidOperationException($"Insufficient balance for proposal deposit. Required: {rule.ProposalDeposit.Value}, Available: {userBalance}");
            }
        }

        // Validate custom parameters against rule limits
        if (request.CustomQuorumPercentage.HasValue)
        {
            if (request.CustomQuorumPercentage.Value < rule.MinimumQuorum)
            {
                throw new ArgumentException($"Custom quorum percentage ({request.CustomQuorumPercentage.Value}%) cannot be less than minimum required ({rule.MinimumQuorum}%)");
            }
        }

        if (request.CustomApprovalThreshold.HasValue)
        {
            if (request.CustomApprovalThreshold.Value < rule.ApprovalThreshold)
            {
                throw new ArgumentException($"Custom approval threshold ({request.CustomApprovalThreshold.Value}%) cannot be less than minimum required ({rule.ApprovalThreshold}%)");
            }
        }

        // Validate timing constraints
        if (request.CustomVotingStartTime.HasValue)
        {
            var minimumStartTime = DateTime.UtcNow.Add(rule.ExecutionDelay);
            if (request.CustomVotingStartTime.Value < minimumStartTime)
            {
                throw new ArgumentException($"Custom voting start time cannot be earlier than {minimumStartTime}");
            }
        }

        _logger.LogInformation("Proposal creation validation passed for user {CreatorId}", request.CreatorId);
    }

    #endregion

    #region Governance Rule Operations

    public async Task<GovernanceRule> GetApplicableGovernanceRuleAsync(ProposalType proposalType)
    {
        _logger.LogInformation("Getting applicable governance rule for proposal type {ProposalType}", proposalType);

        var rule = await _context.GovernanceRules
            .Where(r => r.ApplicableType == proposalType && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (rule == null)
        {
            throw new InvalidOperationException($"No active governance rule found for proposal type: {proposalType}");
        }

        return rule;
    }

    public async Task<bool> ValidateGovernanceRuleAsync(GovernanceRule rule)
    {
        _logger.LogInformation("Validating governance rule {RuleId}", rule.Id);

        try
        {
            // Validate percentage values
            if (rule.MinimumQuorum < 0 || rule.MinimumQuorum > 100)
            {
                _logger.LogWarning("Invalid minimum quorum percentage: {MinimumQuorum}", rule.MinimumQuorum);
                return false;
            }

            if (rule.ApprovalThreshold < 0 || rule.ApprovalThreshold > 100)
            {
                _logger.LogWarning("Invalid approval threshold percentage: {ApprovalThreshold}", rule.ApprovalThreshold);
                return false;
            }

            // Validate time periods
            if (rule.VotingPeriod <= TimeSpan.Zero)
            {
                _logger.LogWarning("Invalid voting period: {VotingPeriod}", rule.VotingPeriod);
                return false;
            }

            if (rule.ExecutionDelay < TimeSpan.Zero)
            {
                _logger.LogWarning("Invalid execution delay: {ExecutionDelay}", rule.ExecutionDelay);
                return false;
            }

            // Validate multi-signature requirements
            if (rule.RequiresMultiSig && (!rule.RequiredSignatures.HasValue || rule.RequiredSignatures.Value <= 0))
            {
                _logger.LogWarning("Multi-signature required but no valid signature count specified");
                return false;
            }

            // Validate token requirements
            if (rule.MinimumTokensRequired.HasValue && rule.MinimumTokensRequired.Value < 0)
            {
                _logger.LogWarning("Invalid minimum tokens required: {MinimumTokensRequired}", rule.MinimumTokensRequired);
                return false;
            }

            if (rule.ProposalDeposit.HasValue && rule.ProposalDeposit.Value < 0)
            {
                _logger.LogWarning("Invalid proposal deposit: {ProposalDeposit}", rule.ProposalDeposit);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating governance rule {RuleId}", rule.Id);
            return false;
        }
    }

    public async Task<bool> IsGovernanceRuleActiveAsync(Guid ruleId)
    {
        try
        {
            var rule = await _context.GovernanceRules.FindAsync(ruleId);
            return rule?.IsActive ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if governance rule {RuleId} is active", ruleId);
            return false;
        }
    }

    #endregion

    #region Permission and Eligibility Checking

    public async Task<bool> HasSufficientTokensForProposalAsync(Guid userId, ProposalType proposalType)
    {
        _logger.LogInformation("Checking if user {UserId} has sufficient tokens for {ProposalType} proposal", userId, proposalType);

        try
        {
            var rule = await GetApplicableGovernanceRuleAsync(proposalType);
            
            if (!rule.MinimumTokensRequired.HasValue)
            {
                return true; // No minimum requirement
            }

            var userBalance = await _votingService.GetUserTokenBalanceAsync(userId);
            var hasEnough = userBalance >= rule.MinimumTokensRequired.Value;

            _logger.LogInformation("User {UserId} token check: Required={Required}, Available={Available}, HasEnough={HasEnough}", 
                userId, rule.MinimumTokensRequired.Value, userBalance, hasEnough);

            return hasEnough;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token sufficiency for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HasSufficientVotingPowerAsync(Guid userId, decimal minimumPower)
    {
        _logger.LogInformation("Checking if user {UserId} has sufficient voting power (minimum: {MinimumPower})", userId, minimumPower);

        try
        {
            var votingPower = await _votingService.CalculateVotingPowerAsync(userId);
            var hasEnough = votingPower >= minimumPower;

            _logger.LogInformation("User {UserId} voting power check: Required={Required}, Available={Available}, HasEnough={HasEnough}", 
                userId, minimumPower, votingPower, hasEnough);

            return hasEnough;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking voting power for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserEligibleForGovernanceAsync(Guid userId)
    {
        _logger.LogInformation("Checking governance eligibility for user {UserId}", userId);

        try
        {
            // Check if user has any governance tokens
            var tokenHoldings = await _votingService.GetUserTokenHoldingsAsync(userId);
            var hasGovernanceTokens = tokenHoldings.Any(h => h.IsGovernanceToken && h.Balance > 0);

            if (!hasGovernanceTokens)
            {
                _logger.LogInformation("User {UserId} has no governance tokens", userId);
                return false;
            }

            // Additional eligibility checks can be added here
            // For example: account age, KYC status, etc.

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking governance eligibility for user {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region Business Rule Validation

    public async Task<bool> ValidateProposalTimingAsync(CreateProposalRequest request)
    {
        _logger.LogInformation("Validating proposal timing for user {CreatorId}", request.CreatorId);

        try
        {
            var now = DateTime.UtcNow;

            // Validate custom voting start time
            if (request.CustomVotingStartTime.HasValue)
            {
                if (request.CustomVotingStartTime.Value <= now)
                {
                    _logger.LogWarning("Custom voting start time {StartTime} is in the past", request.CustomVotingStartTime.Value);
                    return false;
                }
            }

            // Validate custom voting end time
            if (request.CustomVotingEndTime.HasValue)
            {
                var startTime = request.CustomVotingStartTime ?? now;
                if (request.CustomVotingEndTime.Value <= startTime)
                {
                    _logger.LogWarning("Custom voting end time {EndTime} is not after start time {StartTime}", 
                        request.CustomVotingEndTime.Value, startTime);
                    return false;
                }

                // Ensure minimum voting period
                var votingDuration = request.CustomVotingEndTime.Value - startTime;
                if (votingDuration < TimeSpan.FromHours(1)) // Minimum 1 hour voting period
                {
                    _logger.LogWarning("Voting period {Duration} is too short (minimum 1 hour)", votingDuration);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating proposal timing for user {CreatorId}", request.CreatorId);
            return false;
        }
    }

    public async Task<bool> ValidateExecutionParametersAsync(string? executionParameters, ProposalType proposalType)
    {
        _logger.LogInformation("Validating execution parameters for proposal type {ProposalType}", proposalType);

        try
        {
            // If no execution parameters provided, check if they're required for this proposal type
            if (string.IsNullOrEmpty(executionParameters))
            {
                // Some proposal types require execution parameters
                var requiresParameters = proposalType switch
                {
                    ProposalType.Treasury => true,
                    ProposalType.Parameter => true,
                    ProposalType.Upgrade => true,
                    _ => false
                };

                if (requiresParameters)
                {
                    _logger.LogWarning("Execution parameters required for proposal type {ProposalType}", proposalType);
                    return false;
                }

                return true;
            }

            // Validate parameter format based on proposal type
            switch (proposalType)
            {
                case ProposalType.Treasury:
                    return await ValidateTreasuryParametersAsync(executionParameters);
                
                case ProposalType.Parameter:
                    return await ValidateParameterChangeAsync(executionParameters);
                
                case ProposalType.Upgrade:
                    return await ValidateUpgradeParametersAsync(executionParameters);
                
                default:
                    // For other types, basic validation
                    return executionParameters.Length <= 10000; // Max 10KB
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating execution parameters for proposal type {ProposalType}", proposalType);
            return false;
        }
    }

    public async Task<bool> ValidateProposalAmountAsync(decimal? amount, string? currency, ProposalType proposalType)
    {
        _logger.LogInformation("Validating proposal amount {Amount} {Currency} for type {ProposalType}", amount, currency, proposalType);

        try
        {
            if (!amount.HasValue)
            {
                return true; // No amount specified
            }

            // Amount must be positive
            if (amount.Value <= 0)
            {
                _logger.LogWarning("Proposal amount must be positive: {Amount}", amount.Value);
                return false;
            }

            // Validate currency for treasury proposals
            if (proposalType == ProposalType.Treasury)
            {
                if (string.IsNullOrEmpty(currency))
                {
                    _logger.LogWarning("Currency required for treasury proposals");
                    return false;
                }

                // Validate against supported currencies
                var supportedCurrencies = new[] { "USD", "EUR", "BTC", "ETH", "USDC", "USDT" };
                if (!supportedCurrencies.Contains(currency.ToUpper()))
                {
                    _logger.LogWarning("Unsupported currency: {Currency}", currency);
                    return false;
                }
            }

            // Validate amount limits based on proposal type
            var maxAmount = proposalType switch
            {
                ProposalType.Treasury => 1_000_000m, // Max 1M for treasury
                ProposalType.Emergency => 100_000m,  // Max 100K for emergency
                _ => decimal.MaxValue
            };

            if (amount.Value > maxAmount)
            {
                _logger.LogWarning("Amount {Amount} exceeds maximum {MaxAmount} for proposal type {ProposalType}", 
                    amount.Value, maxAmount, proposalType);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating proposal amount {Amount} {Currency}", amount, currency);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<bool> ValidateTreasuryParametersAsync(string parameters)
    {
        try
        {
            // Treasury parameters should contain recipient address and amount
            // This is a simplified validation - in practice, you'd parse JSON/XML
            return parameters.Contains("recipient") && parameters.Contains("amount");
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ValidateParameterChangeAsync(string parameters)
    {
        try
        {
            // Parameter change should specify parameter name and new value
            return parameters.Contains("parameter") && parameters.Contains("value");
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> ValidateUpgradeParametersAsync(string parameters)
    {
        try
        {
            // Upgrade parameters should contain version and deployment info
            return parameters.Contains("version") && parameters.Contains("deployment");
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
