using GovernanceService.Data;
using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;
using GovernanceService.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovernanceService.Services;

public class GovernanceService : IGovernanceService
{
    private readonly GovernanceDbContext _context;
    private readonly IGovernanceVotingService _votingService;
    private readonly IGovernanceValidationService _validationService;
    private readonly IGovernanceAnalyticsService _analyticsService;
    private readonly IGovernanceExecutionService _executionService;
    private readonly IGovernanceBackgroundService _backgroundService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GovernanceService> _logger;

    public GovernanceService(
        GovernanceDbContext context,
        IGovernanceVotingService votingService,
        IGovernanceValidationService validationService,
        IGovernanceAnalyticsService analyticsService,
        IGovernanceExecutionService executionService,
        IGovernanceBackgroundService backgroundService,
        IMemoryCache cache,
        ILogger<GovernanceService> logger)
    {
        _context = context;
        _votingService = votingService;
        _validationService = validationService;
        _analyticsService = analyticsService;
        _executionService = executionService;
        _backgroundService = backgroundService;
        _cache = cache;
        _logger = logger;
    }

    #region Proposal Management

    public async Task<ProposalResponse> CreateProposalAsync(CreateProposalRequest request)
    {
        _logger.LogInformation("Creating proposal: {Title} by user {CreatorId}", request.Title, request.CreatorId);

        try
        {
            // Validate governance rules and user permissions
            var rule = await _validationService.GetApplicableGovernanceRuleAsync(request.Type);
            await _validationService.ValidateProposalCreationAsync(request, rule);

            // Calculate voting parameters based on governance rules
            var votingStartTime = request.CustomVotingStartTime ?? DateTime.UtcNow.Add(rule.ExecutionDelay);
            var votingEndTime = request.CustomVotingEndTime ?? votingStartTime.Add(rule.VotingPeriod);

            // Create proposal with calculated parameters
            var proposal = new Proposal
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                CreatorId = request.CreatorId,
                VotingStartTime = votingStartTime,
                VotingEndTime = votingEndTime,
                QuorumPercentage = request.CustomQuorumPercentage ?? rule.MinimumQuorum,
                ApprovalThreshold = request.CustomApprovalThreshold ?? rule.ApprovalThreshold,
                ExecutionParameters = request.ExecutionParameters,
                RequestedAmount = request.RequestedAmount,
                RequestedCurrency = request.RequestedCurrency,
                Status = ProposalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();

            // Invalidate relevant caches
            await _backgroundService.InvalidateGovernanceCacheAsync();

            _logger.LogInformation("Proposal created successfully: {ProposalId}", proposal.Id);
            return proposal.Adapt<ProposalResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create proposal for user {CreatorId}", request.CreatorId);
            throw;
        }
    }

    public async Task<ProposalResponse> GetProposalAsync(Guid proposalId)
    {
        _logger.LogInformation("Retrieving proposal: {ProposalId}", proposalId);

        var proposal = await _context.Proposals
            .Include(p => p.Votes)
            .Include(p => p.Executions)
            .FirstOrDefaultAsync(p => p.Id == proposalId);

        if (proposal == null)
        {
            throw new ArgumentException($"Proposal not found: {proposalId}");
        }

        return proposal.Adapt<ProposalResponse>();
    }

    public async Task<PagedResponse<ProposalResponse>> GetProposalsAsync(GetProposalsRequest request)
    {
        _logger.LogInformation("Retrieving proposals with filters");

        var query = _context.Proposals.AsQueryable();

        // Apply filters
        if (request.Type.HasValue)
            query = query.Where(p => p.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.CreatorId.HasValue)
            query = query.Where(p => p.CreatorId == request.CreatorId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt <= request.ToDate.Value);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            "type" => request.SortDescending ? query.OrderByDescending(p => p.Type) : query.OrderBy(p => p.Type),
            "status" => request.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "votingstarttime" => request.SortDescending ? query.OrderByDescending(p => p.VotingStartTime) : query.OrderBy(p => p.VotingStartTime),
            _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var proposals = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Votes)
            .ToListAsync();

        var proposalResponses = proposals.Adapt<List<ProposalResponse>>();

