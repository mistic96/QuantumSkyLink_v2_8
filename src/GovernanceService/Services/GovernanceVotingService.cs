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

public class GovernanceVotingService : IGovernanceVotingService
{
    private readonly GovernanceDbContext _context;
    private readonly ITokenServiceClient _tokenServiceClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GovernanceVotingService> _logger;

    public GovernanceVotingService(
        GovernanceDbContext context,
        ITokenServiceClient tokenServiceClient,
        IMemoryCache cache,
        ILogger<GovernanceVotingService> logger)
    {
        _context = context;
        _tokenServiceClient = tokenServiceClient;
        _cache = cache;
        _logger = logger;
    }

    #region Voting Operations

    public async Task<VoteResponse> CastVoteAsync(CastVoteRequest request)
    {
        _logger.LogInformation("Casting vote for proposal {ProposalId} by user {VoterId}", request.ProposalId, request.VoterId);

        try
        {
            // Comprehensive validation
            await ValidateVoteEligibilityAsync(request);

            // Check if user has already voted
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ProposalId == request.ProposalId && v.VoterId == request.VoterId);

            if (existingVote != null)
            {
                throw new InvalidOperationException("User has already voted on this proposal");
            }

            // Calculate effective voting power (including delegations)
            var votingPower = request.CustomVotingPower ?? await CalculateEffectiveVotingPowerAsync(request.VoterId, request.ProposalId);

            // Create vote
            var vote = new Vote
            {
                Id = Guid.NewGuid(),
                ProposalId = request.ProposalId,
                VoterId = request.VoterId,
                Choice = request.Choice,
                VotingPower = votingPower,
                Reason = request.Reason,
                CastAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Update proposal status if thresholds met
            await UpdateProposalStatusIfNeededAsync(request.ProposalId);

            // Invalidate relevant caches
            await InvalidateVotingCacheAsync(request.ProposalId);

            _logger.LogInformation("Vote cast successfully: {VoteId} with power {VotingPower}", vote.Id, votingPower);
            return vote.Adapt<VoteResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cast vote for proposal {ProposalId} by user {VoterId}", request.ProposalId, request.VoterId);
            throw;
        }
    }

    public async Task<VoteResponse?> GetVoteAsync(Guid proposalId, Guid voterId)
    {
        _logger.LogInformation("Retrieving vote for proposal {ProposalId} by user {VoterId}", proposalId, voterId);

        var vote = await _context.Votes
            .FirstOrDefaultAsync(v => v.ProposalId == proposalId && v.VoterId == voterId);

        return vote?.Adapt<VoteResponse>();
    }

    public async Task<PagedResponse<VoteResponse>> GetVotesAsync(GetVotesRequest request)
    {
        _logger.LogInformation("Retrieving votes for proposal {ProposalId}", request.ProposalId);

        var query = _context.Votes
            .Where(v => v.ProposalId == request.ProposalId);

        // Apply filters
        if (request.Choice.HasValue)
            query = query.Where(v => v.Choice == request.Choice.Value);

        if (request.VoterId.HasValue)
            query = query.Where(v => v.VoterId == request.VoterId.Value);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "choice" => request.SortDescending ? query.OrderByDescending(v => v.Choice) : query.OrderBy(v => v.Choice),
            "votingpower" => request.SortDescending ? query.OrderByDescending(v => v.VotingPower) : query.OrderBy(v => v.VotingPower),
            _ => request.SortDescending ? query.OrderByDescending(v => v.CastAt) : query.OrderBy(v => v.CastAt)
        };

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var votes = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var voteResponses = votes.Adapt<List<VoteResponse>>();

