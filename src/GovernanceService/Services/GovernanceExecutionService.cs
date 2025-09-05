using GovernanceService.Data;
using GovernanceService.Data.Entities;
using GovernanceService.Models.Requests;
using GovernanceService.Models.Responses;
using GovernanceService.Services.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovernanceService.Services;

public class GovernanceExecutionService : IGovernanceExecutionService
{
    private readonly GovernanceDbContext _context;
    private readonly ILogger<GovernanceExecutionService> _logger;

    public GovernanceExecutionService(
        GovernanceDbContext context,
        ILogger<GovernanceExecutionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Proposal Execution

    public async Task<ProposalExecutionResponse> ScheduleProposalExecutionAsync(Guid proposalId)
    {
        _logger.LogInformation("Scheduling execution for proposal {ProposalId}", proposalId);

        try
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null)
            {
                throw new ArgumentException($"Proposal not found: {proposalId}");
            }

            // Validate proposal is approved
            if (proposal.Status != ProposalStatus.Approved)
            {
                throw new InvalidOperationException($"Cannot schedule execution for proposal in status: {proposal.Status}");
            }

            // Check if execution already exists
            var existingExecution = await _context.ProposalExecutions
                .FirstOrDefaultAsync(e => e.ProposalId == proposalId);

            if (existingExecution != null)
            {
                _logger.LogInformation("Execution already exists for proposal {ProposalId}: {ExecutionId}", proposalId, existingExecution.Id);
                return existingExecution.Adapt<ProposalExecutionResponse>();
            }

            // Create new execution record
            var execution = new ProposalExecution
            {
                Id = Guid.NewGuid(),
                ProposalId = proposalId,
                Status = ExecutionStatus.Pending,
                ScheduledAt = DateTime.UtcNow,
                ExecutionParameters = proposal.ExecutionParameters,
                MaxRetries = 3,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ProposalExecutions.Add(execution);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Execution scheduled successfully for proposal {ProposalId}: {ExecutionId}", proposalId, execution.Id);
            return execution.Adapt<ProposalExecutionResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule execution for proposal {ProposalId}", proposalId);
            throw;
        }
    }

    public async Task<ProposalExecutionResponse> ExecuteProposalAsync(Guid proposalId, Guid executorId)
    {
        _logger.LogInformation("Executing proposal {ProposalId} by executor {ExecutorId}", proposalId, executorId);

        try
        {
            var execution = await _context.ProposalExecutions
                .FirstOrDefaultAsync(e => e.ProposalId == proposalId);

            if (execution == null)
            {
                throw new ArgumentException($"No execution found for proposal: {proposalId}");
            }

            // Validate execution can be performed
            if (execution.Status != ExecutionStatus.Pending && execution.Status != ExecutionStatus.Failed)
            {
                throw new InvalidOperationException($"Cannot execute proposal in status: {execution.Status}");
            }

            // Update execution status
            execution.Status = ExecutionStatus.InProgress;
            execution.ExecutedById = executorId;
            execution.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            try
            {
                // Perform the actual execution based on proposal type
                var proposal = await _context.Proposals.FindAsync(proposalId);
                var executionResult = await PerformProposalExecutionAsync(proposal!, execution);

                // Update execution with success
                execution.Status = ExecutionStatus.Completed;
                execution.ExecutedAt = DateTime.UtcNow;
                execution.ExecutionResult = executionResult;
                execution.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Proposal {ProposalId} executed successfully", proposalId);
            }
            catch (Exception executionEx)
            {
                // Update execution with failure
                execution.Status = ExecutionStatus.Failed;
                execution.ErrorMessage = executionEx.Message;
                execution.RetryCount++;
                execution.UpdatedAt = DateTime.UtcNow;

                _logger.LogError(executionEx, "Failed to execute proposal {ProposalId}, retry count: {RetryCount}", proposalId, execution.RetryCount);

                // Schedule retry if under max retries
                if (execution.RetryCount < execution.MaxRetries)
                {
                    execution.Status = ExecutionStatus.Pending;
                    _logger.LogInformation("Scheduled retry for proposal {ProposalId}, attempt {RetryCount}", proposalId, execution.RetryCount + 1);
                }
            }

            await _context.SaveChangesAsync();
            return execution.Adapt<ProposalExecutionResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute proposal {ProposalId}", proposalId);
            throw;
        }
    }

    public async Task<List<ProposalExecutionResponse>> GetPendingExecutionsAsync()
    {
        _logger.LogInformation("Retrieving pending executions");

        var executions = await _context.ProposalExecutions
            .Where(e => e.Status == ExecutionStatus.Pending)
            .OrderBy(e => e.ScheduledAt)
            .ToListAsync();

        return executions.Adapt<List<ProposalExecutionResponse>>();
    }

    public async Task<ProposalExecutionResponse?> GetProposalExecutionAsync(Guid proposalId)
    {
        _logger.LogInformation("Retrieving execution for proposal {ProposalId}", proposalId);

        var execution = await _context.ProposalExecutions
            .FirstOrDefaultAsync(e => e.ProposalId == proposalId);

        return execution?.Adapt<ProposalExecutionResponse>();
    }

    #endregion

    #region Background Processing for Execution

