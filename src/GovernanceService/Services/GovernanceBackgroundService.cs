using GovernanceService.Data;
using GovernanceService.Data.Entities;
using GovernanceService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GovernanceService.Services;

public class GovernanceBackgroundService : IGovernanceBackgroundService
{
    private readonly GovernanceDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GovernanceBackgroundService> _logger;

    public GovernanceBackgroundService(
        GovernanceDbContext context,
        IMemoryCache cache,
        ILogger<GovernanceBackgroundService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    #region Background Processing

    public async Task ProcessExpiredProposalsAsync()
    {
        _logger.LogInformation("Processing expired proposals");

        try
        {
            var now = DateTime.UtcNow;
            
            // Find active proposals that have expired
            var expiredProposals = await _context.Proposals
                .Where(p => p.Status == ProposalStatus.Active && p.VotingEndTime <= now)
                .Include(p => p.Votes)
                .ToListAsync();

            foreach (var proposal in expiredProposals)
            {
                try
                {
                    await ProcessExpiredProposalAsync(proposal);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process expired proposal {ProposalId}", proposal.Id);
                }
            }

            _logger.LogInformation("Processed {Count} expired proposals", expiredProposals.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired proposals");
        }
    }

    public async Task ProcessScheduledExecutionsAsync()
    {
        _logger.LogInformation("Processing scheduled executions");

        try
        {
            var now = DateTime.UtcNow;
            
            // Find pending executions that are ready to run
            var readyExecutions = await _context.ProposalExecutions
                .Where(e => e.Status == ExecutionStatus.Pending && e.ScheduledAt <= now)
                .Include(e => e.Proposal)
                .ToListAsync();

            foreach (var execution in readyExecutions)
            {
                try
                {
                    // Update status to in progress
                    execution.Status = ExecutionStatus.InProgress;
                    execution.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Simulate execution (in real implementation, this would call actual execution logic)
                    await SimulateProposalExecutionAsync(execution);

                    // Mark as completed
                    execution.Status = ExecutionStatus.Completed;
                    execution.ExecutedAt = DateTime.UtcNow;
                    execution.ExecutionResult = $"Background execution completed for {execution.Proposal.Type} proposal";
                    execution.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("Executed proposal {ProposalId} in background", execution.ProposalId);
                }
                catch (Exception ex)
                {
                    // Mark as failed and increment retry count
                    execution.Status = ExecutionStatus.Failed;
                    execution.ErrorMessage = ex.Message;
                    execution.RetryCount++;
                    execution.UpdatedAt = DateTime.UtcNow;

                    // Schedule retry if under max retries
                    if (execution.RetryCount < execution.MaxRetries)
                    {
                        execution.Status = ExecutionStatus.Pending;
                        execution.ScheduledAt = DateTime.UtcNow.AddMinutes(5); // Retry in 5 minutes
                        _logger.LogWarning("Scheduled retry for failed execution {ExecutionId}, attempt {RetryCount}", 
                            execution.Id, execution.RetryCount + 1);
                    }
                    else
                    {
                        _logger.LogError(ex, "Execution {ExecutionId} failed permanently after {RetryCount} attempts", 
                            execution.Id, execution.RetryCount);
                    }
                }

                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Processed {Count} scheduled executions", readyExecutions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled executions");
        }
    }

    public async Task UpdateProposalStatusesAsync()
    {
        _logger.LogInformation("Updating proposal statuses");

        try
        {
            var now = DateTime.UtcNow;
            
            // Find pending proposals that should become active
            var pendingProposals = await _context.Proposals
                .Where(p => p.Status == ProposalStatus.Pending && p.VotingStartTime <= now)
                .ToListAsync();

            foreach (var proposal in pendingProposals)
            {
                proposal.Status = ProposalStatus.Active;
                proposal.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Activated proposal {ProposalId}", proposal.Id);
            }

            // Find active proposals that have ended and need status evaluation
            var endedProposals = await _context.Proposals
                .Where(p => p.Status == ProposalStatus.Active && p.VotingEndTime <= now)
                .Include(p => p.Votes)
                .ToListAsync();

            foreach (var proposal in endedProposals)
            {
                await EvaluateProposalOutcomeAsync(proposal);
            }

            if (pendingProposals.Any() || endedProposals.Any())
            {
                await _context.SaveChangesAsync();
                await InvalidateGovernanceCacheAsync();
            }

            _logger.LogInformation("Updated {PendingCount} pending and {EndedCount} ended proposals", 
                pendingProposals.Count, endedProposals.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating proposal statuses");
        }
    }

    public async Task ProcessDelegationExpirationsAsync()
    {
        _logger.LogInformation("Processing delegation expirations");

        try
        {
            var now = DateTime.UtcNow;
            
            // Find expired delegations
            var expiredDelegations = await _context.VotingDelegations
                .Where(d => d.IsActive && d.ExpiresAt.HasValue && d.ExpiresAt <= now)
                .ToListAsync();

            foreach (var delegation in expiredDelegations)
            {
                if (delegation.AutoRenew)
                {
                    // Auto-renew the delegation
                    delegation.ExpiresAt = now.AddMonths(1); // Renew for 1 month
                    delegation.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Auto-renewed delegation {DelegationId}", delegation.Id);
                }
                else
                {
                    // Expire the delegation
                    delegation.IsActive = false;
                    delegation.RevokedAt = DateTime.UtcNow;
                    delegation.RevocationReason = "Expired";
                    delegation.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Expired delegation {DelegationId}", delegation.Id);
                }
            }

            if (expiredDelegations.Any())
            {
                await _context.SaveChangesAsync();
                await InvalidateGovernanceCacheAsync();
            }

            _logger.LogInformation("Processed {Count} delegation expirations", expiredDelegations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing delegation expirations");
        }
    }

    #endregion

    #region Cache Management

    public async Task InvalidateGovernanceCacheAsync()
    {
        try
        {
            // Remove governance-related cache entries
            var cacheKeys = new[]
            {
                "governance_stats",
                "active_proposals",
                "proposals_requiring_action"
            };

            foreach (var key in cacheKeys)
            {
                _cache.Remove(key);
            }

            // Remove proposal-specific cache entries (this is a simplified approach)
            // In a real implementation, you might want to track cache keys more systematically
            var proposalIds = await _context.Proposals
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var proposalId in proposalIds)
            {
                var proposalCacheKeys = new[]
                {
                    $"voting_analytics_{proposalId}",
                    $"proposal_status_{proposalId}",
                    $"quorum_met_{proposalId}",
                    $"approval_met_{proposalId}",
                    $"voting_power_{proposalId}"
                };

                foreach (var key in proposalCacheKeys)
                {
                    _cache.Remove(key);
                }
            }

            _logger.LogDebug("Invalidated governance cache entries");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating governance cache");
        }
    }

    public async Task RefreshGovernanceCacheAsync()
    {
        _logger.LogInformation("Refreshing governance cache");

        try
        {
            // First invalidate existing cache
            await InvalidateGovernanceCacheAsync();

            // Pre-warm important cache entries
            await PreWarmCacheAsync();

            _logger.LogInformation("Governance cache refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing governance cache");
        }
    }

    public async Task CleanupExpiredCacheEntriesAsync()
    {
        _logger.LogInformation("Cleaning up expired cache entries");

        try
        {
            // The IMemoryCache doesn't provide a direct way to enumerate and clean up entries
            // This is typically handled automatically by the cache implementation
            // For custom cleanup, you would need to track cache keys separately

            // For now, we'll just log that cleanup was attempted
            _logger.LogInformation("Cache cleanup completed (automatic cleanup by IMemoryCache)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache entries");
        }
    }

    #endregion

    #region Maintenance Tasks

    public async Task PerformDailyMaintenanceAsync()
    {
        _logger.LogInformation("Performing daily maintenance tasks");

        try
        {
            // Process expired proposals
            await ProcessExpiredProposalsAsync();

            // Update proposal statuses
            await UpdateProposalStatusesAsync();

            // Process delegation expirations
            await ProcessDelegationExpirationsAsync();

            // Process scheduled executions
            await ProcessScheduledExecutionsAsync();

            // Refresh cache
            await RefreshGovernanceCacheAsync();

            // Archive old proposals (older than 1 year)
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);
            await ArchiveOldProposalsAsync(oneYearAgo);

            // Cleanup failed executions
            await CleanupFailedExecutionsAsync();

            // Process auto-renewal delegations
            await ProcessAutoRenewalDelegationsAsync();

            _logger.LogInformation("Daily maintenance completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daily maintenance");
        }
    }

    public async Task ArchiveOldProposalsAsync(DateTime cutoffDate)
    {
        _logger.LogInformation("Archiving proposals older than {CutoffDate}", cutoffDate);

        try
        {
            // Find old completed proposals
            var oldProposals = await _context.Proposals
                .Where(p => p.CreatedAt < cutoffDate && 
                           (p.Status == ProposalStatus.Approved || 
                            p.Status == ProposalStatus.Rejected || 
                            p.Status == ProposalStatus.Cancelled))
                .ToListAsync();

            // In a real implementation, you might move these to an archive table
            // For now, we'll just log the count
            _logger.LogInformation("Found {Count} proposals eligible for archiving", oldProposals.Count);

            // You could implement actual archiving logic here
            // For example: move to archive table, compress data, etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old proposals");
        }
    }

    public async Task CleanupFailedExecutionsAsync()
    {
        _logger.LogInformation("Cleaning up failed executions");

        try
        {
            // Find executions that have failed permanently (exceeded max retries)
            var failedExecutions = await _context.ProposalExecutions
                .Where(e => e.Status == ExecutionStatus.Failed && e.RetryCount >= e.MaxRetries)
                .Where(e => e.UpdatedAt < DateTime.UtcNow.AddDays(-7)) // Older than 7 days
                .ToListAsync();

            foreach (var execution in failedExecutions)
            {
                // Log the permanent failure
                _logger.LogWarning("Permanently failed execution {ExecutionId} for proposal {ProposalId}: {ErrorMessage}", 
                    execution.Id, execution.ProposalId, execution.ErrorMessage);

                // You could implement cleanup logic here
                // For example: move to failed executions table, send notifications, etc.
            }

            _logger.LogInformation("Found {Count} permanently failed executions", failedExecutions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up failed executions");
        }
    }

    public async Task ProcessAutoRenewalDelegationsAsync()
    {
        _logger.LogInformation("Processing auto-renewal delegations");

        try
        {
            var now = DateTime.UtcNow;
            var renewalThreshold = now.AddDays(7); // Renew delegations expiring within 7 days

            // Find delegations that need auto-renewal
            var delegationsToRenew = await _context.VotingDelegations
                .Where(d => d.IsActive && 
                           d.AutoRenew && 
                           d.ExpiresAt.HasValue && 
                           d.ExpiresAt <= renewalThreshold)
                .ToListAsync();

            foreach (var delegation in delegationsToRenew)
            {
                // Extend the expiration date
                delegation.ExpiresAt = delegation.ExpiresAt.Value.AddMonths(1);
                delegation.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Auto-renewed delegation {DelegationId} until {ExpiresAt}", 
                    delegation.Id, delegation.ExpiresAt);
            }

            if (delegationsToRenew.Any())
            {
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Processed {Count} auto-renewal delegations", delegationsToRenew.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-renewal delegations");
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task ProcessExpiredProposalAsync(Proposal proposal)
    {
        _logger.LogInformation("Processing expired proposal {ProposalId}", proposal.Id);

        // Calculate voting results
        var totalVotingPower = proposal.Votes.Sum(v => v.VotingPower);
        var approvalPower = proposal.Votes.Where(v => v.Choice == VoteChoice.Approve).Sum(v => v.VotingPower);

        // Calculate percentages (simplified - in real implementation, use total system voting power)
        var currentQuorum = totalVotingPower; // Simplified
        var currentApproval = totalVotingPower > 0 ? (approvalPower / totalVotingPower) * 100 : 0;

        // Determine outcome
        var isQuorumMet = currentQuorum >= proposal.QuorumPercentage;
        var isApprovalMet = currentApproval >= proposal.ApprovalThreshold;

        if (isQuorumMet && isApprovalMet)
        {
            proposal.Status = ProposalStatus.Approved;
            _logger.LogInformation("Proposal {ProposalId} approved: Quorum={Quorum}%, Approval={Approval}%", 
                proposal.Id, currentQuorum, currentApproval);
        }
        else
        {
            proposal.Status = ProposalStatus.Rejected;
            _logger.LogInformation("Proposal {ProposalId} rejected: Quorum={Quorum}%, Approval={Approval}%", 
                proposal.Id, currentQuorum, currentApproval);
        }

        proposal.UpdatedAt = DateTime.UtcNow;
    }

    private async Task EvaluateProposalOutcomeAsync(Proposal proposal)
    {
        await ProcessExpiredProposalAsync(proposal);
    }

    private async Task SimulateProposalExecutionAsync(ProposalExecution execution)
    {
        // Simulate execution time based on proposal type
        var delay = execution.Proposal.Type switch
        {
            ProposalType.Treasury => 200,
            ProposalType.Upgrade => 500,
            ProposalType.Parameter => 100,
            ProposalType.Emergency => 50,
            _ => 150
        };

        await Task.Delay(delay);

        // Simulate potential execution failure (5% chance)
        if (Random.Shared.Next(100) < 5)
        {
            throw new InvalidOperationException($"Simulated execution failure for {execution.Proposal.Type} proposal");
        }
    }

    private async Task PreWarmCacheAsync()
    {
        try
        {
            // Pre-warm governance stats cache
            // This would typically call the analytics service to populate cache
            _logger.LogDebug("Pre-warming governance cache");

            // In a real implementation, you would call:
            // await _analyticsService.GetGovernanceStatsAsync();
            // await _analyticsService.GetActiveProposalsAsync();
            // etc.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pre-warming cache");
        }
    }

    #endregion
}
