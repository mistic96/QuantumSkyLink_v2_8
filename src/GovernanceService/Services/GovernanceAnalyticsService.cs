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

public class GovernanceAnalyticsService : IGovernanceAnalyticsService
{
    private readonly GovernanceDbContext _context;
    private readonly IGovernanceVotingService _votingService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GovernanceAnalyticsService> _logger;

    public GovernanceAnalyticsService(
        GovernanceDbContext context,
        IGovernanceVotingService votingService,
        IMemoryCache cache,
        ILogger<GovernanceAnalyticsService> logger)
    {
        _context = context;
        _votingService = votingService;
        _cache = cache;
        _logger = logger;
    }

    #region Analytics and Reporting

    public async Task<VotingAnalyticsResponse> GetVotingAnalyticsAsync(Guid proposalId)
    {
        _logger.LogInformation("Calculating voting analytics for proposal {ProposalId}", proposalId);

        try
        {
            // Check cache first
            var cacheKey = $"voting_analytics_{proposalId}";
            if (_cache.TryGetValue(cacheKey, out VotingAnalyticsResponse? cachedAnalytics))
            {
                return cachedAnalytics!;
            }

            var proposal = await _context.Proposals
                .Include(p => p.Votes)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null)
            {
                throw new ArgumentException($"Proposal not found: {proposalId}");
            }

            // Calculate basic metrics
            var totalVoters = proposal.Votes.Count;
            var totalVotingPower = proposal.Votes.Sum(v => v.VotingPower);
            var totalSystemVotingPower = await _votingService.CalculateTotalVotingPowerAsync();

            var currentQuorum = totalSystemVotingPower > 0 ? (totalVotingPower / totalSystemVotingPower) * 100 : 0;
            var currentApproval = await CalculateCurrentApprovalAsync(proposalId);

            // Calculate vote breakdown
            var voteBreakdown = new VoteBreakdown
            {
                ApprovalVotes = proposal.Votes.Count(v => v.Choice == VoteChoice.Approve),
                RejectionVotes = proposal.Votes.Count(v => v.Choice == VoteChoice.Reject),
                AbstainVotes = proposal.Votes.Count(v => v.Choice == VoteChoice.Abstain),
                ApprovalPower = proposal.Votes.Where(v => v.Choice == VoteChoice.Approve).Sum(v => v.VotingPower),
                RejectionPower = proposal.Votes.Where(v => v.Choice == VoteChoice.Reject).Sum(v => v.VotingPower),
                AbstainPower = proposal.Votes.Where(v => v.Choice == VoteChoice.Abstain).Sum(v => v.VotingPower)
            };

            // Calculate percentages
            if (totalVotingPower > 0)
            {
                voteBreakdown.ApprovalPercentage = (voteBreakdown.ApprovalPower / totalVotingPower) * 100;
                voteBreakdown.RejectionPercentage = (voteBreakdown.RejectionPower / totalVotingPower) * 100;
                voteBreakdown.AbstainPercentage = (voteBreakdown.AbstainPower / totalVotingPower) * 100;
            }

            // Calculate participation metrics
            var participationMetrics = await CalculateParticipationMetricsAsync(proposalId);

            // Calculate voting power distribution
            var votingPowerDistribution = await CalculateVotingPowerDistributionAsync(proposalId);

            // Calculate time remaining
            TimeSpan? timeRemaining = null;
            if (proposal.Status == ProposalStatus.Active && DateTime.UtcNow < proposal.VotingEndTime)
            {
                timeRemaining = proposal.VotingEndTime - DateTime.UtcNow;
            }

            var analytics = new VotingAnalyticsResponse
            {
                ProposalId = proposalId,
                ProposalTitle = proposal.Title,
                ProposalType = proposal.Type,
                ProposalStatus = proposal.Status,
                TotalVoters = totalVoters,
                TotalVotingPower = totalVotingPower,
                QuorumRequired = proposal.QuorumPercentage,
                CurrentQuorum = currentQuorum,
                ApprovalThreshold = proposal.ApprovalThreshold,
                CurrentApproval = currentApproval,
                VoteBreakdown = voteBreakdown,
                Participation = participationMetrics,
                VotingPowerDistribution = votingPowerDistribution,
                VotingStartTime = proposal.VotingStartTime,
                VotingEndTime = proposal.VotingEndTime,
                TimeRemaining = timeRemaining
            };

            // Cache for 2 minutes
            _cache.Set(cacheKey, analytics, TimeSpan.FromMinutes(2));

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate voting analytics for proposal {ProposalId}", proposalId);
            throw;
        }
    }

    public async Task<GovernanceStatsResponse> GetGovernanceStatsAsync(GetAnalyticsRequest? request = null)
    {
        _logger.LogInformation("Calculating governance statistics");

        try
        {
            var cacheKey = "governance_stats";
            if (_cache.TryGetValue(cacheKey, out GovernanceStatsResponse? cachedStats))
            {
                return cachedStats!;
            }

            // Basic proposal statistics
            var totalProposals = await _context.Proposals.CountAsync();
            var activeProposals = await _context.Proposals.CountAsync(p => p.Status == ProposalStatus.Active);
            var approvedProposals = await _context.Proposals.CountAsync(p => p.Status == ProposalStatus.Approved);
            var rejectedProposals = await _context.Proposals.CountAsync(p => p.Status == ProposalStatus.Rejected);
            var executedProposals = await _context.ProposalExecutions.CountAsync(e => e.Status == ExecutionStatus.Completed);

            // Voting statistics
            var totalVotingPower = await _votingService.CalculateTotalVotingPowerAsync();
            var totalVoters = await _context.Votes.Select(v => v.VoterId).Distinct().CountAsync();
            var activeDelegations = await _context.VotingDelegations.CountAsync(d => d.IsActive);

            // Proposals by type
            var proposalsByType = await _context.Proposals
                .GroupBy(p => p.Type)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            // Participation by month (last 12 months)
            var participationByMonth = await CalculateParticipationByMonthAsync();

            // Last proposal date
            var lastProposalDate = await _context.Proposals
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            // Next scheduled execution
            var nextScheduledExecution = await _context.ProposalExecutions
                .Where(e => e.Status == ExecutionStatus.Pending)
                .OrderBy(e => e.ScheduledAt)
                .Select(e => e.ScheduledAt)
                .FirstOrDefaultAsync();

            // Top voters
            var topVoters = await CalculateTopVotersAsync();

            // Top delegates
            var topDelegates = await CalculateTopDelegatesAsync();

            var stats = new GovernanceStatsResponse
            {
                TotalProposals = totalProposals,
                ActiveProposals = activeProposals,
                ApprovedProposals = approvedProposals,
                RejectedProposals = rejectedProposals,
                ExecutedProposals = executedProposals,
                TotalVotingPower = totalVotingPower,
                TotalVoters = totalVoters,
                ActiveDelegations = activeDelegations,
                ProposalsByType = proposalsByType,
                ParticipationByMonth = participationByMonth,
                LastProposalDate = lastProposalDate == default ? null : lastProposalDate,
                NextScheduledExecution = nextScheduledExecution == default ? null : nextScheduledExecution,
                TopVoters = topVoters,
                TopDelegates = topDelegates
            };

            // Cache for 5 minutes
            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(5));

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate governance statistics");
            throw;
        }
    }

    public async Task<List<ProposalResponse>> GetActiveProposalsAsync()
    {
        _logger.LogInformation("Retrieving active proposals");

        var proposals = await _context.Proposals
            .Where(p => p.Status == ProposalStatus.Active)
            .Include(p => p.Votes)
            .OrderBy(p => p.VotingEndTime)
            .ToListAsync();

        return proposals.Adapt<List<ProposalResponse>>();
    }

    public async Task<List<ProposalResponse>> GetProposalsRequiringActionAsync()
    {
        _logger.LogInformation("Retrieving proposals requiring action");

        var now = DateTime.UtcNow;

        // Proposals that need status updates or execution
        var proposals = await _context.Proposals
            .Where(p => 
                (p.Status == ProposalStatus.Active && p.VotingEndTime <= now) ||
                (p.Status == ProposalStatus.Approved && !_context.ProposalExecutions.Any(e => e.ProposalId == p.Id)))
            .Include(p => p.Votes)
            .OrderBy(p => p.VotingEndTime)
            .ToListAsync();

        return proposals.Adapt<List<ProposalResponse>>();
    }

    #endregion

    #region Proposal Status Checking

    public async Task<bool> IsProposalActiveAsync(Guid proposalId)
    {
        try
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;
            return proposal.Status == ProposalStatus.Active &&
                   now >= proposal.VotingStartTime &&
                   now <= proposal.VotingEndTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if proposal {ProposalId} is active", proposalId);
            return false;
        }
    }

    public async Task<bool> IsQuorumMetAsync(Guid proposalId)
    {
        try
        {
            var cacheKey = $"quorum_met_{proposalId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var currentQuorum = await CalculateCurrentQuorumAsync(proposalId);
            var proposal = await _context.Proposals.FindAsync(proposalId);
            
            if (proposal == null)
            {
                return false;
            }

            var isQuorumMet = currentQuorum >= proposal.QuorumPercentage;

            // Cache for 1 minute
            _cache.Set(cacheKey, isQuorumMet, TimeSpan.FromMinutes(1));

            return isQuorumMet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking quorum for proposal {ProposalId}", proposalId);
            return false;
        }
    }

    public async Task<bool> IsApprovalThresholdMetAsync(Guid proposalId)
    {
        try
        {
            var cacheKey = $"approval_met_{proposalId}";
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                return cachedResult;
            }

            var currentApproval = await CalculateCurrentApprovalAsync(proposalId);
            var proposal = await _context.Proposals.FindAsync(proposalId);
            
            if (proposal == null)
            {
                return false;
            }

            var isApprovalMet = currentApproval >= proposal.ApprovalThreshold;

            // Cache for 1 minute
            _cache.Set(cacheKey, isApprovalMet, TimeSpan.FromMinutes(1));

            return isApprovalMet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking approval threshold for proposal {ProposalId}", proposalId);
            return false;
        }
    }

    #endregion

    #region Advanced Analytics Calculations

    public async Task<decimal> CalculateCurrentQuorumAsync(Guid proposalId)
    {
        try
        {
            var totalVotingPower = await _context.Votes
                .Where(v => v.ProposalId == proposalId)
                .SumAsync(v => v.VotingPower);

            var totalSystemVotingPower = await _votingService.CalculateTotalVotingPowerAsync();

            return totalSystemVotingPower > 0 ? (totalVotingPower / totalSystemVotingPower) * 100 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating current quorum for proposal {ProposalId}", proposalId);
            return 0;
        }
    }

    public async Task<decimal> CalculateCurrentApprovalAsync(Guid proposalId)
    {
        try
        {
            var votes = await _context.Votes
                .Where(v => v.ProposalId == proposalId)
                .ToListAsync();

            var totalVotingPower = votes.Sum(v => v.VotingPower);
            var approvalPower = votes.Where(v => v.Choice == VoteChoice.Approve).Sum(v => v.VotingPower);

            return totalVotingPower > 0 ? (approvalPower / totalVotingPower) * 100 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating current approval for proposal {ProposalId}", proposalId);
            return 0;
        }
    }

    public async Task<ParticipationMetrics> CalculateParticipationMetricsAsync(Guid proposalId)
    {
        try
        {
            var votes = await _context.Votes
                .Where(v => v.ProposalId == proposalId)
                .ToListAsync();

            var totalVoters = votes.Count;
            var delegatedVotes = votes.Count(v => v.IsDelegated);
            var totalVotingPower = votes.Sum(v => v.VotingPower);

            // Estimate eligible voters (this could be improved with actual user count)
            var eligibleVoters = Math.Max(totalVoters, 100); // Minimum assumption

            var participationRate = eligibleVoters > 0 ? (decimal)totalVoters / eligibleVoters * 100 : 0;
            var delegationRate = totalVoters > 0 ? (decimal)delegatedVotes / totalVoters * 100 : 0;

            var averageVotingPower = totalVoters > 0 ? totalVotingPower / totalVoters : 0;
            var medianVotingPower = CalculateMedianVotingPower(votes.Select(v => v.VotingPower).ToList());

            return new ParticipationMetrics
            {
                EligibleVoters = eligibleVoters,
                ActualVoters = totalVoters,
                ParticipationRate = participationRate,
                DelegatedVotes = delegatedVotes,
                DelegationRate = delegationRate,
                AverageVotingPower = averageVotingPower,
                MedianVotingPower = medianVotingPower
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating participation metrics for proposal {ProposalId}", proposalId);
            return new ParticipationMetrics();
        }
    }

    public async Task<List<VotingPowerDistribution>> CalculateVotingPowerDistributionAsync(Guid proposalId)
    {
        try
        {
            var votes = await _context.Votes
                .Where(v => v.ProposalId == proposalId)
                .Select(v => v.VotingPower)
                .ToListAsync();

            if (!votes.Any())
            {
                return new List<VotingPowerDistribution>();
            }

            var totalPower = votes.Sum();
            var distribution = new List<VotingPowerDistribution>();

            // Define power ranges
            var ranges = new[]
            {
                new { Min = 0m, Max = 100m, Label = "0-100" },
                new { Min = 100m, Max = 1000m, Label = "100-1K" },
                new { Min = 1000m, Max = 10000m, Label = "1K-10K" },
                new { Min = 10000m, Max = 100000m, Label = "10K-100K" },
                new { Min = 100000m, Max = decimal.MaxValue, Label = "100K+" }
            };

            foreach (var range in ranges)
            {
                var votersInRange = votes.Count(v => v >= range.Min && v < range.Max);
                var powerInRange = votes.Where(v => v >= range.Min && v < range.Max).Sum();

                if (votersInRange > 0)
                {
                    distribution.Add(new VotingPowerDistribution
                    {
                        Range = range.Label,
                        VoterCount = votersInRange,
                        TotalPower = powerInRange,
                        Percentage = totalPower > 0 ? (powerInRange / totalPower) * 100 : 0
                    });
                }
            }

            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating voting power distribution for proposal {ProposalId}", proposalId);
            return new List<VotingPowerDistribution>();
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<Dictionary<string, decimal>> CalculateParticipationByMonthAsync()
    {
        try
        {
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
            
            var monthlyParticipation = await _context.Votes
                .Where(v => v.CreatedAt >= twelveMonthsAgo)
                .GroupBy(v => new { v.CreatedAt.Year, v.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    VoterCount = g.Select(v => v.VoterId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.Month, x => (decimal)x.VoterCount);

            return monthlyParticipation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating participation by month");
            return new Dictionary<string, decimal>();
        }
    }

    private async Task<List<TopVoter>> CalculateTopVotersAsync()
    {
        try
        {
            var topVoters = await _context.Votes
                .GroupBy(v => v.VoterId)
                .Select(g => new TopVoter
                {
                    VoterId = g.Key,
                    ProposalsVoted = g.Select(v => v.ProposalId).Distinct().Count(),
                    TotalVotingPower = g.Sum(v => v.VotingPower),
                    ParticipationRate = 0 // Would need total proposals to calculate
                })
                .OrderByDescending(tv => tv.ProposalsVoted)
                .Take(10)
                .ToListAsync();

            return topVoters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating top voters");
            return new List<TopVoter>();
        }
    }

    private async Task<List<TopDelegate>> CalculateTopDelegatesAsync()
    {
        try
        {
            var topDelegates = await _context.VotingDelegations
                .Where(d => d.IsActive)
                .GroupBy(d => d.DelegateId)
                .Select(g => new TopDelegate
                {
                    DelegateId = g.Key,
                    DelegationsReceived = g.Count(),
                    TotalDelegatedPower = g.Sum(d => d.MaxDelegationPercentage ?? 100m),
                    ProposalsVotedOn = 0 // Would need to calculate from votes
                })
                .OrderByDescending(td => td.DelegationsReceived)
                .Take(10)
                .ToListAsync();

            return topDelegates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating top delegates");
            return new List<TopDelegate>();
        }
    }

    private decimal CalculateMedianVotingPower(List<decimal> votingPowers)
    {
        if (!votingPowers.Any())
            return 0;

        var sorted = votingPowers.OrderBy(x => x).ToList();
        var count = sorted.Count;

        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
        }
        else
        {
            return sorted[count / 2];
        }
    }

    #endregion
}
