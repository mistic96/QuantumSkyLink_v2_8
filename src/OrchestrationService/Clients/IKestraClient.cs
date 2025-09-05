using Refit;
using OrchestrationService.Models;

namespace OrchestrationService.Clients;

/// <summary>
/// Kestra API client for workflow execution and management
/// Integrates with Kestra workflow engine via REST API
/// </summary>
public interface IKestraClient
{
    /// <summary>
    /// Execute a workflow with the specified inputs
    /// </summary>
    [Post("/api/v1/executions")]
    Task<KestraExecutionResponse> ExecuteWorkflowAsync(
        [Body] KestraExecutionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow execution status
    /// </summary>
    [Get("/api/v1/executions/{executionId}")]
    Task<KestraExecutionStatus> GetExecutionStatusAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all available workflow definitions
    /// </summary>
    [Get("/api/v1/flows")]
    Task<List<KestraFlowDefinition>> GetFlowDefinitionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific workflow definition
    /// </summary>
    [Get("/api/v1/flows/{namespace}/{flowId}")]
    Task<KestraFlowDefinition> GetFlowDefinitionAsync(
        string @namespace,
        string flowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow execution logs
    /// </summary>
    [Get("/api/v1/executions/{executionId}/logs")]
    Task<List<KestraLogEntry>> GetExecutionLogsAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a running workflow execution
    /// </summary>
    [Delete("/api/v1/executions/{executionId}")]
    Task<KestraExecutionResponse> CancelExecutionAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow execution metrics
    /// </summary>
    [Get("/api/v1/executions/{executionId}/metrics")]
    Task<KestraExecutionMetrics> GetExecutionMetricsAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate workflow inputs before execution
    /// </summary>
    [Post("/api/v1/flows/{namespace}/{flowId}/validate")]
    Task<KestraValidationResponse> ValidateWorkflowInputsAsync(
        string @namespace,
        string flowId,
        [Body] Dictionary<string, object> inputs,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Kestra workflow execution request
/// </summary>
public class KestraExecutionRequest
{
    public string Namespace { get; set; } = "quantumskylink";
    public string FlowId { get; set; } = string.Empty;
    public Dictionary<string, object> Inputs { get; set; } = new();
    public Dictionary<string, string> Labels { get; set; } = new();
}

/// <summary>
/// Kestra workflow execution response
/// </summary>
public class KestraExecutionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string FlowId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Dictionary<string, object> Inputs { get; set; } = new();
    public Dictionary<string, object> Outputs { get; set; } = new();
}

/// <summary>
/// Kestra workflow execution status
/// </summary>
public class KestraExecutionStatus
{
    public string Id { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public List<KestraTaskRun> TaskRuns { get; set; } = new();
    public Dictionary<string, object> Outputs { get; set; } = new();
}

/// <summary>
/// Kestra task run information
/// </summary>
public class KestraTaskRun
{
    public string Id { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, object> Outputs { get; set; } = new();
}

/// <summary>
/// Kestra flow definition
/// </summary>
public class KestraFlowDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Labels { get; set; } = new();
    public List<KestraInput> Inputs { get; set; } = new();
    public List<KestraTask> Tasks { get; set; } = new();
}

/// <summary>
/// Kestra workflow input definition
/// </summary>
public class KestraInput
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; }
    public object? Defaults { get; set; }
}

/// <summary>
/// Kestra task definition
/// </summary>
public class KestraTask
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Kestra log entry
/// </summary>
public class KestraLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string TaskRunId { get; set; } = string.Empty;
}

/// <summary>
/// Kestra execution metrics
/// </summary>
public class KestraExecutionMetrics
{
    public string ExecutionId { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int TaskCount { get; set; }
    public int SuccessfulTasks { get; set; }
    public int FailedTasks { get; set; }
    public Dictionary<string, TimeSpan> TaskDurations { get; set; } = new();
}

/// <summary>
/// Kestra validation response
/// </summary>
public class KestraValidationResponse
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