        return new PagedResponse<ProposalResponse>
        {
            Items = proposalResponses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<ProposalResponse> UpdateProposalAsync(Guid proposalId, UpdateProposalRequest request)
    {
        _logger.LogInformation("Updating proposal: {ProposalId}", proposalId);

        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            throw new ArgumentException($"Proposal not found: {proposalId}");
        }

        // Validate that proposal can be updated
        if (proposal.Status != ProposalStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot update proposal in status: {proposal.Status}");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Title))
            proposal.Title = request.Title;

        if (!string.IsNullOrEmpty(request.Description))
            proposal.Description = request.Description;

        if (!string.IsNullOrEmpty(request.ExecutionParameters))
            proposal.ExecutionParameters = request.ExecutionParameters;

        if (request.RequestedAmount.HasValue)
            proposal.RequestedAmount = request.RequestedAmount.Value;

        if (!string.IsNullOrEmpty(request.RequestedCurrency))
            proposal.RequestedCurrency = request.RequestedCurrency;

        if (request.Status.HasValue)
            proposal.Status = request.Status.Value;

        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        _logger.LogInformation("Proposal updated successfully: {ProposalId}", proposalId);
        return proposal.Adapt<ProposalResponse>();
    }

    public async Task<bool> DeleteProposalAsync(Guid proposalId)
    {
        _logger.LogInformation("Deleting proposal: {ProposalId}", proposalId);

        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            return false;
        }

        // Only allow deletion of pending proposals
        if (proposal.Status != ProposalStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot delete proposal in status: {proposal.Status}");
        }

        _context.Proposals.Remove(proposal);
        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        _logger.LogInformation("Proposal deleted successfully: {ProposalId}", proposalId);
        return true;
    }

    public async Task<ProposalResponse> CancelProposalAsync(Guid proposalId, Guid userId)
    {
        _logger.LogInformation("Cancelling proposal: {ProposalId} by user {UserId}", proposalId, userId);

        var proposal = await _context.Proposals.FindAsync(proposalId);
        if (proposal == null)
        {
            throw new ArgumentException($"Proposal not found: {proposalId}");
        }

        // Validate that user can cancel the proposal
        if (proposal.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the proposal creator can cancel the proposal");
        }

        // Validate that proposal can be cancelled
        if (proposal.Status != ProposalStatus.Pending && proposal.Status != ProposalStatus.Active)
        {
            throw new InvalidOperationException($"Cannot cancel proposal in status: {proposal.Status}");
        }

        proposal.Status = ProposalStatus.Cancelled;
        proposal.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        _logger.LogInformation("Proposal cancelled successfully: {ProposalId}", proposalId);
        return proposal.Adapt<ProposalResponse>();
    }

    #endregion

    #region Voting Operations - Delegate to VotingService

    public async Task<VoteResponse> CastVoteAsync(CastVoteRequest request)
        => await _votingService.CastVoteAsync(request);

    public async Task<VoteResponse?> GetVoteAsync(Guid proposalId, Guid voterId)
        => await _votingService.GetVoteAsync(proposalId, voterId);

    public async Task<PagedResponse<VoteResponse>> GetVotesAsync(GetVotesRequest request)
        => await _votingService.GetVotesAsync(request);

    public async Task<bool> HasUserVotedAsync(Guid proposalId, Guid voterId)
        => await _votingService.HasUserVotedAsync(proposalId, voterId);

    public async Task<decimal> CalculateVotingPowerAsync(Guid userId, Guid? proposalId = null)
        => await _votingService.CalculateVotingPowerAsync(userId, proposalId);

    public async Task<decimal> CalculateEffectiveVotingPowerAsync(Guid userId, Guid proposalId)
        => await _votingService.CalculateEffectiveVotingPowerAsync(userId, proposalId);

    #endregion

    #region Governance Rules