        return new PagedResponse<VoteResponse>
        {
            Items = voteResponses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<bool> HasUserVotedAsync(Guid proposalId, Guid voterId)
    {
        return await _context.Votes
            .AnyAsync(v => v.ProposalId == proposalId && v.VoterId == voterId);
    }

    public async Task<decimal> CalculateVotingPowerAsync(Guid userId, Guid? proposalId = null)
    {
        _logger.LogInformation("Calculating voting power for user {UserId}", userId);

        try
        {
            // Check cache first
            var cacheKey = $"voting_power_{userId}_{proposalId}";
            if (_cache.TryGetValue(cacheKey, out decimal cachedPower))
            {
                return cachedPower;
            }

            // Get user token holdings from TokenService
            var tokenHoldingsResponse = await _tokenServiceClient.GetUserTokenHoldingsAsync(userId);
            if (!tokenHoldingsResponse.IsSuccessStatusCode || tokenHoldingsResponse.Content == null)
            {
                _logger.LogWarning("Failed to retrieve token holdings for user {UserId}", userId);
                return 0m;
            }

            var tokenHoldings = tokenHoldingsResponse.Content;

            // Calculate base voting power from governance tokens
            var baseVotingPower = tokenHoldings
                .Where(h => h.IsGovernanceToken)
                .Sum(h => h.Balance * h.VotingWeight);

            // Apply delegation effects if proposal-specific
            decimal effectiveVotingPower = baseVotingPower;
            if (proposalId.HasValue)
            {
                effectiveVotingPower = await CalculateEffectiveVotingPowerAsync(userId, proposalId.Value);
            }

            // Cache the result for 5 minutes
            _cache.Set(cacheKey, effectiveVotingPower, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Calculated voting power {VotingPower} for user {UserId}", effectiveVotingPower, userId);
            return effectiveVotingPower;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate voting power for user {UserId}", userId);
            return 0m;
        }
    }

    public async Task<decimal> CalculateEffectiveVotingPowerAsync(Guid userId, Guid proposalId)
    {
        _logger.LogInformation("Calculating effective voting power for user {UserId} on proposal {ProposalId}", userId, proposalId);

        try
        {
            // Get base voting power
            var baseVotingPower = await CalculateVotingPowerAsync(userId);

            // Get proposal details for type-specific delegations
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null)
            {
                return baseVotingPower;
            }

            // Get active delegations TO this user for this proposal type
            var delegationsToUser = await _context.VotingDelegations
                .Where(d => d.DelegateId == userId && 
                           d.IsActive && 
                           (d.ExpiresAt == null || d.ExpiresAt > DateTime.UtcNow) &&
                           (d.SpecificType == null || d.SpecificType == proposal.Type))
                .ToListAsync();

            // Calculate delegated voting power
            decimal delegatedPower = 0m;
            foreach (var delegation in delegationsToUser)
            {
                var delegatorBasePower = await CalculateVotingPowerAsync(delegation.DelegatorId);
                var delegatedAmount = delegatorBasePower * ((delegation.MaxDelegationPercentage ?? 100m) / 100m);
                delegatedPower += delegatedAmount;
            }

            // Get active delegations FROM this user for this proposal type
            var delegationsFromUser = await _context.VotingDelegations
                .Where(d => d.DelegatorId == userId && 
                           d.IsActive && 
                           (d.ExpiresAt == null || d.ExpiresAt > DateTime.UtcNow) &&
                           (d.SpecificType == null || d.SpecificType == proposal.Type))
                .ToListAsync();

            // Calculate delegated away voting power
            decimal delegatedAwayPower = 0m;
            foreach (var delegation in delegationsFromUser)
            {
                var delegatedAmount = baseVotingPower * ((delegation.MaxDelegationPercentage ?? 100m) / 100m);
                delegatedAwayPower += delegatedAmount;
            }

            // Effective power = base power - delegated away + delegated to
            var effectivePower = baseVotingPower - delegatedAwayPower + delegatedPower;

            _logger.LogInformation("Effective voting power calculated: Base={BasePower}, DelegatedTo={DelegatedTo}, DelegatedAway={DelegatedAway}, Effective={EffectivePower}", 
                baseVotingPower, delegatedPower, delegatedAwayPower, effectivePower);

            return Math.Max(0m, effectivePower);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate effective voting power for user {UserId} on proposal {ProposalId}", userId, proposalId);
            return await CalculateVotingPowerAsync(userId);
        }
    }

    #endregion

    #region Delegation Management

    public async Task<VotingDelegationResponse> DelegateVotingPowerAsync(DelegateVoteRequest request)
    {
        _logger.LogInformation("Creating delegation from {DelegatorId} to {DelegateId}", request.DelegatorId, request.DelegateId);

        // Validate delegation request
        if (request.DelegatorId == request.DelegateId)
        {
            throw new InvalidOperationException("Cannot delegate to yourself");
        }

        // Check for existing active delegation
        var existingDelegation = await _context.VotingDelegations
            .FirstOrDefaultAsync(d => d.DelegatorId == request.DelegatorId && 
                                     d.DelegateId == request.DelegateId && 
                                     d.IsActive &&
                                     (d.SpecificType == null || d.SpecificType == request.SpecificType));

        if (existingDelegation != null)
        {
            throw new InvalidOperationException("Active delegation already exists");
        }

        var delegation = new VotingDelegation
        {
            Id = Guid.NewGuid(),
            DelegatorId = request.DelegatorId,
            DelegateId = request.DelegateId,
            SpecificType = request.SpecificType,
            DelegationReason = request.DelegationReason,
            MaxDelegationPercentage = request.MaxDelegationPercentage ?? 100m,
            ExpiresAt = request.ExpiresAt,
            AutoRenew = request.AutoRenew,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.VotingDelegations.Add(delegation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Delegation created successfully: {DelegationId}", delegation.Id);
        return delegation.Adapt<VotingDelegationResponse>();
    }

    public async Task<bool> RevokeVotingDelegationAsync(Guid delegationId, Guid userId, string? reason = null)
    {
        var delegation = await _context.VotingDelegations.FindAsync(delegationId);
        if (delegation == null || !delegation.IsActive)
        {
            return false;
        }

        // Only delegator can revoke
        if (delegation.DelegatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the delegator can revoke the delegation");
        }

        delegation.IsActive = false;
        delegation.RevokedAt = DateTime.UtcNow;
        delegation.RevokedById = userId;
        delegation.RevocationReason = reason;
        delegation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResponse<VotingDelegationResponse>> GetDelegationsAsync(GetDelegationsRequest request)
    {
        var query = _context.VotingDelegations.AsQueryable();

        // Apply filters
        if (request.DelegatorId.HasValue)
            query = query.Where(d => d.DelegatorId == request.DelegatorId.Value);

        if (request.DelegateId.HasValue)
            query = query.Where(d => d.DelegateId == request.DelegateId.Value);

        if (request.SpecificType.HasValue)
            query = query.Where(d => d.SpecificType == request.SpecificType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(d => d.IsActive == request.IsActive.Value);

        var totalCount = await query.CountAsync();

        var delegations = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var delegationResponses = delegations.Adapt<List<VotingDelegationResponse>>();

        return new PagedResponse<VotingDelegationResponse>
        {
            Items = delegationResponses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<List<VotingDelegationResponse>> GetUserDelegationsAsync(Guid userId, bool? isActive = null)
    {
        var query = _context.VotingDelegations
            .Where(d => d.DelegatorId == userId);

        if (isActive.HasValue)
            query = query.Where(d => d.IsActive == isActive.Value);

        var delegations = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return delegations.Adapt<List<VotingDelegationResponse>>();
    }

    public async Task<List<VotingDelegationResponse>> GetDelegationsToUserAsync(Guid delegateId, bool? isActive = null)
    {
        var query = _context.VotingDelegations
            .Where(d => d.DelegateId == delegateId);

        if (isActive.HasValue)
            query = query.Where(d => d.IsActive == isActive.Value);

        var delegations = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return delegations.Adapt<List<VotingDelegationResponse>>();
    }

    #endregion

    #region Token Service Integration

    public async Task<decimal> GetUserTokenBalanceAsync(Guid userId)
    {
        try
        {
            var response = await _tokenServiceClient.GetUserTokenBalanceAsync(userId);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return response.Content;
            }
            
            _logger.LogWarning("Failed to retrieve token balance for user {UserId}", userId);
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving token balance for user {UserId}", userId);
            return 0m;
        }
    }

    public async Task<List<TokenHolding>> GetUserTokenHoldingsAsync(Guid userId)
    {
        try
        {
            var response = await _tokenServiceClient.GetUserTokenHoldingsAsync(userId);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return response.Content;
            }
            
            _logger.LogWarning("Failed to retrieve token holdings for user {UserId}", userId);
            return new List<TokenHolding>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving token holdings for user {UserId}", userId);
            return new List<TokenHolding>();
        }
    }

    public async Task<decimal> CalculateTotalVotingPowerAsync()
    {
        try
        {
            var response = await _tokenServiceClient.GetTotalTokenSupplyAsync();
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                return response.Content;
            }
            
            _logger.LogWarning("Failed to retrieve total token supply");
            return 0m;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total token supply");
            return 0m;
        }
    }

    #endregion

    #region Validation and Business Logic

    public async Task<bool> CanUserVoteAsync(Guid userId, Guid proposalId)
    {
        try
        {
            // Check if proposal exists and is active
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null || proposal.Status != ProposalStatus.Active)
            {
                return false;
            }

            // Check if voting period is active
            var now = DateTime.UtcNow;
            if (now < proposal.VotingStartTime || now > proposal.VotingEndTime)
            {
                return false;
            }

            // Check if user has already voted
            var hasVoted = await HasUserVotedAsync(proposalId, userId);
            if (hasVoted)
            {
                return false;
            }

            // Check if user has voting power
            var votingPower = await CalculateVotingPowerAsync(userId, proposalId);
            return votingPower > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} can vote on proposal {ProposalId}", userId, proposalId);
            return false;
        }
    }

    public async Task ValidateVoteEligibilityAsync(CastVoteRequest request)
    {
        var canVote = await CanUserVoteAsync(request.VoterId, request.ProposalId);
        if (!canVote)
        {
            throw new InvalidOperationException("User is not eligible to vote on this proposal");
        }

        // Additional validation can be added here
        var votingPower = await CalculateVotingPowerAsync(request.VoterId, request.ProposalId);
        if (votingPower <= 0)
        {
            throw new InvalidOperationException("User has no voting power");
        }
    }

    public async Task UpdateProposalStatusIfNeededAsync(Guid proposalId)
    {
        try
        {
            var proposal = await _context.Proposals
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null || proposal.Status != ProposalStatus.Active)
            {
                return;
            }

            // Check if voting period has ended
            if (DateTime.UtcNow > proposal.VotingEndTime)
            {
                // Calculate final results
                var totalVotingPower = proposal.Votes.Sum(v => v.VotingPower);
                var approvalPower = proposal.Votes.Where(v => v.Choice == VoteChoice.Approve).Sum(v => v.VotingPower);
                
                var currentQuorum = totalVotingPower > 0 ? (totalVotingPower / await CalculateTotalVotingPowerAsync()) * 100 : 0;
                var currentApproval = totalVotingPower > 0 ? (approvalPower / totalVotingPower) * 100 : 0;

                // Check if quorum and approval thresholds are met
                var isQuorumMet = currentQuorum >= proposal.QuorumPercentage;
                var isApprovalMet = currentApproval >= proposal.ApprovalThreshold;

                proposal.Status = isQuorumMet && isApprovalMet ? ProposalStatus.Approved : ProposalStatus.Rejected;
                proposal.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Proposal {ProposalId} status updated to {Status}", proposalId, proposal.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating proposal status for {ProposalId}", proposalId);
        }
    }

    public async Task InvalidateVotingCacheAsync(Guid proposalId)
    {
        try
        {
            // Remove voting-related cache entries
            var cacheKeys = new[]
            {
                $"voting_analytics_{proposalId}",
                $"proposal_status_{proposalId}",
                $"quorum_met_{proposalId}",
                $"approval_met_{proposalId}"
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            _logger.LogDebug("Invalidated voting cache for proposal {ProposalId}", proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating voting cache for proposal {ProposalId}", proposalId);
        }
    }

    #endregion
}