    public async Task ProcessScheduledExecutionsAsync()
    {
        _logger.LogInformation("Processing scheduled executions");

        try
        {
            var pendingExecutions = await _context.ProposalExecutions
                .Where(e => e.Status == ExecutionStatus.Pending)
                .Where(e => e.ScheduledAt <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var execution in pendingExecutions)
            {
                try
                {
                    // Execute with system executor ID (could be configurable)
                    var systemExecutorId = Guid.Empty; // System executor
                    await ExecuteProposalAsync(execution.ProposalId, systemExecutorId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process scheduled execution {ExecutionId} for proposal {ProposalId}", 
                        execution.Id, execution.ProposalId);
                }
            }

            _logger.LogInformation("Processed {Count} scheduled executions", pendingExecutions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled executions");
        }
    }

    public async Task<bool> IsProposalReadyForExecutionAsync(Guid proposalId)
    {
        try
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null)
            {
                return false;
            }

            // Check if proposal is approved
            if (proposal.Status != ProposalStatus.Approved)
            {
                return false;
            }

            // Check if execution delay has passed
            var executionTime = proposal.VotingEndTime.Add(TimeSpan.FromHours(1)); // Default 1 hour delay
            if (DateTime.UtcNow < executionTime)
            {
                return false;
            }

            // Check if not already executed
            var existingExecution = await _context.ProposalExecutions
                .FirstOrDefaultAsync(e => e.ProposalId == proposalId && e.Status == ExecutionStatus.Completed);

            return existingExecution == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if proposal {ProposalId} is ready for execution", proposalId);
            return false;
        }
    }

    public async Task<bool> ValidateExecutionParametersAsync(Guid proposalId)
    {
        try
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null)
            {
                return false;
            }

            // Basic validation of execution parameters
            if (string.IsNullOrEmpty(proposal.ExecutionParameters))
            {
                // Some proposal types require execution parameters
                var requiresParameters = proposal.Type switch
                {
                    ProposalType.Treasury => true,
                    ProposalType.Parameter => true,
                    ProposalType.Upgrade => true,
                    _ => false
                };

                return !requiresParameters;
            }

            // Additional validation based on proposal type
            return proposal.Type switch
            {
                ProposalType.Treasury => ValidateTreasuryExecution(proposal.ExecutionParameters),
                ProposalType.Parameter => ValidateParameterExecution(proposal.ExecutionParameters),
                ProposalType.Upgrade => ValidateUpgradeExecution(proposal.ExecutionParameters),
                _ => true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating execution parameters for proposal {ProposalId}", proposalId);
            return false;
        }
    }

    public async Task RetryFailedExecutionAsync(Guid executionId)
    {
        _logger.LogInformation("Retrying failed execution {ExecutionId}", executionId);

        try
        {
            var execution = await _context.ProposalExecutions.FindAsync(executionId);
            if (execution == null)
            {
                throw new ArgumentException($"Execution not found: {executionId}");
            }

            if (execution.Status != ExecutionStatus.Failed)
            {
                throw new InvalidOperationException($"Cannot retry execution in status: {execution.Status}");
            }

            if (execution.RetryCount >= execution.MaxRetries)
            {
                throw new InvalidOperationException($"Maximum retry attempts ({execution.MaxRetries}) exceeded");
            }

            // Reset execution for retry
            execution.Status = ExecutionStatus.Pending;
            execution.ErrorMessage = null;
            execution.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Execution {ExecutionId} scheduled for retry", executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry execution {ExecutionId}", executionId);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<string> PerformProposalExecutionAsync(Proposal proposal, ProposalExecution execution)
    {
        _logger.LogInformation("Performing execution for proposal type {ProposalType}", proposal.Type);

        // Simulate execution based on proposal type
        return proposal.Type switch
        {
            ProposalType.Treasury => await ExecuteTreasuryProposalAsync(proposal, execution),
            ProposalType.Parameter => await ExecuteParameterProposalAsync(proposal, execution),
            ProposalType.Upgrade => await ExecuteUpgradeProposalAsync(proposal, execution),
            ProposalType.Constitutional => await ExecuteConstitutionalProposalAsync(proposal, execution),
            ProposalType.Emergency => await ExecuteEmergencyProposalAsync(proposal, execution),
            ProposalType.General => await ExecuteGeneralProposalAsync(proposal, execution),
            _ => throw new NotSupportedException($"Proposal type {proposal.Type} is not supported for execution")
        };
    }

    private async Task<string> ExecuteTreasuryProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate treasury operation
        await Task.Delay(100); // Simulate processing time
        
        var result = $"Treasury proposal executed: {proposal.RequestedAmount} {proposal.RequestedCurrency} transferred";
        _logger.LogInformation("Treasury proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private async Task<string> ExecuteParameterProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate parameter change
        await Task.Delay(50);
        
        var result = "System parameters updated successfully";
        _logger.LogInformation("Parameter proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private async Task<string> ExecuteUpgradeProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate system upgrade
        await Task.Delay(200);
        
        var result = "System upgrade deployed successfully";
        _logger.LogInformation("Upgrade proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private async Task<string> ExecuteConstitutionalProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate constitutional change
        await Task.Delay(100);
        
        var result = "Constitutional changes applied successfully";
        _logger.LogInformation("Constitutional proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private async Task<string> ExecuteEmergencyProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate emergency action
        await Task.Delay(50);
        
        var result = "Emergency measures implemented successfully";
        _logger.LogInformation("Emergency proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private async Task<string> ExecuteGeneralProposalAsync(Proposal proposal, ProposalExecution execution)
    {
        // Simulate general proposal execution
        await Task.Delay(75);
        
        var result = "General proposal executed successfully";
        _logger.LogInformation("General proposal {ProposalId} executed: {Result}", proposal.Id, result);
        
        return result;
    }

    private bool ValidateTreasuryExecution(string parameters)
    {
        // Basic validation for treasury parameters
        return parameters.Contains("recipient") && parameters.Contains("amount");
    }

    private bool ValidateParameterExecution(string parameters)
    {
        // Basic validation for parameter changes
        return parameters.Contains("parameter") && parameters.Contains("value");
    }

    private bool ValidateUpgradeExecution(string parameters)
    {
        // Basic validation for upgrade parameters
        return parameters.Contains("version") && parameters.Contains("deployment");
    }

    #endregion
}