    public async Task<GovernanceRuleResponse> CreateGovernanceRuleAsync(CreateGovernanceRuleRequest request)
    {
        _logger.LogInformation("Creating governance rule: {RuleName} for type {ApplicableType}", request.RuleName, request.ApplicableType);

        var rule = new GovernanceRule
        {
            Id = Guid.NewGuid(),
            RuleName = request.RuleName,
            Description = request.Description,
            ApplicableType = request.ApplicableType,
            MinimumQuorum = request.MinimumQuorum,
            ApprovalThreshold = request.ApprovalThreshold,
            VotingPeriod = request.VotingPeriod,
            ExecutionDelay = request.ExecutionDelay,
            MinimumTokensRequired = request.MinimumTokensRequired,
            ProposalDeposit = request.ProposalDeposit,
            RequiresMultiSig = request.RequiresMultiSig,
            RequiredSignatures = request.RequiredSignatures,
            AllowDelegation = request.AllowDelegation,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedById = request.CreatedById
        };

        _context.GovernanceRules.Add(rule);
        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        _logger.LogInformation("Governance rule created successfully: {RuleId}", rule.Id);
        return rule.Adapt<GovernanceRuleResponse>();
    }

    public async Task<GovernanceRuleResponse?> GetGovernanceRuleAsync(ProposalType proposalType)
    {
        var rule = await _context.GovernanceRules
            .Where(r => r.ApplicableType == proposalType && r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return rule?.Adapt<GovernanceRuleResponse>();
    }

    public async Task<List<GovernanceRuleResponse>> GetAllGovernanceRulesAsync()
    {
        var rules = await _context.GovernanceRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.ApplicableType)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rules.Adapt<List<GovernanceRuleResponse>>();
    }

    public async Task<GovernanceRuleResponse> UpdateGovernanceRuleAsync(Guid ruleId, UpdateGovernanceRuleRequest request)
    {
        var rule = await _context.GovernanceRules.FindAsync(ruleId);
        if (rule == null)
        {
            throw new ArgumentException($"Governance rule not found: {ruleId}");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.RuleName))
            rule.RuleName = request.RuleName;

        if (!string.IsNullOrEmpty(request.Description))
            rule.Description = request.Description;

        if (request.MinimumQuorum.HasValue)
            rule.MinimumQuorum = request.MinimumQuorum.Value;

        if (request.ApprovalThreshold.HasValue)
            rule.ApprovalThreshold = request.ApprovalThreshold.Value;

        if (request.VotingPeriod.HasValue)
            rule.VotingPeriod = request.VotingPeriod.Value;

        if (request.ExecutionDelay.HasValue)
            rule.ExecutionDelay = request.ExecutionDelay.Value;

        if (request.MinimumTokensRequired.HasValue)
            rule.MinimumTokensRequired = request.MinimumTokensRequired.Value;

        if (request.ProposalDeposit.HasValue)
            rule.ProposalDeposit = request.ProposalDeposit.Value;

        if (request.RequiresMultiSig.HasValue)
            rule.RequiresMultiSig = request.RequiresMultiSig.Value;

        if (request.RequiredSignatures.HasValue)
            rule.RequiredSignatures = request.RequiredSignatures.Value;

        if (request.AllowDelegation.HasValue)
            rule.AllowDelegation = request.AllowDelegation.Value;

        if (request.IsActive.HasValue)
            rule.IsActive = request.IsActive.Value;

