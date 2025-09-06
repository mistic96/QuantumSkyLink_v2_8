using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuantumSkyLink.Shared.Eventing
{
    /// <summary>
    /// Background service that consumes messages from SQS queues
    /// Each microservice should register this with their specific queue
    /// </summary>
    public class SQSConsumerService : BackgroundService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SQSConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _queueUrl;
        private readonly string _serviceName;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, Type> _eventTypeMap;
        private readonly int _maxNumberOfMessages;
        private readonly int _waitTimeSeconds;

        public SQSConsumerService(
            IAmazonSQS sqsClient,
            IServiceProvider serviceProvider,
            ILogger<SQSConsumerService> logger,
            IConfiguration configuration,
            string serviceName)
        {
            _sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            
            // Get queue URL from configuration
            var environment = configuration["AWS:Deployment:Environment"] ?? "development";
            var projectName = configuration["AWS:ProjectName"] ?? "qsl";
            
            // Try to get from CloudFormation outputs first, fallback to constructed URL
            _queueUrl = configuration[$"AWS:Resources:SQS:{_serviceName}QueueUrl"] 
                ?? configuration[$"AWS:SQS:{_serviceName}QueueUrl"]
                ?? $"https://sqs.{configuration["AWS:Region"]}.amazonaws.com/{configuration["AWS:AccountId"]}/{projectName}-{environment}-{_serviceName.ToLower()}-inbox";
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            _eventTypeMap = new Dictionary<string, Type>();
            _maxNumberOfMessages = configuration.GetValue<int>("AWS:SQS:MaxNumberOfMessages", 10);
            _waitTimeSeconds = configuration.GetValue<int>("AWS:SQS:WaitTimeSeconds", 20);
            
            RegisterEventTypes();
        }

        /// <summary>
        /// Register event type mappings for deserialization
        /// </summary>
        public void RegisterEventType<T>(string detailType) where T : class
        {
            _eventTypeMap[detailType] = typeof(T);
            _logger.LogDebug("Registered event type {DetailType} -> {Type}", detailType, typeof(T).Name);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting SQS consumer for {ServiceName} on queue {QueueUrl}", 
                _serviceName, _queueUrl);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollAndProcessMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SQS consumer loop for {ServiceName}", _serviceName);
                    
                    // Wait before retrying to avoid tight loop on persistent errors
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Stopping SQS consumer for {ServiceName}", _serviceName);
        }

        private async Task PollAndProcessMessages(CancellationToken cancellationToken)
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = _maxNumberOfMessages,
                WaitTimeSeconds = _waitTimeSeconds,
                MessageAttributeNames = new List<string> { "All" },
                AttributeNames = new List<string> { "All" }
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);

            if (response.Messages == null || !response.Messages.Any())
            {
                // No messages available
                return;
            }

            _logger.LogDebug("Received {Count} messages from queue", response.Messages.Count);

            // Process messages in parallel
            var processingTasks = response.Messages.Select(message => 
                ProcessMessageAsync(message, cancellationToken));
            
            await Task.WhenAll(processingTasks);
        }

        private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Processing message {MessageId}", message.MessageId);

                // Parse the message body
                var messageBody = JsonSerializer.Deserialize<EventBridgeMessage>(message.Body, _jsonOptions);
                
                if (messageBody == null)
                {
                    _logger.LogWarning("Could not deserialize message body for message {MessageId}", message.MessageId);
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                // Extract event details
                var detailType = messageBody.DetailType ?? messageBody.Detail?.GetProperty("DetailType").GetString();
                var eventSource = messageBody.Source ?? messageBody.Detail?.GetProperty("Source").GetString();
                
                _logger.LogInformation("Processing event {DetailType} from {Source}", detailType, eventSource);

                // Find handler for this event type
                if (!_eventTypeMap.TryGetValue(detailType, out var eventType))
                {
                    _logger.LogWarning("No handler registered for event type {DetailType}", detailType);
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                // Deserialize the event detail
                var eventData = messageBody.Detail.HasValue
                    ? JsonSerializer.Deserialize(messageBody.Detail.Value.GetRawText(), eventType, _jsonOptions)
                    : JsonSerializer.Deserialize(messageBody.DetailJson, eventType, _jsonOptions);

                // Create scope for dependency injection
                using var scope = _serviceProvider.CreateScope();
                
                // Find and invoke the handler
                var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var handler = scope.ServiceProvider.GetService(handlerType);
                
                if (handler == null)
                {
                    _logger.LogWarning("No handler service registered for {HandlerType}", handlerType);
                    await DeleteMessageAsync(message, cancellationToken);
                    return;
                }

                // Invoke the handler
                var handleMethod = handlerType.GetMethod("HandleAsync");
                var handleTask = (Task)handleMethod.Invoke(handler, new[] { eventData, cancellationToken });
                await handleTask;

                // Delete message after successful processing
                await DeleteMessageAsync(message, cancellationToken);
                
                _logger.LogDebug("Successfully processed message {MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                
                // Message will become visible again after visibility timeout
                // and will be retried up to max receive count before going to DLQ
            }
        }

        private async Task DeleteMessageAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = _queueUrl,
                    ReceiptHandle = message.ReceiptHandle
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", message.MessageId);
            }
        }

        private void RegisterEventTypes()
        {
            // Register default event types based on service name
            // Services should override this or call RegisterEventType explicitly
            switch (_serviceName.ToLower())
            {
                case "compliance":
                case "complianceservice":
                    RegisterEventType<UserCreatedEvent>("User.Created");
                    RegisterEventType<UserUpdatedEvent>("User.Updated");
                    RegisterEventType<TransactionInitiatedEvent>("Transaction.Initiated");
                    break;
                    
                case "payment":
                case "paymentgateway":
                case "paymentgatewayservice":
                    RegisterEventType<PaymentRequestedEvent>("Payment.Requested");
                    RegisterEventType<OrderCreatedEvent>("Order.Created");
                    RegisterEventType<WalletFundedEvent>("Wallet.Funded");
                    break;
                    
                case "notification":
                case "notificationservice":
                    RegisterEventType<NotificationSendEvent>("Notification.Send");
                    RegisterEventType<UserWelcomeEvent>("User.Welcome");
                    RegisterEventType<PaymentConfirmedEvent>("Payment.Confirmed");
                    RegisterEventType<SystemAlertEvent>("System.Alert");
                    break;
                    
                // Add more service-specific mappings as needed
            }
        }
    }

    /// <summary>
    /// Interface for event handlers
    /// </summary>
    public interface IEventHandler<T> where T : class
    {
        Task HandleAsync(T @event, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// EventBridge message structure as received from SQS
    /// </summary>
    public class EventBridgeMessage
    {
        public string Version { get; set; }
        public string Id { get; set; }
        public string DetailType { get; set; }
        public string Source { get; set; }
        public string Account { get; set; }
        public DateTime Time { get; set; }
        public string Region { get; set; }
        public List<string> Resources { get; set; }
        public JsonElement? Detail { get; set; }
        public string DetailJson { get; set; } // Fallback for string detail
    }

    // Sample event classes - services should define their own
    public class UserCreatedEvent : DomainEvent
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public override string GetEventSource() => "qsl.user";
        public override string GetDetailType() => "User.Created";
    }

    public class UserUpdatedEvent : DomainEvent
    {
        public string Username { get; set; }
        public Dictionary<string, object> Changes { get; set; }
        public override string GetEventSource() => "qsl.user";
        public override string GetDetailType() => "User.Updated";
    }

    public class TransactionInitiatedEvent : DomainEvent
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public override string GetEventSource() => "qsl.transaction";
        public override string GetDetailType() => "Transaction.Initiated";
    }

    public class PaymentRequestedEvent : DomainEvent
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public override string GetEventSource() => "qsl.payment";
        public override string GetDetailType() => "Payment.Requested";
    }

    public class OrderCreatedEvent : DomainEvent
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public override string GetEventSource() => "qsl.order";
        public override string GetDetailType() => "Order.Created";
    }

    public class WalletFundedEvent : DomainEvent
    {
        public string WalletId { get; set; }
        public decimal Amount { get; set; }
        public override string GetEventSource() => "qsl.wallet";
        public override string GetDetailType() => "Wallet.Funded";
    }

    public class NotificationSendEvent : DomainEvent
    {
        public string RecipientId { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; }
        public override string GetEventSource() => "qsl.notification";
        public override string GetDetailType() => "Notification.Send";
    }

    public class UserWelcomeEvent : DomainEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public override string GetEventSource() => "qsl.user";
        public override string GetDetailType() => "User.Welcome";
    }

    public class PaymentConfirmedEvent : DomainEvent
    {
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public override string GetEventSource() => "qsl.payment";
        public override string GetDetailType() => "Payment.Confirmed";
    }

    public class SystemAlertEvent : DomainEvent
    {
        public string AlertType { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }
        public override string GetEventSource() => "qsl.system";
        public override string GetDetailType() => "System.Alert";
    }
}
