using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GovernanceService.Services.Interfaces;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;

namespace GovernanceService.Controllers;

/// <summary>
/// Controller for governance analytics and reporting
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IGovernanceService governanceService,
        ILogger<AnalyticsController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Get voting analytics for a specific proposal
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Detailed voting analytics</returns>
    [HttpGet("voting/{proposalId}")]
    [ProducesResponseType(typeof(VotingAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VotingAnalyticsResponse>> GetVotingAnalytics(
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Getting voting analytics for proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            var analytics = await _governanceService.GetVotingAnalyticsAsync(proposalId);
            
            if (analytics == null)
            {
                return NotFound($"Voting analytics not found for proposal: {proposalId}");
            }

            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid voting analytics request: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voting analytics: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while retrieving voting analytics");
        }
    }

    /// <summary>
    /// Get overall governance statistics
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="proposalType">Optional proposal type filter</param>
    /// <param name="includeVotingPowerDistribution">Include voting power distribution analysis</param>
    /// <param name="includeParticipationMetrics">Include participation metrics</param>
    /// <returns>Comprehensive governance statistics</returns>
    [HttpGet("governance-stats")]
    [ProducesResponseType(typeof(GovernanceStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GovernanceStatsResponse>> GetGovernanceStats(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? proposalType = null,
        [FromQuery] bool includeVotingPowerDistribution = true,
        [FromQuery] bool includeParticipationMetrics = true)
    {
        try
        {
            _logger.LogInformation("Getting governance statistics - From: {FromDate}, To: {ToDate}, Type: {ProposalType}", 
                fromDate, toDate, proposalType);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var request = new GetAnalyticsRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProposalType = !string.IsNullOrEmpty(proposalType) && Enum.TryParse<Data.Entities.ProposalType>(proposalType, true, out var parsedType) ? parsedType : null,
                IncludeVotingPowerDistribution = includeVotingPowerDistribution,
                IncludeParticipationMetrics = includeParticipationMetrics
            };

            var stats = await _governanceService.GetGovernanceStatsAsync(request);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid governance stats request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance statistics");
            return StatusCode(500, "An error occurred while retrieving governance statistics");
        }
    }

    /// <summary>
    /// Get participation metrics across all governance activities
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="proposalType">Optional proposal type filter</param>
    /// <returns>Participation metrics and trends</returns>
    [HttpGet("participation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetParticipationMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? proposalType = null)
    {
        try
        {
            _logger.LogInformation("Getting participation metrics - From: {FromDate}, To: {ToDate}, Type: {ProposalType}", 
                fromDate, toDate, proposalType);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var request = new GetAnalyticsRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProposalType = !string.IsNullOrEmpty(proposalType) && Enum.TryParse<Data.Entities.ProposalType>(proposalType, true, out var parsedType) ? parsedType : null,
                IncludeParticipationMetrics = true
            };

            var stats = await _governanceService.GetGovernanceStatsAsync(request);
            
            // Extract and enhance participation data
            var participationMetrics = new
            {
                Overview = new
                {
                    TotalVoters = stats.TotalVoters,
                    ActiveDelegations = stats.ActiveDelegations,
                    TotalVotingPower = stats.TotalVotingPower,
                    ParticipationByMonth = stats.ParticipationByMonth
                },
                TopParticipants = new
                {
                    TopVoters = stats.TopVoters.Take(10),
                    TopDelegates = stats.TopDelegates.Take(10)
                },
                Trends = new
                {
                    ProposalParticipationTrend = stats.ParticipationByMonth.OrderBy(kvp => kvp.Key),
                    AverageParticipationRate = stats.ParticipationByMonth.Values.DefaultIfEmpty(0).Average(),
                    ParticipationGrowth = CalculateParticipationGrowth(stats.ParticipationByMonth)
                },
                Insights = new
                {
                    MostActiveProposalType = stats.ProposalsByType.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key,
                    DelegationUtilization = stats.ActiveDelegations > 0 ? (decimal)stats.ActiveDelegations / stats.TotalVoters * 100 : 0,
                    GovernanceHealth = CalculateGovernanceHealth(stats)
                }
            };

            return Ok(participationMetrics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid participation metrics request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting participation metrics");
            return StatusCode(500, "An error occurred while retrieving participation metrics");
        }
    }

    /// <summary>
    /// Get voting power distribution analysis
    /// </summary>
    /// <param name="includeHistorical">Include historical distribution data</param>
    /// <returns>Voting power distribution analysis</returns>
    [HttpGet("voting-power-distribution")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetVotingPowerDistribution(
        [FromQuery] bool includeHistorical = false)
    {
        try
        {
            _logger.LogInformation("Getting voting power distribution analysis - Historical: {IncludeHistorical}", includeHistorical);

            var request = new GetAnalyticsRequest
            {
                IncludeVotingPowerDistribution = true,
                IncludeParticipationMetrics = true
            };

            var stats = await _governanceService.GetGovernanceStatsAsync(request);
            var totalVotingPower = await _governanceService.CalculateTotalVotingPowerAsync();

            var distribution = new
            {
                CurrentDistribution = new
                {
                    TotalVotingPower = totalVotingPower,
                    TotalHolders = stats.TotalVoters,
                    ConcentrationMetrics = new
                    {
                        Top1PercentPower = CalculateTopPercentagePower(stats.TopVoters, 0.01m, totalVotingPower),
                        Top5PercentPower = CalculateTopPercentagePower(stats.TopVoters, 0.05m, totalVotingPower),
                        Top10PercentPower = CalculateTopPercentagePower(stats.TopVoters, 0.10m, totalVotingPower),
                        GiniCoefficient = CalculateGiniCoefficient(stats.TopVoters)
                    }
                },
                PowerRanges = new
                {
                    Whales = new { Threshold = "1M+", Count = stats.TopVoters.Count(v => v.TotalVotingPower >= 1000000), TotalPower = stats.TopVoters.Where(v => v.TotalVotingPower >= 1000000).Sum(v => v.TotalVotingPower) },
                    LargeHolders = new { Threshold = "100K-1M", Count = stats.TopVoters.Count(v => v.TotalVotingPower >= 100000 && v.TotalVotingPower < 1000000), TotalPower = stats.TopVoters.Where(v => v.TotalVotingPower >= 100000 && v.TotalVotingPower < 1000000).Sum(v => v.TotalVotingPower) },
                    MediumHolders = new { Threshold = "10K-100K", Count = stats.TopVoters.Count(v => v.TotalVotingPower >= 10000 && v.TotalVotingPower < 100000), TotalPower = stats.TopVoters.Where(v => v.TotalVotingPower >= 10000 && v.TotalVotingPower < 100000).Sum(v => v.TotalVotingPower) },
                    SmallHolders = new { Threshold = "1K-10K", Count = stats.TopVoters.Count(v => v.TotalVotingPower >= 1000 && v.TotalVotingPower < 10000), TotalPower = stats.TopVoters.Where(v => v.TotalVotingPower >= 1000 && v.TotalVotingPower < 10000).Sum(v => v.TotalVotingPower) },
                    MinorHolders = new { Threshold = "<1K", Count = stats.TopVoters.Count(v => v.TotalVotingPower < 1000), TotalPower = stats.TopVoters.Where(v => v.TotalVotingPower < 1000).Sum(v => v.TotalVotingPower) }
                },
                DecentralizationMetrics = new
                {
                    NakamotoCoefficient = CalculateNakamotoCoefficient(stats.TopVoters, totalVotingPower),
                    HerfindahlIndex = CalculateHerfindahlIndex(stats.TopVoters, totalVotingPower),
                    DecentralizationScore = CalculateDecentralizationScore(stats.TopVoters, totalVotingPower)
                },
                DelegationImpact = new
                {
                    TotalDelegations = stats.ActiveDelegations,
                    DelegatedPower = stats.TopDelegates.Sum(d => d.TotalDelegatedPower),
                    DelegationConcentration = CalculateDelegationConcentration(stats.TopDelegates)
                }
            };

            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting voting power distribution");
            return StatusCode(500, "An error occurred while retrieving voting power distribution");
        }
    }

    /// <summary>
    /// Get proposal performance analytics
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="proposalType">Optional proposal type filter</param>
    /// <returns>Proposal performance metrics</returns>
    [HttpGet("proposal-performance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetProposalPerformance(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? proposalType = null)
    {
        try
        {
            _logger.LogInformation("Getting proposal performance analytics - From: {FromDate}, To: {ToDate}, Type: {ProposalType}", 
                fromDate, toDate, proposalType);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var request = new GetAnalyticsRequest
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProposalType = !string.IsNullOrEmpty(proposalType) && Enum.TryParse<Data.Entities.ProposalType>(proposalType, true, out var parsedType) ? parsedType : null
            };

            var stats = await _governanceService.GetGovernanceStatsAsync(request);
            
            var performance = new
            {
                Overview = new
                {
                    TotalProposals = stats.TotalProposals,
                    ActiveProposals = stats.ActiveProposals,
                    ApprovedProposals = stats.ApprovedProposals,
                    RejectedProposals = stats.RejectedProposals,
                    ExecutedProposals = stats.ExecutedProposals,
                    SuccessRate = stats.TotalProposals > 0 ? (decimal)stats.ApprovedProposals / stats.TotalProposals * 100 : 0,
                    ExecutionRate = stats.ApprovedProposals > 0 ? (decimal)stats.ExecutedProposals / stats.ApprovedProposals * 100 : 0
                },
                ByType = stats.ProposalsByType.ToDictionary(kvp => kvp.Key.ToString(), kvp => new
                {
                    Count = kvp.Value,
                    Percentage = stats.TotalProposals > 0 ? (decimal)kvp.Value / stats.TotalProposals * 100 : 0
                }),
                Timeline = new
                {
                    LastProposalDate = stats.LastProposalDate,
                    NextScheduledExecution = stats.NextScheduledExecution,
                    AverageProposalsPerMonth = CalculateAverageProposalsPerMonth(stats.ParticipationByMonth)
                },
                Efficiency = new
                {
                    AverageTimeToResolution = CalculateAverageTimeToResolution(fromDate, toDate),
                    QuorumAchievementRate = CalculateQuorumAchievementRate(stats),
                    ParticipationTrend = stats.ParticipationByMonth.OrderBy(kvp => kvp.Key)
                }
            };

            return Ok(performance);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal performance request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting proposal performance analytics");
            return StatusCode(500, "An error occurred while retrieving proposal performance analytics");
        }
    }

    /// <summary>
    /// Get governance health dashboard
    /// </summary>
    /// <returns>Comprehensive governance health metrics</returns>
    [HttpGet("health-dashboard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetGovernanceHealthDashboard()
    {
        try
        {
            _logger.LogInformation("Getting governance health dashboard");

            var stats = await _governanceService.GetGovernanceStatsAsync();
            var totalVotingPower = await _governanceService.CalculateTotalVotingPowerAsync();
            var activeProposals = await _governanceService.GetActiveProposalsAsync();
            var pendingExecutions = await _governanceService.GetPendingExecutionsAsync();

            var healthScore = CalculateOverallHealthScore(stats, totalVotingPower, activeProposals.Count, pendingExecutions.Count);

            var dashboard = new
            {
                OverallHealth = new
                {
                    Score = healthScore,
                    Status = GetHealthStatus(healthScore),
                    LastUpdated = DateTime.UtcNow
                },
                KeyMetrics = new
                {
                    ActiveProposals = activeProposals.Count,
                    PendingExecutions = pendingExecutions.Count,
                    TotalVoters = stats.TotalVoters,
                    ActiveDelegations = stats.ActiveDelegations,
                    TotalVotingPower = totalVotingPower
                },
                ParticipationHealth = new
                {
                    Score = CalculateParticipationHealth(stats),
                    AverageParticipation = stats.ParticipationByMonth.Values.DefaultIfEmpty(0).Average(),
                    TrendDirection = CalculateTrendDirection(stats.ParticipationByMonth)
                },
                DecentralizationHealth = new
                {
                    Score = CalculateDecentralizationScore(stats.TopVoters, totalVotingPower),
                    NakamotoCoefficient = CalculateNakamotoCoefficient(stats.TopVoters, totalVotingPower),
                    PowerConcentration = CalculateTopPercentagePower(stats.TopVoters, 0.10m, totalVotingPower)
                },
                EfficiencyHealth = new
                {
                    Score = CalculateEfficiencyHealth(stats),
                    ExecutionRate = stats.ApprovedProposals > 0 ? (decimal)stats.ExecutedProposals / stats.ApprovedProposals * 100 : 0,
                    SuccessRate = stats.TotalProposals > 0 ? (decimal)stats.ApprovedProposals / stats.TotalProposals * 100 : 0
                },
                Alerts = GenerateGovernanceAlerts(stats, activeProposals.Count, pendingExecutions.Count),
                Recommendations = GenerateGovernanceRecommendations(stats, healthScore)
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting governance health dashboard");
            return StatusCode(500, "An error occurred while retrieving governance health dashboard");
        }
    }

    #region Private Helper Methods

    private static decimal CalculateParticipationGrowth(Dictionary<string, decimal> participationByMonth)
    {
        if (participationByMonth.Count < 2) return 0;
        
        var sortedData = participationByMonth.OrderBy(kvp => kvp.Key).ToList();
        var firstMonth = sortedData.First().Value;
        var lastMonth = sortedData.Last().Value;
        
        return firstMonth > 0 ? (lastMonth - firstMonth) / firstMonth * 100 : 0;
    }

    private static string CalculateGovernanceHealth(GovernanceStatsResponse stats)
    {
        var participationRate = stats.TotalVoters > 0 ? stats.ParticipationByMonth.Values.DefaultIfEmpty(0).Average() : 0;
        var successRate = stats.TotalProposals > 0 ? (decimal)stats.ApprovedProposals / stats.TotalProposals * 100 : 0;
        
        var healthScore = (participationRate + successRate) / 2;
        
        return healthScore switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Critical"
        };
    }

    private static decimal CalculateTopPercentagePower(List<TopVoter> topVoters, decimal percentage, decimal totalPower)
    {
        if (totalPower == 0 || !topVoters.Any()) return 0;
        
        var topCount = Math.Max(1, (int)(topVoters.Count * percentage));
        var topPower = topVoters.Take(topCount).Sum(v => v.TotalVotingPower);
        
        return topPower / totalPower * 100;
    }

    private static decimal CalculateGiniCoefficient(List<TopVoter> topVoters)
    {
        if (!topVoters.Any()) return 0;
        
        var sortedPowers = topVoters.Select(v => v.TotalVotingPower).OrderBy(p => p).ToList();
        var n = sortedPowers.Count;
        var sum = sortedPowers.Sum();
        
        if (sum == 0) return 0;
        
        var gini = 0m;
        for (int i = 0; i < n; i++)
        {
            gini += (2 * (i + 1) - n - 1) * sortedPowers[i];
        }
        
        return gini / (n * sum);
    }

    private static int CalculateNakamotoCoefficient(List<TopVoter> topVoters, decimal totalPower)
    {
        if (totalPower == 0 || !topVoters.Any()) return 0;
        
        var threshold = totalPower / 2;
        var cumulativePower = 0m;
        var count = 0;
        
        foreach (var voter in topVoters.OrderByDescending(v => v.TotalVotingPower))
        {
            cumulativePower += voter.TotalVotingPower;
            count++;
            if (cumulativePower >= threshold) break;
        }
        
        return count;
    }

    private static decimal CalculateHerfindahlIndex(List<TopVoter> topVoters, decimal totalPower)
    {
        if (totalPower == 0 || !topVoters.Any()) return 0;
        
        return topVoters.Sum(v => (v.TotalVotingPower / totalPower) * (v.TotalVotingPower / totalPower)) * 10000;
    }

    private static decimal CalculateDecentralizationScore(List<TopVoter> topVoters, decimal totalPower)
    {
        var nakamoto = CalculateNakamotoCoefficient(topVoters, totalPower);
        var herfindahl = CalculateHerfindahlIndex(topVoters, totalPower);
        var top10Percent = CalculateTopPercentagePower(topVoters, 0.10m, totalPower);
        
        // Higher Nakamoto coefficient is better (more decentralized)
        // Lower Herfindahl index is better (less concentrated)
        // Lower top 10% power is better (less concentrated)
        
        var nakamotoScore = Math.Min(100, nakamoto * 10); // Scale Nakamoto coefficient
        var herfindahlScore = Math.Max(0, 100 - herfindahl / 100); // Invert and scale Herfindahl
        var concentrationScore = Math.Max(0, 100 - top10Percent); // Invert top 10% concentration
        
        return (nakamotoScore + herfindahlScore + concentrationScore) / 3;
    }

    private static decimal CalculateDelegationConcentration(List<TopDelegate> topDelegates)
    {
        if (!topDelegates.Any()) return 0;
        
        var totalDelegatedPower = topDelegates.Sum(d => d.TotalDelegatedPower);
        if (totalDelegatedPower == 0) return 0;
        
        var top5Power = topDelegates.Take(5).Sum(d => d.TotalDelegatedPower);
        return top5Power / totalDelegatedPower * 100;
    }

    private static decimal CalculateAverageProposalsPerMonth(Dictionary<string, decimal> participationByMonth)
    {
        return participationByMonth.Count > 0 ? participationByMonth.Values.Average() : 0;
    }

    private static TimeSpan CalculateAverageTimeToResolution(DateTime? fromDate, DateTime? toDate)
    {
        // This would typically query actual proposal data
        // For now, return a reasonable estimate
        return TimeSpan.FromDays(7);
    }

    private static decimal CalculateQuorumAchievementRate(GovernanceStatsResponse stats)
    {
        // This would typically calculate based on actual quorum data
        // For now, return a reasonable estimate based on success rate
        return stats.TotalProposals > 0 ? (decimal)stats.ApprovedProposals / stats.TotalProposals * 100 : 0;
    }

    private static decimal CalculateOverallHealthScore(GovernanceStatsResponse stats, decimal totalPower, int activeProposals, int pendingExecutions)
    {
        var participationScore = CalculateParticipationHealth(stats);
        var decentralizationScore = CalculateDecentralizationScore(stats.TopVoters, totalPower);
        var efficiencyScore = CalculateEfficiencyHealth(stats);
        var activityScore = Math.Min(100, (activeProposals + pendingExecutions) * 10);
        
        return (participationScore + decentralizationScore + efficiencyScore + activityScore) / 4;
    }

    private static decimal CalculateParticipationHealth(GovernanceStatsResponse stats)
    {
        var avgParticipation = stats.ParticipationByMonth.Values.DefaultIfEmpty(0).Average();
        var delegationRate = stats.TotalVoters > 0 ? (decimal)stats.ActiveDelegations / stats.TotalVoters * 100 : 0;
        
        return (avgParticipation + delegationRate) / 2;
    }

    private static decimal CalculateEfficiencyHealth(GovernanceStatsResponse stats)
    {
        var successRate = stats.TotalProposals > 0 ? (decimal)stats.ApprovedProposals / stats.TotalProposals * 100 : 0;
        var executionRate = stats.ApprovedProposals > 0 ? (decimal)stats.ExecutedProposals / stats.ApprovedProposals * 100 : 0;
        
        return (successRate + executionRate) / 2;
    }

    private static string GetHealthStatus(decimal healthScore)
    {
        return healthScore switch
        {
            >= 90 => "Excellent",
            >= 75 => "Good",
            >= 60 => "Fair",
            >= 40 => "Poor",
            _ => "Critical"
        };
    }

    private static string CalculateTrendDirection(Dictionary<string, decimal> participationByMonth)
    {
        if (participationByMonth.Count < 2) return "Stable";
        
        var growth = CalculateParticipationGrowth(participationByMonth);
        return growth switch
        {
            > 5 => "Increasing",
            < -5 => "Decreasing",
            _ => "Stable"
        };
    }

    private static List<string> GenerateGovernanceAlerts(GovernanceStatsResponse stats, int activeProposals, int pendingExecutions)
    {
        var alerts = new List<string>();
        
        if (activeProposals == 0)
            alerts.Add("No active proposals - governance activity is low");
        
        if (pendingExecutions > 5)
            alerts.Add($"High number of pending executions ({pendingExecutions}) - execution backlog detected");
        
        if (stats.TotalVoters < 100)
            alerts.Add("Low voter participation - consider engagement initiatives");
        
        var avgParticipation = stats.ParticipationByMonth.Values.DefaultIfEmpty(0).Average();
        if (avgParticipation < 20)
            alerts.Add("Low participation rate - governance health at risk");
        
        return alerts;
    }

    private static List<string> GenerateGovernanceRecommendations(GovernanceStatsResponse stats, decimal healthScore)
    {
        var recommendations = new List<string>();
        
        if (healthScore < 60)
            recommendations.Add("Consider governance parameter adjustments to improve participation");
        
        if (stats.ActiveDelegations < stats.TotalVoters * 0.1m)
            recommendations.Add("Promote delegation features to increase governance participation");
        
        if (stats.TotalProposals > 0 && (decimal)stats.ApprovedProposals / stats.TotalProposals < 0.5m)
            recommendations.Add("Review proposal quality and requirements to improve success rate");
        
        return recommendations;
    }

    #endregion
}