        rule.UpdatedAt = DateTime.UtcNow;
        rule.UpdatedById = request.UpdatedById;

        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        return rule.Adapt<GovernanceRuleResponse>();
    }

    public async Task<bool> DeactivateGovernanceRuleAsync(Guid ruleId, Guid userId)
    {
        var rule = await _context.GovernanceRules.FindAsync(ruleId);
        if (rule == null)
        {
            return false;
        }

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        rule.UpdatedById = userId;

        await _context.SaveChangesAsync();
        await _backgroundService.InvalidateGovernanceCacheAsync();

        return true;
    }

    #endregion

    #region Delegation Management - Delegate to VotingService

    public async Task<VotingDelegationResponse> DelegateVotingPowerAsync(DelegateVoteRequest request)
        => await _votingService.DelegateVotingPowerAsync(request);

    public async Task<bool> RevokeVotingDelegationAsync(Guid delegationId, Guid userId, string? reason = null)
        => await _votingService.RevokeVotingDelegationAsync(delegationId, userId, reason);

    public async Task<PagedResponse<VotingDelegationResponse>> GetDelegationsAsync(GetDelegationsRequest request)
        => await _votingService.GetDelegationsAsync(request);

    public async Task<List<VotingDelegationResponse>> GetUserDelegationsAsync(Guid userId, bool? isActive = null)
        => await _votingService.GetUserDelegationsAsync(userId, isActive);

    public async Task<List<VotingDelegationResponse>> GetDelegationsToUserAsync(Guid delegateId, bool? isActive = null)
        => await _votingService.GetDelegationsToUserAsync(delegateId, isActive);

    #endregion

    #region Proposal Execution - Delegate to ExecutionService

    public async Task<ProposalExecutionResponse> ScheduleProposalExecutionAsync(Guid proposalId)
        => await _executionService.ScheduleProposalExecutionAsync(proposalId);

    public async Task<ProposalExecutionResponse> ExecuteProposalAsync(Guid proposalId, Guid executorId)
        => await _executionService.ExecuteProposalAsync(proposalId, executorId);

    public async Task<List<ProposalExecutionResponse>> GetPendingExecutionsAsync()
        => await _executionService.GetPendingExecutionsAsync();

    public async Task<ProposalExecutionResponse?> GetProposalExecutionAsync(Guid proposalId)
        => await _executionService.GetProposalExecutionAsync(proposalId);

    #endregion

    #region Analytics and Reporting - Delegate to AnalyticsService

    public async Task<VotingAnalyticsResponse> GetVotingAnalyticsAsync(Guid proposalId)
        => await _analyticsService.GetVotingAnalyticsAsync(proposalId);

    public async Task<GovernanceStatsResponse> GetGovernanceStatsAsync(GetAnalyticsRequest? request = null)
        => await _analyticsService.GetGovernanceStatsAsync(request);

    public async Task<List<ProposalResponse>> GetActiveProposalsAsync()
        => await _analyticsService.GetActiveProposalsAsync();

    public async Task<List<ProposalResponse>> GetProposalsRequiringActionAsync()
        => await _analyticsService.GetProposalsRequiringActionAsync();

    #endregion

    #region Validation and Business Logic - Delegate to ValidationService

    public async Task<bool> ValidateProposalAsync(CreateProposalRequest request)
        => await _validationService.ValidateProposalAsync(request);

    public async Task<bool> CanUserCreateProposalAsync(Guid userId, ProposalType proposalType)
        => await _validationService.CanUserCreateProposalAsync(userId, proposalType);

    public async Task<bool> CanUserVoteAsync(Guid userId, Guid proposalId)
        => await _votingService.CanUserVoteAsync(userId, proposalId);

    public async Task<bool> IsProposalActiveAsync(Guid proposalId)
        => await _analyticsService.IsProposalActiveAsync(proposalId);

    public async Task<bool> IsQuorumMetAsync(Guid proposalId)
        => await _analyticsService.IsQuorumMetAsync(proposalId);

    public async Task<bool> IsApprovalThresholdMetAsync(Guid proposalId)
        => await _analyticsService.IsApprovalThresholdMetAsync(proposalId);

    #endregion

    #region Background Processing - Delegate to BackgroundService

    public async Task ProcessExpiredProposalsAsync()
        => await _backgroundService.ProcessExpiredProposalsAsync();

    public async Task ProcessScheduledExecutionsAsync()
        => await _backgroundService.ProcessScheduledExecutionsAsync();

    public async Task UpdateProposalStatusesAsync()
        => await _backgroundService.UpdateProposalStatusesAsync();

    public async Task ProcessDelegationExpirationsAsync()
        => await _backgroundService.ProcessDelegationExpirationsAsync();

    #endregion

    #region Token Service Integration - Delegate to VotingService

    public async Task<decimal> GetUserTokenBalanceAsync(Guid userId)
        => await _votingService.GetUserTokenBalanceAsync(userId);

    public async Task<List<TokenHolding>> GetUserTokenHoldingsAsync(Guid userId)
        => await _votingService.GetUserTokenHoldingsAsync(userId);

    public async Task<decimal> CalculateTotalVotingPowerAsync()
        => await _votingService.CalculateTotalVotingPowerAsync();

    #endregion
}
