using Microsoft.AspNetCore.Mvc;
using OrchestrationService.Models;
using OrchestrationService.Services;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using OrchestrationService.Clients;

namespace OrchestrationService.Endpoints;

/// <summary>
/// Minimal API endpoints for OrchestrationService
/// Follows the same pattern as SignatureService and PaymentGatewayService
/// </summary>
public static class OrchestrationEndpoints
{
    /// <summary>
    /// Map all orchestration endpoints
    /// </summary>
    public static void MapOrchestrationEndpoints(this WebApplication app)
    {
        var orchestrationGroup = app.MapGroup("/api/orchestration")
            .WithTags("Orchestration")
            .WithOpenApi();

        // Workflow execution endpoints
        orchestrationGroup.MapPost("/workflows/{workflowId}/execute", ExecuteWorkflowAsync)
            .WithName("ExecuteWorkflow")
            .WithSummary("Execute a workflow with the specified inputs")
            .Produces<WorkflowExecutionResult>(200)
            .Produces<OrchestrationErrorResponse>(400)
            .Produces<OrchestrationErrorResponse>(404)
            .Produces<OrchestrationErrorResponse>(500);

        // Workflow status endpoints
        orchestrationGroup.MapGet("/executions/{executionId}/status", GetWorkflowStatusAsync)
            .WithName("GetWorkflowStatus")
            .WithSummary("Get real-time workflow status")
            .Produces<WorkflowStatusResponse>(200)
            .Produces<OrchestrationErrorResponse>(404);

        orchestrationGroup.MapGet("/executions/{executionId}/progress", GetWorkflowProgressAsync)
            .WithName("GetWorkflowProgress")
            .WithSummary("Get workflow progress percentage")
            .Produces<int>(200)
            .Produces<OrchestrationErrorResponse>(404);

        // Workflow management endpoints
        orchestrationGroup.MapGet("/workflows", GetWorkflowDefinitionsAsync)
            .WithName("GetWorkflowDefinitions")
            .WithSummary("Get available workflow definitions")
            .Produces<List<WorkflowDefinition>>(200);

        orchestrationGroup.MapPost("/workflows/{workflowId}/validate", ValidateWorkflowAsync)
            .WithName("ValidateWorkflow")
            .WithSummary("Validate workflow inputs before execution")
            .Produces<WorkflowValidationResult>(200)
            .Produces<OrchestrationErrorResponse>(400);

        // Event trigger endpoints
        orchestrationGroup.MapPost("/triggers/event", TriggerWorkflowFromEventAsync)
            .WithName("TriggerWorkflowFromEvent")
            .WithSummary("Trigger workflow execution from external event")
            .Produces<EventTriggerResponse>(200)
            .Produces<OrchestrationErrorResponse>(400);

        // User onboarding endpoints (task_1754874509300_crfexbakt)
        // POST /api/orchestration/onboarding/run  { "userId": "..." }
        orchestrationGroup.MapPost("/onboarding/run", RunOnboardingAsync)
            .WithName("RunOnboarding")
            .WithSummary("Run user onboarding workflow for the specified userId")
            .Produces<WorkflowExecutionResult>(200)
            .Produces<OrchestrationErrorResponse>(400);

        // GET /api/orchestration/onboarding/status/{id}  (id = userId or executionId)
        orchestrationGroup.MapGet("/onboarding/status/{id}", GetOnboardingStatusAsync)
            .WithName("GetOnboardingStatus")
            .WithSummary("Get onboarding status by userId or executionId")
            .Produces<WorkflowStatusResponse>(200)
            .Produces<OrchestrationErrorResponse>(404);

        // Monitoring endpoints
        orchestrationGroup.MapGet("/health", GetHealthAsync)
            .WithName("GetOrchestrationHealth")
            .WithSummary("Get service health status")
            .Produces<object>(200);

        orchestrationGroup.MapGet("/metrics", GetMetricsAsync)
            .WithName("GetOrchestrationMetrics")
            .WithSummary("Get service performance metrics")
            .Produces<object>(200);
    }

