using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

namespace OrchestrationService.Services;

/// <summary>
/// Service for publishing workflow events to SNS/SQS
/// Publishes highlights-only events without exposing internal workflow details
/// </summary>
public class WorkflowEventPublisher
{
    private readonly ILogger<WorkflowEventPublisher> _logger;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly IConfiguration _configuration;

    public WorkflowEventPublisher(
        ILogger<WorkflowEventPublisher> logger,
        IAmazonSimpleNotificationService snsClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _snsClient = snsClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Publish workflow event to SNS topic
    /// Only publishes safe, non-sensitive event data
    /// </summary>
    public async Task PublishWorkflowEventAsync(
        string workflowType,
        string executionId,
        string eventType,
        object eventData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var topicArn = GetTopicArn(eventType);
            if (string.IsNullOrEmpty(topicArn))
            {
                _logger.LogWarning("No SNS topic configured for event type: {EventType}", eventType);
                return;
            }

            var message = CreateEventMessage(workflowType, executionId, eventType, eventData);
            var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var publishRequest = new PublishRequest
            {
                TopicArn = topicArn,
                Message = messageJson,
                Subject = $"Workflow Event: {eventType}",
                MessageAttributes = CreateMessageAttributes(workflowType, eventType)
            };

            var response = await _snsClient.PublishAsync(publishRequest, cancellationToken);

            _logger.LogInformation("Published workflow event: {EventType}, ExecutionId: {ExecutionId}, MessageId: {MessageId}",
                eventType, executionId, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish workflow event: {EventType}, ExecutionId: {ExecutionId}",
                eventType, executionId);
            
            // Don't throw - event publishing failures shouldn't break workflow execution
        }
    }

    /// <summary>
    /// Publish workflow status update event
    /// </summary>
    public async Task PublishWorkflowStatusUpdateAsync(
        string workflowType,
        string executionId,
        string status,
        int progress,
        string? currentStep = null,
        CancellationToken cancellationToken = default)
    {
        var eventData = new
        {
            Status = status,
            Progress = progress,
            CurrentStep = currentStep,
            Timestamp = DateTime.UtcNow
        };

        await PublishWorkflowEventAsync(workflowType, executionId, "workflow_status_update", eventData, cancellationToken);
    }

    /// <summary>
    /// Publish workflow completion event
    /// </summary>
    public async Task PublishWorkflowCompletionAsync(
        string workflowType,
        string executionId,
        string status,
        TimeSpan duration,
        Dictionary<string, object>? safeResults = null,
        CancellationToken cancellationToken = default)
    {
        var eventData = new
        {
            Status = status,
            Duration = duration.TotalSeconds,
            Results = safeResults ?? new Dictionary<string, object>(),
            CompletedAt = DateTime.UtcNow
        };

        await PublishWorkflowEventAsync(workflowType, executionId, "workflow_completed", eventData, cancellationToken);
    }

    /// <summary>
    /// Publish workflow error event
    /// </summary>
    public async Task PublishWorkflowErrorAsync(
        string workflowType,
        string executionId,
        string errorType,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var eventData = new
        {
            ErrorType = errorType,
            ErrorMessage = SanitizeErrorMessage(errorMessage),
            Timestamp = DateTime.UtcNow
        };

        await PublishWorkflowEventAsync(workflowType, executionId, "workflow_error", eventData, cancellationToken);
    }

