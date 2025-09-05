using OrchestrationService.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace OrchestrationService.Services;

/// <summary>
/// Service for tracking workflow status without exposing internal details
/// Provides real-time status updates with highlights-only approach
/// </summary>
public class WorkflowStatusService
{
    private readonly ILogger<WorkflowStatusService> _logger;
    private readonly IMemoryCache _cache;

    public WorkflowStatusService(
        ILogger<WorkflowStatusService> logger,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get workflow status by execution ID
    /// Returns high-level status without exposing internal workflow details
    /// </summary>
    public async Task<WorkflowStatusResponse> GetWorkflowStatusAsync(string executionId)
    {
        _logger.LogDebug("Getting workflow status for ExecutionId: {ExecutionId}", executionId);

        // Retrieve execution context from cache
        if (!_cache.TryGetValue($"workflow_execution_{executionId}", out WorkflowExecutionContext? context) || context == null)
        {
            throw new ArgumentException($"Workflow execution not found: {executionId}");
        }

        var response = new WorkflowStatusResponse
        {
            ExecutionId = context.ExecutionId,
            WorkflowId = context.WorkflowId,
            Status = context.Status,
            CurrentStep = GetFriendlyCurrentStep(context),
            Progress = CalculateProgress(context),
            EstimatedCompletion = CalculateEstimatedCompletion(context),
            StartedAt = context.StartedAt,
            CompletedAt = context.CompletedAt,
            Duration = context.CompletedAt?.Subtract(context.StartedAt),
            Highlights = ExtractHighlights(context),
            Results = GetSafeResults(context),
            ErrorMessage = GetErrorMessage(context)
        };

        return response;
    }

    /// <summary>
    /// Get workflow progress as percentage (0-100)
    /// </summary>
    public async Task<int> GetWorkflowProgressAsync(string executionId)
    {
        var status = await GetWorkflowStatusAsync(executionId);
        return status.Progress;
    }

    /// <summary>
    /// Check if workflow is still running
    /// </summary>
    public async Task<bool> IsWorkflowRunningAsync(string executionId)
    {
        try
        {
            var status = await GetWorkflowStatusAsync(executionId);
            return status.Status == "RUNNING";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get friendly current step name without exposing internal details
    /// </summary>
    private string GetFriendlyCurrentStep(WorkflowExecutionContext context)
    {
        return context.Status switch
        {
            "RUNNING" => GetCurrentStepFromWorkflowType(context.WorkflowId),
            "SUCCESS" => "Completed",
            "FAILED" => "Failed",
            "CANCELLED" => "Cancelled",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get current step based on workflow type and progress
    /// </summary>
    private string GetCurrentStepFromWorkflowType(string workflowId)
    {
        return workflowId switch
        {
            "payment-processing-zero-trust" => "Processing Payment",
            "user-onboarding-optimized" => "Setting Up Account",
            "treasury-operations-secure" => "Executing Treasury Operation",
            _ => "Processing"
        };
    }

    /// <summary>
    /// Calculate workflow progress percentage
    /// </summary>
    private int CalculateProgress(WorkflowExecutionContext context)
    {
        return context.Status switch
        {
            "RUNNING" => CalculateRunningProgress(context),
            "SUCCESS" => 100,
            "FAILED" => 0,
            "CANCELLED" => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Calculate progress for running workflows
    /// </summary>
    private int CalculateRunningProgress(WorkflowExecutionContext context)
    {
        var elapsed = DateTime.UtcNow - context.StartedAt;
        var estimatedDuration = GetEstimatedDuration(context.WorkflowId);
        
        if (estimatedDuration == TimeSpan.Zero)
            return 50; // Default to 50% if no estimate

        var progressRatio = elapsed.TotalSeconds / estimatedDuration.TotalSeconds;
        return Math.Min(95, Math.Max(10, (int)(progressRatio * 100))); // Cap at 95% until completion
    }

    /// <summary>
    /// Get estimated duration for workflow type
    /// </summary>
    private TimeSpan GetEstimatedDuration(string workflowId)
    {
        return workflowId switch
        {
            "payment-processing-zero-trust" => TimeSpan.FromSeconds(5),
            "user-onboarding-optimized" => TimeSpan.FromSeconds(10),
            "treasury-operations-secure" => TimeSpan.FromSeconds(15),
            _ => TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Calculate estimated completion time
    /// </summary>
    private DateTime? CalculateEstimatedCompletion(WorkflowExecutionContext context)
    {
        if (context.Status != "RUNNING")
            return context.CompletedAt;

        var estimatedDuration = GetEstimatedDuration(context.WorkflowId);
        return context.StartedAt.Add(estimatedDuration);
    }

    /// <summary>
    /// Extract workflow highlights without exposing internal details
    /// Only shows high-level steps that are safe for external consumption
    /// </summary>
    private List<WorkflowHighlight> ExtractHighlights(WorkflowExecutionContext context)
    {
        var highlights = new List<WorkflowHighlight>();

        // Add standard workflow highlights based on type
        switch (context.WorkflowId)
        {
            case "payment-processing-zero-trust":
                highlights.AddRange(GetPaymentWorkflowHighlights(context));
                break;
            case "user-onboarding-optimized":
                highlights.AddRange(GetOnboardingWorkflowHighlights(context));
                break;
            case "treasury-operations-secure":
                highlights.AddRange(GetTreasuryWorkflowHighlights(context));
                break;
        }

        return highlights;
    }

    /// <summary>
    /// Get payment workflow highlights
    /// </summary>
    private List<WorkflowHighlight> GetPaymentWorkflowHighlights(WorkflowExecutionContext context)
    {
        var highlights = new List<WorkflowHighlight>
        {
            new WorkflowHighlight
            {
                Step = "Security Validation",
                Status = context.Status == "FAILED" ? "FAILED" : "SUCCESS",
                Timestamp = context.StartedAt,
                Duration = TimeSpan.FromMilliseconds(500),
                Message = "Signature validation completed"
            },
            new WorkflowHighlight
            {
                Step = "Payment Processing",
                Status = context.Status == "SUCCESS" ? "SUCCESS" : context.Status == "FAILED" ? "FAILED" : "RUNNING",
                Timestamp = context.StartedAt.AddSeconds(1),
                Duration = context.Status == "SUCCESS" ? TimeSpan.FromSeconds(2) : null,
                Message = context.Status == "SUCCESS" ? "Payment processed successfully" : "Processing payment"
            }
        };

        if (context.Status == "SUCCESS")
        {
            highlights.Add(new WorkflowHighlight
            {
                Step = "Confirmation",
                Status = "SUCCESS",
                Timestamp = context.CompletedAt ?? DateTime.UtcNow,
                Duration = TimeSpan.FromMilliseconds(200),
                Message = "Payment confirmed and recorded"
            });
        }

        return highlights;
    }

    /// <summary>
    /// Get onboarding workflow highlights
    /// </summary>
    private List<WorkflowHighlight> GetOnboardingWorkflowHighlights(WorkflowExecutionContext context)
    {
        return new List<WorkflowHighlight>
        {
            new WorkflowHighlight
            {
                Step = "Account Creation",
                Status = context.Status == "FAILED" ? "FAILED" : "SUCCESS",
                Timestamp = context.StartedAt,
                Message = "User account created"
            },
            new WorkflowHighlight
            {
                Step = "Identity Verification",
                Status = context.Status == "SUCCESS" ? "SUCCESS" : context.Status == "FAILED" ? "FAILED" : "RUNNING",
                Timestamp = context.StartedAt.AddSeconds(2),
                Message = "Identity verification in progress"
            }
        };
    }

    /// <summary>
    /// Get treasury workflow highlights
    /// </summary>
    private List<WorkflowHighlight> GetTreasuryWorkflowHighlights(WorkflowExecutionContext context)
    {
        return new List<WorkflowHighlight>
        {
            new WorkflowHighlight
            {
                Step = "Authorization",
                Status = context.Status == "FAILED" ? "FAILED" : "SUCCESS",
                Timestamp = context.StartedAt,
                Message = "Multi-signature authorization completed"
            },
            new WorkflowHighlight
            {
                Step = "Treasury Operation",
                Status = context.Status == "SUCCESS" ? "SUCCESS" : context.Status == "FAILED" ? "FAILED" : "RUNNING",
                Timestamp = context.StartedAt.AddSeconds(3),
                Message = "Treasury operation execution"
            }
        };
    }

    /// <summary>
    /// Get safe results that can be exposed externally
    /// Filters out sensitive internal data
    /// </summary>
    private Dictionary<string, object> GetSafeResults(WorkflowExecutionContext context)
    {
        var safeResults = new Dictionary<string, object>();

        if (context.Status == "SUCCESS" && context.Results.Any())
        {
            // Only expose safe, non-sensitive result data
            foreach (var result in context.Results)
            {
                switch (result.Key)
                {
                    case "paymentId":
                    case "transactionId":
                    case "operationId":
                    case "userId":
                    case "accountId":
                        safeResults[result.Key] = result.Value;
                        break;
                    // Skip sensitive data like validation IDs, signatures, etc.
                }
            }
        }

        return safeResults;
    }

    /// <summary>
    /// Get error message if workflow failed
    /// </summary>
    private string? GetErrorMessage(WorkflowExecutionContext context)
    {
        if (context.Status == "FAILED" && context.Results.ContainsKey("error"))
        {
            return context.Results["error"]?.ToString();
        }

        return null;
    }
}