    /// <summary>
    /// Execute a workflow with the specified inputs
    /// </summary>
    private static async Task<IResult> ExecuteWorkflowAsync(
        string workflowId,
        [FromBody] WorkflowExecutionRequest request,
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // Set the workflow ID from the route parameter
            request.WorkflowId = workflowId;

            logger.LogInformation("Executing workflow: {WorkflowId}, TriggeredBy: {TriggeredBy}",
                workflowId, request.TriggeredBy);

            var result = await workflowManager.ExecuteWorkflowAsync(request, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid workflow execution request: {WorkflowId}", workflowId);
            return Results.BadRequest(new OrchestrationErrorResponse
            {
                Error = "InvalidRequest",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute workflow: {WorkflowId}", workflowId);
            return Results.Problem(
                detail: ex.Message,
                title: "Workflow execution failed",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Get real-time workflow status
    /// </summary>
    private static async Task<IResult> GetWorkflowStatusAsync(
        string executionId,
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger)
    {
        try
        {
            logger.LogDebug("Getting workflow status: {ExecutionId}", executionId);

            var status = await workflowManager.GetWorkflowStatusAsync(executionId);
            return Results.Ok(status);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Workflow execution not found: {ExecutionId}", executionId);
            return Results.NotFound(new OrchestrationErrorResponse
            {
                Error = "ExecutionNotFound",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get workflow status: {ExecutionId}", executionId);
            return Results.Problem(
                detail: ex.Message,
                title: "Failed to get workflow status",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Get workflow progress percentage
    /// </summary>
    private static async Task<IResult> GetWorkflowProgressAsync(
        string executionId,
        WorkflowStatusService statusService,
        ILogger<WorkflowStatusService> logger)
    {
        try
        {
            logger.LogDebug("Getting workflow progress: {ExecutionId}", executionId);

            var progress = await statusService.GetWorkflowProgressAsync(executionId);
            return Results.Ok(progress);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Workflow execution not found: {ExecutionId}", executionId);
            return Results.NotFound(new OrchestrationErrorResponse
            {
                Error = "ExecutionNotFound",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get workflow progress: {ExecutionId}", executionId);
            return Results.Problem(
                detail: ex.Message,
                title: "Failed to get workflow progress",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Get available workflow definitions
    /// </summary>
    private static async Task<IResult> GetWorkflowDefinitionsAsync(
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger)
    {
        try
        {
            logger.LogDebug("Getting workflow definitions");

            var definitions = await workflowManager.GetWorkflowDefinitionsAsync();
            return Results.Ok(definitions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get workflow definitions");
            return Results.Problem(
                detail: ex.Message,
                title: "Failed to get workflow definitions",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Validate workflow inputs before execution
    /// </summary>
    private static async Task<IResult> ValidateWorkflowAsync(
        string workflowId,
        [FromBody] WorkflowValidationRequest request,
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger)
    {
        try
        {
            // Set the workflow ID from the route parameter
            request.WorkflowId = workflowId;

            logger.LogDebug("Validating workflow: {WorkflowId}", workflowId);

            var result = await workflowManager.ValidateWorkflowAsync(request);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate workflow: {WorkflowId}", workflowId);
            return Results.BadRequest(new OrchestrationErrorResponse
            {
                Error = "ValidationFailed",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
    }

    /// <summary>
    /// Trigger workflow execution from external event
    /// </summary>
    private static async Task<IResult> TriggerWorkflowFromEventAsync(
        [FromBody] EventTriggerRequest request,
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Processing event trigger: {EventType}, Source: {Source}",
                request.EventType, request.Source);

            // Map event to workflow execution request
            var workflowRequest = MapEventToWorkflowRequest(request);
            
            if (workflowRequest == null)
            {
                return Results.Ok(new EventTriggerResponse
                {
                    TriggerResult = "IGNORED",
                    Message = $"No workflow mapped for event type: {request.EventType}"
                });
            }

            var result = await workflowManager.ExecuteWorkflowAsync(workflowRequest, cancellationToken);

            return Results.Ok(new EventTriggerResponse
            {
                TriggerResult = "TRIGGERED",
                TriggeredWorkflows = new List<string> { result.ExecutionId },
                Message = "Workflow triggered successfully"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process event trigger: {EventType}", request.EventType);
            return Results.BadRequest(new OrchestrationErrorResponse
            {
                Error = "EventTriggerFailed",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
    }

    /// <summary>
    /// Run user onboarding for a given userId
    /// Implements task_1754874509300_crfexbakt: POST /api/orchestration/onboarding/run
    /// </summary>
    private static async Task<IResult> RunOnboardingAsync(
        [FromBody] OnboardingRunRequest? request,
        WorkflowManager workflowManager,
        ILogger<WorkflowManager> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
            {
                return Results.BadRequest(new OrchestrationErrorResponse
                {
                    Error = "InvalidRequest",
                    Message = "userId is required in body",
                    TraceId = Activity.Current?.Id
                });
            }

            logger.LogInformation("Starting onboarding for userId: {UserId}", request.UserId);

            // Build workflow execution request for the onboarding workflow
            var inputs = new Dictionary<string, object>
            {
                ["userRegistration"] = new Dictionary<string, object>
                {
                    ["userId"] = request.UserId
                }
            };

            var execRequest = new WorkflowExecutionRequest
            {
                WorkflowId = "user-onboarding-optimized",
                Inputs = inputs,
                TriggeredBy = "OrchestrationService",
                Description = $"Onboarding run for user {request.UserId}"
            };

            var result = await workflowManager.ExecuteWorkflowAsync(execRequest, cancellationToken);

            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid onboarding request");
            return Results.BadRequest(new OrchestrationErrorResponse
            {
                Error = "InvalidRequest",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start onboarding for user");
            return Results.Problem(detail: ex.Message, title: "Onboarding failed", statusCode: 500);
        }
    }

    /// <summary>
    /// Get onboarding status by userId or executionId
    /// Implements task_1754874509300_crfexbakt: GET /api/orchestration/onboarding/status/{id}
    /// </summary>
    private static async Task<IResult> GetOnboardingStatusAsync(
        string id,
        WorkflowStatusService statusService,
        IMemoryCache cache,
        ILogger logger)
    {
        try
        {
            logger.LogDebug("Getting onboarding status for id: {Id}", id);

            // If caller provided a userId, we map it to executionId stored in cache by the workflow
            if (cache.TryGetValue($"onboarding_user_{id}", out string? mappedExecutionId) && !string.IsNullOrEmpty(mappedExecutionId))
            {
                var status = await statusService.GetWorkflowStatusAsync(mappedExecutionId);
                return Results.Ok(status);
            }

            // Otherwise, treat id as an executionId
            var statusByExecution = await statusService.GetWorkflowStatusAsync(id);
            return Results.Ok(statusByExecution);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Onboarding status not found: {Id}", id);
            return Results.NotFound(new OrchestrationErrorResponse
            {
                Error = "ExecutionNotFound",
                Message = ex.Message,
                TraceId = Activity.Current?.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get onboarding status: {Id}", id);
            return Results.Problem(detail: ex.Message, title: "Failed to get onboarding status", statusCode: 500);
        }
    }

    /// <summary>
    /// Simple request model for onboarding run
    /// </summary>
    public class OnboardingRunRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Get service health status
    /// </summary>
    private static async Task<IResult> GetHealthAsync(
        ILogger logger)
    {
        try
        {
            var health = new
            {
                Service = "OrchestrationService",
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Uptime = Environment.TickCount64,
                Features = new[]
                {
                    "Workflow Execution",
                    "Status Tracking",
                    "Event Publishing",
                    "Zero-Trust Integration"
                }
            };

            return Results.Ok(health);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return Results.Problem(
                detail: ex.Message,
                title: "Health check failed",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Get service performance metrics
    /// </summary>
    private static async Task<IResult> GetMetricsAsync(
        ILogger logger)
    {
        try
        {
            var metrics = new
            {
                Service = "OrchestrationService",
                Version = "1.0.0",
                Timestamp = DateTime.UtcNow,
                PerformanceTargets = new
                {
                    PaymentWorkflow = "≤5 seconds",
                    UserOnboarding = "≤10 seconds",
                    TreasuryOperations = "≤15 seconds",
                    StatusQueries = "≤100ms"
                },
                WorkflowTypes = new[]
                {
                    "payment-processing-zero-trust",
                    "user-onboarding-optimized",
                    "treasury-operations-secure"
                },
                IntegratedServices = new[]
                {
                    "SignatureService",
                    "PaymentGatewayService",
                    "QuantumLedger.Hub",
                    "UserService",
                    "AccountService",
                    "TreasuryService",
                    "NotificationService",
                    "IdentityVerificationService"
                }
            };

            return Results.Ok(metrics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get metrics");
            return Results.Problem(
                detail: ex.Message,
                title: "Failed to get metrics",
                statusCode: 500);
        }
    }

    /// <summary>
    /// Map external event to workflow execution request
    /// </summary>
    private static WorkflowExecutionRequest? MapEventToWorkflowRequest(EventTriggerRequest eventRequest)
    {
        return eventRequest.EventType switch
        {
            "payment_requested" => new WorkflowExecutionRequest
            {
                WorkflowId = "payment-processing-zero-trust",
                Inputs = eventRequest.EventData,
                TriggeredBy = eventRequest.Source,
                Context = eventRequest.Headers,
                Description = "Payment workflow triggered by external event"
            },
            "user_registration" => new WorkflowExecutionRequest
            {
                WorkflowId = "user-onboarding-optimized",
                Inputs = eventRequest.EventData,
                TriggeredBy = eventRequest.Source,
                Context = eventRequest.Headers,
                Description = "User onboarding workflow triggered by registration event"
            },
            "treasury_operation_requested" => new WorkflowExecutionRequest
            {
                WorkflowId = "treasury-operations-secure",
                Inputs = eventRequest.EventData,
                TriggeredBy = eventRequest.Source,
                Context = eventRequest.Headers,
                Description = "Treasury workflow triggered by operation request"
            },
            _ => null // No workflow mapped for this event type
        };
    }
}
