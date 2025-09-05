using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GovernanceService.Services.Interfaces;
using GovernanceService.Models.Responses;

namespace GovernanceService.Controllers;

/// <summary>
/// Controller for proposal execution management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ExecutionController : ControllerBase
{
    private readonly IGovernanceService _governanceService;
    private readonly ILogger<ExecutionController> _logger;

    public ExecutionController(
        IGovernanceService governanceService,
        ILogger<ExecutionController> logger)
    {
        _governanceService = governanceService;
        _logger = logger;
    }

    /// <summary>
    /// Schedule proposal execution
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Scheduled execution details</returns>
    [HttpPost("schedule/{proposalId}")]
    [ProducesResponseType(typeof(ProposalExecutionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalExecutionResponse>> ScheduleExecution(
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Scheduling execution for proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            // Check if proposal exists and is approved
            var proposal = await _governanceService.GetProposalAsync(proposalId);
            if (proposal == null)
            {
                return NotFound($"Proposal not found: {proposalId}");
            }

            if (proposal.Status != Data.Entities.ProposalStatus.Approved)
            {
                return BadRequest($"Proposal must be approved before scheduling execution. Current status: {proposal.Status}");
            }

            // Check if execution is already scheduled
            var existingExecution = await _governanceService.GetProposalExecutionAsync(proposalId);
            if (existingExecution != null)
            {
                return BadRequest($"Execution already scheduled for proposal: {proposalId}");
            }

            var result = await _governanceService.ScheduleProposalExecutionAsync(proposalId);
            
            _logger.LogInformation("Execution scheduled successfully for proposal: {ProposalId}, ExecutionId: {ExecutionId}", 
                proposalId, result.Id);
            
            return CreatedAtAction(nameof(GetExecution), new { proposalId }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid execution scheduling request: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Execution scheduling failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized execution scheduling attempt: {ProposalId}", proposalId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling execution: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while scheduling the execution");
        }
    }

    /// <summary>
    /// Execute proposal manually
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="executorId">Executor user ID</param>
    /// <returns>Execution result</returns>
    [HttpPost("execute/{proposalId}")]
    [ProducesResponseType(typeof(ProposalExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalExecutionResponse>> ExecuteProposal(
        [FromRoute] Guid proposalId,
        [FromBody] Guid executorId)
    {
        try
        {
            _logger.LogInformation("Executing proposal: {ProposalId} by executor: {ExecutorId}", proposalId, executorId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (executorId == Guid.Empty)
            {
                return BadRequest("Executor ID cannot be empty");
            }

            // Check if proposal exists and is approved
            var proposal = await _governanceService.GetProposalAsync(proposalId);
            if (proposal == null)
            {
                return NotFound($"Proposal not found: {proposalId}");
            }

            if (proposal.Status != Data.Entities.ProposalStatus.Approved)
            {
                return BadRequest($"Proposal must be approved before execution. Current status: {proposal.Status}");
            }

            // Check if proposal meets approval requirements
            var isApprovalMet = await _governanceService.IsApprovalThresholdMetAsync(proposalId);
            if (!isApprovalMet)
            {
                return BadRequest("Proposal does not meet approval threshold requirements");
            }

            var isQuorumMet = await _governanceService.IsQuorumMetAsync(proposalId);
            if (!isQuorumMet)
            {
                return BadRequest("Proposal does not meet quorum requirements");
            }

            var result = await _governanceService.ExecuteProposalAsync(proposalId, executorId);
            
            _logger.LogInformation("Proposal executed successfully: {ProposalId}, ExecutionId: {ExecutionId}, Status: {Status}", 
                proposalId, result.Id, result.Status);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid proposal execution request: {ProposalId}, {ExecutorId}", proposalId, executorId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Proposal execution failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized proposal execution attempt: {ProposalId}, {ExecutorId}", proposalId, executorId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing proposal: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while executing the proposal");
        }
    }

    /// <summary>
    /// Get pending executions
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>List of pending executions</returns>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetPendingExecutions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting pending executions - Page: {Page}, Size: {PageSize}", page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var pendingExecutions = await _governanceService.GetPendingExecutionsAsync();
            
            // Apply pagination
            var totalCount = pendingExecutions.Count;
            var skip = (page - 1) * pageSize;
            var pagedExecutions = pendingExecutions.Skip(skip).Take(pageSize).ToList();
            
            var result = new
            {
                Items = pagedExecutions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = skip + pageSize < totalCount,
                HasPreviousPage = page > 1,
                Summary = new
                {
                    TotalPending = totalCount,
                    ScheduledToday = pagedExecutions.Count(e => e.ScheduledAt.Date == DateTime.UtcNow.Date),
                    OverdueExecutions = pagedExecutions.Count(e => e.ScheduledAt < DateTime.UtcNow && e.Status == Data.Entities.ExecutionStatus.Pending),
                    FailedExecutions = pagedExecutions.Count(e => e.Status == Data.Entities.ExecutionStatus.Failed),
                    RetryingExecutions = pagedExecutions.Count(e => e.RetryCount > 0 && e.Status == Data.Entities.ExecutionStatus.Pending)
                }
            };

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid pending executions request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending executions");
            return StatusCode(500, "An error occurred while retrieving pending executions");
        }
    }

    /// <summary>
    /// Get execution status for a proposal
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <returns>Execution details</returns>
    [HttpGet("{proposalId}")]
    [ProducesResponseType(typeof(ProposalExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalExecutionResponse>> GetExecution(
        [FromRoute] Guid proposalId)
    {
        try
        {
            _logger.LogInformation("Getting execution status for proposal: {ProposalId}", proposalId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            var execution = await _governanceService.GetProposalExecutionAsync(proposalId);
            
            if (execution == null)
            {
                return NotFound($"Execution not found for proposal: {proposalId}");
            }

            return Ok(execution);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid execution request: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while retrieving the execution");
        }
    }

    /// <summary>
    /// Get execution history with filtering and pagination
    /// </summary>
    /// <param name="status">Optional execution status filter</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of records per page (default: 20, max: 100)</param>
    /// <returns>Paginated execution history</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetExecutionHistory(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Getting execution history - Status: {Status}, From: {FromDate}, To: {ToDate}, Page: {Page}, Size: {PageSize}", 
                status, fromDate, toDate, page, pageSize);

            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            // Get all executions (in a real implementation, this would be filtered at the database level)
            var allExecutions = await _governanceService.GetPendingExecutionsAsync();
            
            // Apply filters
            var filteredExecutions = allExecutions.AsEnumerable();
            
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<Data.Entities.ExecutionStatus>(status, true, out var executionStatus))
            {
                filteredExecutions = filteredExecutions.Where(e => e.Status == executionStatus);
            }
            
            if (fromDate.HasValue)
            {
                filteredExecutions = filteredExecutions.Where(e => e.CreatedAt >= fromDate.Value);
            }
            
            if (toDate.HasValue)
            {
                filteredExecutions = filteredExecutions.Where(e => e.CreatedAt <= toDate.Value);
            }
            
            // Apply pagination
            var totalCount = filteredExecutions.Count();
            var skip = (page - 1) * pageSize;
            var pagedExecutions = filteredExecutions
                .OrderByDescending(e => e.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToList();
            
            var result = new
            {
                Items = pagedExecutions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = skip + pageSize < totalCount,
                HasPreviousPage = page > 1,
                Statistics = new
                {
                    TotalExecutions = totalCount,
                    SuccessfulExecutions = pagedExecutions.Count(e => e.Status == Data.Entities.ExecutionStatus.Completed),
                    FailedExecutions = pagedExecutions.Count(e => e.Status == Data.Entities.ExecutionStatus.Failed),
                    PendingExecutions = pagedExecutions.Count(e => e.Status == Data.Entities.ExecutionStatus.Pending),
                    AverageExecutionTime = CalculateAverageExecutionTime(pagedExecutions),
                    SuccessRate = totalCount > 0 ? (decimal)pagedExecutions.Count(e => e.Status == Data.Entities.ExecutionStatus.Completed) / totalCount * 100 : 0
                }
            };

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid execution history request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution history");
            return StatusCode(500, "An error occurred while retrieving execution history");
        }
    }

    /// <summary>
    /// Get execution statistics and metrics
    /// </summary>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>Execution statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetExecutionStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            _logger.LogInformation("Getting execution statistics - From: {FromDate}, To: {ToDate}", fromDate, toDate);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest("From date cannot be greater than to date");
            }

            var allExecutions = await _governanceService.GetPendingExecutionsAsync();
            
            // Apply date filters
            var filteredExecutions = allExecutions.AsEnumerable();
            if (fromDate.HasValue)
            {
                filteredExecutions = filteredExecutions.Where(e => e.CreatedAt >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                filteredExecutions = filteredExecutions.Where(e => e.CreatedAt <= toDate.Value);
            }
            
            var executionList = filteredExecutions.ToList();
            
            var statistics = new
            {
                Overview = new
                {
                    TotalExecutions = executionList.Count,
                    SuccessfulExecutions = executionList.Count(e => e.Status == Data.Entities.ExecutionStatus.Completed),
                    FailedExecutions = executionList.Count(e => e.Status == Data.Entities.ExecutionStatus.Failed),
                    PendingExecutions = executionList.Count(e => e.Status == Data.Entities.ExecutionStatus.Pending),
                    SuccessRate = executionList.Count > 0 ? (decimal)executionList.Count(e => e.Status == Data.Entities.ExecutionStatus.Completed) / executionList.Count * 100 : 0
                },
                Performance = new
                {
                    AverageExecutionTime = CalculateAverageExecutionTime(executionList),
                    FastestExecution = CalculateFastestExecution(executionList),
                    SlowestExecution = CalculateSlowestExecution(executionList),
                    TotalGasUsed = executionList.Where(e => e.GasUsed.HasValue).Sum(e => e.GasUsed.Value),
                    AverageGasUsed = executionList.Where(e => e.GasUsed.HasValue).DefaultIfEmpty().Average(e => e?.GasUsed ?? 0),
                    TotalExecutionCost = executionList.Where(e => e.ExecutionCost.HasValue).Sum(e => e.ExecutionCost.Value)
                },
                Reliability = new
                {
                    RetryRate = executionList.Count > 0 ? (decimal)executionList.Count(e => e.RetryCount > 0) / executionList.Count * 100 : 0,
                    AverageRetries = executionList.DefaultIfEmpty().Average(e => e?.RetryCount ?? 0),
                    MaxRetries = executionList.DefaultIfEmpty().Max(e => e?.RetryCount ?? 0),
                    ExecutionsWithErrors = executionList.Count(e => !string.IsNullOrEmpty(e.ErrorMessage))
                },
                Timeline = new
                {
                    ExecutionsByDay = GroupExecutionsByDay(executionList),
                    ExecutionsByStatus = executionList.GroupBy(e => e.Status).ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    RecentActivity = executionList.Where(e => e.CreatedAt >= DateTime.UtcNow.AddDays(-7)).Count(),
                    UpcomingExecutions = executionList.Count(e => e.ScheduledAt > DateTime.UtcNow && e.Status == Data.Entities.ExecutionStatus.Pending)
                },
                Insights = new
                {
                    MostCommonErrors = GetMostCommonErrors(executionList),
                    ExecutionTrends = AnalyzeExecutionTrends(executionList),
                    PerformanceRecommendations = GeneratePerformanceRecommendations(executionList)
                }
            };

            return Ok(statistics);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid execution statistics request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution statistics");
            return StatusCode(500, "An error occurred while retrieving execution statistics");
        }
    }

    /// <summary>
    /// Retry failed execution
    /// </summary>
    /// <param name="proposalId">Proposal ID</param>
    /// <param name="executorId">Executor user ID</param>
    /// <returns>Retry execution result</returns>
    [HttpPost("retry/{proposalId}")]
    [ProducesResponseType(typeof(ProposalExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProposalExecutionResponse>> RetryExecution(
        [FromRoute] Guid proposalId,
        [FromBody] Guid executorId)
    {
        try
        {
            _logger.LogInformation("Retrying execution for proposal: {ProposalId} by executor: {ExecutorId}", proposalId, executorId);

            if (proposalId == Guid.Empty)
            {
                return BadRequest("Proposal ID cannot be empty");
            }

            if (executorId == Guid.Empty)
            {
                return BadRequest("Executor ID cannot be empty");
            }

            // Check if execution exists and can be retried
            var execution = await _governanceService.GetProposalExecutionAsync(proposalId);
            if (execution == null)
            {
                return NotFound($"Execution not found for proposal: {proposalId}");
            }

            if (execution.Status != Data.Entities.ExecutionStatus.Failed)
            {
                return BadRequest($"Only failed executions can be retried. Current status: {execution.Status}");
            }

            if (execution.RetryCount >= execution.MaxRetries)
            {
                return BadRequest($"Maximum retry attempts ({execution.MaxRetries}) exceeded for proposal: {proposalId}");
            }

            var result = await _governanceService.ExecuteProposalAsync(proposalId, executorId);
            
            _logger.LogInformation("Execution retry initiated for proposal: {ProposalId}, ExecutionId: {ExecutionId}, RetryCount: {RetryCount}", 
                proposalId, result.Id, result.RetryCount);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid execution retry request: {ProposalId}, {ExecutorId}", proposalId, executorId);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Execution retry failed: {ProposalId}", proposalId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized execution retry attempt: {ProposalId}, {ExecutorId}", proposalId, executorId);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying execution: {ProposalId}", proposalId);
            return StatusCode(500, "An error occurred while retrying the execution");
        }
    }

    #region Private Helper Methods

    private static TimeSpan CalculateAverageExecutionTime(List<ProposalExecutionResponse> executions)
    {
        var completedExecutions = executions.Where(e => e.ExecutedAt.HasValue && e.Status == Data.Entities.ExecutionStatus.Completed).ToList();
        
        if (!completedExecutions.Any()) return TimeSpan.Zero;
        
        var totalTime = completedExecutions.Sum(e => (e.ExecutedAt!.Value - e.ScheduledAt).Ticks);
        var averageTicks = totalTime / completedExecutions.Count;
        
        return new TimeSpan(averageTicks);
    }

    private static TimeSpan CalculateFastestExecution(List<ProposalExecutionResponse> executions)
    {
        var completedExecutions = executions.Where(e => e.ExecutedAt.HasValue && e.Status == Data.Entities.ExecutionStatus.Completed).ToList();
        
        if (!completedExecutions.Any()) return TimeSpan.Zero;
        
        return completedExecutions.Min(e => e.ExecutedAt!.Value - e.ScheduledAt);
    }

    private static TimeSpan CalculateSlowestExecution(List<ProposalExecutionResponse> executions)
    {
        var completedExecutions = executions.Where(e => e.ExecutedAt.HasValue && e.Status == Data.Entities.ExecutionStatus.Completed).ToList();
        
        if (!completedExecutions.Any()) return TimeSpan.Zero;
        
        return completedExecutions.Max(e => e.ExecutedAt!.Value - e.ScheduledAt);
    }

    private static Dictionary<string, int> GroupExecutionsByDay(List<ProposalExecutionResponse> executions)
    {
        return executions
            .GroupBy(e => e.CreatedAt.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static List<string> GetMostCommonErrors(List<ProposalExecutionResponse> executions)
    {
        return executions
            .Where(e => !string.IsNullOrEmpty(e.ErrorMessage))
            .GroupBy(e => e.ErrorMessage!)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => $"{g.Key} ({g.Count()} occurrences)")
            .ToList();
    }

    private static string AnalyzeExecutionTrends(List<ProposalExecutionResponse> executions)
    {
        if (executions.Count < 2) return "Insufficient data for trend analysis";
        
        var recentExecutions = executions.Where(e => e.CreatedAt >= DateTime.UtcNow.AddDays(-30)).Count();
        var olderExecutions = executions.Where(e => e.CreatedAt < DateTime.UtcNow.AddDays(-30)).Count();
        
        if (olderExecutions == 0) return "Increasing activity (new system)";
        
        var recentRate = recentExecutions / 30.0;
        var olderRate = olderExecutions / Math.Max(1, (DateTime.UtcNow - executions.Min(e => e.CreatedAt)).Days - 30);
        
        var changePercent = (recentRate - olderRate) / olderRate * 100;
        
        return changePercent switch
        {
            > 20 => "Significantly increasing execution activity",
            > 5 => "Moderately increasing execution activity",
            > -5 => "Stable execution activity",
            > -20 => "Moderately decreasing execution activity",
            _ => "Significantly decreasing execution activity"
        };
    }

    private static List<string> GeneratePerformanceRecommendations(List<ProposalExecutionResponse> executions)
    {
        var recommendations = new List<string>();
        
        var failureRate = executions.Count > 0 ? (decimal)executions.Count(e => e.Status == Data.Entities.ExecutionStatus.Failed) / executions.Count * 100 : 0;
        if (failureRate > 10)
        {
            recommendations.Add($"High failure rate ({failureRate:F1}%) - review execution logic and error handling");
        }
        
        var retryRate = executions.Count > 0 ? (decimal)executions.Count(e => e.RetryCount > 0) / executions.Count * 100 : 0;
        if (retryRate > 20)
        {
            recommendations.Add($"High retry rate ({retryRate:F1}%) - investigate underlying execution issues");
        }
        
        var avgExecutionTime = CalculateAverageExecutionTime(executions);
        if (avgExecutionTime > TimeSpan.FromMinutes(10))
        {
            recommendations.Add($"Long average execution time ({avgExecutionTime:hh\\:mm\\:ss}) - consider optimization");
        }
        
        var pendingCount = executions.Count(e => e.Status == Data.Entities.ExecutionStatus.Pending);
        if (pendingCount > 10)
        {
            recommendations.Add($"High number of pending executions ({pendingCount}) - consider increasing execution capacity");
        }
        
        if (!recommendations.Any())
        {
            recommendations.Add("Execution performance appears healthy - no immediate recommendations");
        }
        
        return recommendations;
    }

    #endregion
}
