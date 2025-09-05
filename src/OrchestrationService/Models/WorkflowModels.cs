using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrchestrationService.Models;

/// <summary>
/// Request to execute a workflow with inputs and context
/// </summary>
public class WorkflowExecutionRequest
{
    [Required]
    [StringLength(100)]
    public string WorkflowId { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, object> Inputs { get; set; } = new();

    [Required]
    [StringLength(255)]
    public string TriggeredBy { get; set; } = string.Empty;

    public Dictionary<string, string> Context { get; set; } = new();

    [StringLength(500)]
    public string? Description { get; set; }

    public int Priority { get; set; } = 5; // 1-10, 10 = highest priority
}

/// <summary>
/// Result of workflow execution initiation
/// </summary>
public class WorkflowExecutionResult
{
    [Required]
    public string ExecutionId { get; set; } = string.Empty;

    [Required]
    public string WorkflowId { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty; // QUEUED, RUNNING, SUCCESS, FAILED

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EstimatedCompletion { get; set; }

    public string? Message { get; set; }

    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Real-time workflow status without exposing internal details
/// </summary>
public class WorkflowStatusResponse
{
    [Required]
    public string ExecutionId { get; set; } = string.Empty;

    [Required]
    public string WorkflowId { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty; // RUNNING, SUCCESS, FAILED, CANCELLED

    [StringLength(200)]
    public string CurrentStep { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Progress { get; set; } = 0; // 0-100 percentage

    public DateTime? EstimatedCompletion { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public TimeSpan? Duration { get; set; }

    public List<WorkflowHighlight> Highlights { get; set; } = new();

    public Dictionary<string, object> Results { get; set; } = new();

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// High-level workflow step highlights without internal exposure
/// </summary>
public class WorkflowHighlight
{
    [Required]
    [StringLength(100)]
    public string Step { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty; // PENDING, RUNNING, SUCCESS, FAILED

    public DateTime Timestamp { get; set; }

    public TimeSpan? Duration { get; set; }

    [StringLength(500)]
    public string? Message { get; set; }

    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event trigger request for workflow initiation
/// </summary>
public class EventTriggerRequest
{
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, object> EventData { get; set; } = new();

    [Required]
    [StringLength(255)]
    public string Source { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Response to event trigger
/// </summary>
public class EventTriggerResponse
{
    [Required]
    public string TriggerResult { get; set; } = string.Empty; // TRIGGERED, IGNORED, ERROR

    public List<string> TriggeredWorkflows { get; set; } = new();

    public string? Message { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Workflow definition metadata
/// </summary>
public class WorkflowDefinition
{
    [Required]
    [StringLength(100)]
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Namespace { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = "1.0.0";

    public List<string> Tags { get; set; } = new();

    public Dictionary<string, WorkflowInput> Inputs { get; set; } = new();

    public TimeSpan? EstimatedDuration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Workflow input definition
/// </summary>
public class WorkflowInput
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // STRING, NUMBER, BOOLEAN, OBJECT, ARRAY

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool Required { get; set; } = false;

    public object? DefaultValue { get; set; }

    public Dictionary<string, object> Validation { get; set; } = new();
}

/// <summary>
/// Workflow validation request
/// </summary>
public class WorkflowValidationRequest
{
    [Required]
    [StringLength(100)]
    public string WorkflowId { get; set; } = string.Empty;

    public Dictionary<string, object> Inputs { get; set; } = new();

    public bool ValidateInputsOnly { get; set; } = false;
}

/// <summary>
/// Workflow validation result
/// </summary>
public class WorkflowValidationResult
{
    public bool IsValid { get; set; }

    public List<string> Errors { get; set; } = new();

    public List<string> Warnings { get; set; } = new();

    public Dictionary<string, object> ValidatedInputs { get; set; } = new();

    public TimeSpan? EstimatedDuration { get; set; }
}

/// <summary>
/// Workflow metrics for monitoring
/// </summary>
public class WorkflowMetrics
{
    [Required]
    public string WorkflowId { get; set; } = string.Empty;

    public int TotalExecutions { get; set; }

    public int SuccessfulExecutions { get; set; }

    public int FailedExecutions { get; set; }

    public double SuccessRate { get; set; }

    public TimeSpan AverageDuration { get; set; }

    public TimeSpan MedianDuration { get; set; }

    public TimeSpan P95Duration { get; set; }

    public DateTime LastExecution { get; set; }

    public DateTime MetricsCalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Error response for API endpoints
/// </summary>
public class OrchestrationErrorResponse
{
    [Required]
    public string Error { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string? TraceId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> Metadata { get; set; } = new();
}