    /// <summary>
    /// Publish admin alert for critical workflow issues
    /// </summary>
    public async Task PublishAdminAlertAsync(
        string alertType,
        string message,
        string? workflowType = null,
        string? executionId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var alertTopicArn = _configuration["AWS:SNS:AdminAlertsTopicArn"];
            if (string.IsNullOrEmpty(alertTopicArn))
            {
                _logger.LogWarning("No admin alerts SNS topic configured");
                return;
            }

            var alertMessage = new
            {
                AlertType = alertType,
                Message = message,
                WorkflowType = workflowType,
                ExecutionId = executionId,
                Timestamp = DateTime.UtcNow,
                Service = "OrchestrationService"
            };

            var messageJson = JsonSerializer.Serialize(alertMessage, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var publishRequest = new PublishRequest
            {
                TopicArn = alertTopicArn,
                Message = messageJson,
                Subject = $"OrchestrationService Alert: {alertType}",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["AlertType"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = alertType
                    },
                    ["Service"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = "OrchestrationService"
                    }
                }
            };

            var response = await _snsClient.PublishAsync(publishRequest, cancellationToken);

            _logger.LogInformation("Published admin alert: {AlertType}, MessageId: {MessageId}",
                alertType, response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish admin alert: {AlertType}", alertType);
        }
    }

    /// <summary>
    /// Create event message with safe, non-sensitive data
    /// </summary>
    private object CreateEventMessage(string workflowType, string executionId, string eventType, object eventData)
    {
        return new
        {
            Type = "workflow_event",
            WorkflowType = workflowType,
            ExecutionId = executionId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Data = SanitizeEventData(eventData),
            Source = "OrchestrationService",
            Version = "1.0"
        };
    }

    /// <summary>
    /// Sanitize event data to remove sensitive information
    /// </summary>
    private object SanitizeEventData(object eventData)
    {
        // Convert to JSON and back to remove any sensitive properties
        var json = JsonSerializer.Serialize(eventData);
        var sanitized = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

        if (sanitized == null)
            return new { };

        // Remove sensitive keys
        var sensitiveKeys = new[]
        {
            "signature", "validationId", "privateKey", "secret", "token",
            "password", "apiKey", "internalId", "systemId"
        };

        foreach (var key in sensitiveKeys)
        {
            var keysToRemove = sanitized.Keys.Where(k => 
                string.Equals(k, key, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var keyToRemove in keysToRemove)
            {
                sanitized.Remove(keyToRemove);
            }
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitize error messages to remove sensitive information
    /// </summary>
    private string SanitizeErrorMessage(string errorMessage)
    {
        // Remove potential sensitive information from error messages
        var sanitized = errorMessage;

        // Remove common sensitive patterns
        var sensitivePatterns = new[]
        {
            @"signature:\s*[A-Za-z0-9+/=]+",
            @"token:\s*[A-Za-z0-9\-_]+",
            @"key:\s*[A-Za-z0-9+/=]+",
            @"password:\s*\S+",
            @"secret:\s*\S+"
        };

        foreach (var pattern in sensitivePatterns)
        {
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized, pattern, "[REDACTED]", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    /// <summary>
    /// Get SNS topic ARN for event type
    /// </summary>
    private string GetTopicArn(string eventType)
    {
        return eventType switch
        {
            "workflow_started" => _configuration["AWS:SNS:WorkflowEventsTopicArn"] ?? "",
            "workflow_completed" => _configuration["AWS:SNS:WorkflowEventsTopicArn"] ?? "",
            "workflow_failed" => _configuration["AWS:SNS:WorkflowEventsTopicArn"] ?? "",
            "workflow_status_update" => _configuration["AWS:SNS:WorkflowStatusTopicArn"] ?? "",
            "workflow_error" => _configuration["AWS:SNS:WorkflowErrorsTopicArn"] ?? "",
            _ => _configuration["AWS:SNS:WorkflowEventsTopicArn"] ?? ""
        };
    }

    /// <summary>
    /// Create message attributes for SNS message
    /// </summary>
    private Dictionary<string, MessageAttributeValue> CreateMessageAttributes(string workflowType, string eventType)
    {
        return new Dictionary<string, MessageAttributeValue>
        {
            ["WorkflowType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = workflowType
            },
            ["EventType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = eventType
            },
            ["Source"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = "OrchestrationService"
            },
            ["Timestamp"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = DateTime.UtcNow.ToString("O")
            }
        };
    }
}
