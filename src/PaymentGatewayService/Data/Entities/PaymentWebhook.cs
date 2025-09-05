using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents a webhook event received from a payment gateway
/// </summary>
[Table("PaymentWebhooks")]
[Index(nameof(GatewayType))]
[Index(nameof(EventType))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(ProcessedAt))]
[Index(nameof(ExternalEventId))]
public class PaymentWebhook : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the webhook event
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment gateway type that sent this webhook
    /// </summary>
    [Required]
    public PaymentGatewayType GatewayType { get; set; }

    /// <summary>
    /// Gets or sets the type of event (payment.succeeded, payment.failed, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external event ID from the gateway
    /// </summary>
    [MaxLength(255)]
    public string? ExternalEventId { get; set; }

    /// <summary>
    /// Gets or sets the full webhook payload (JSON)
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string Payload { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the current status of webhook processing
    /// </summary>
    [Required]
    public WebhookStatus Status { get; set; }

    /// <summary>
    /// Gets or sets when the webhook was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if processing failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the related payment ID (if applicable)
    /// </summary>
    public Guid? RelatedPaymentId { get; set; }

    /// <summary>
    /// Gets or sets the related refund ID (if applicable)
    /// </summary>
    public Guid? RelatedRefundId { get; set; }

    /// <summary>
    /// Gets or sets the related payment method ID (if applicable)
    /// </summary>
    public Guid? RelatedPaymentMethodId { get; set; }

    /// <summary>
    /// Gets or sets the number of processing attempts
    /// </summary>
    [Required]
    public int ProcessingAttempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets when the next retry should occur
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts
    /// </summary>
    [Required]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the webhook signature for verification
    /// </summary>
    [MaxLength(500)]
    public string? Signature { get; set; }

    /// <summary>
    /// Gets or sets whether the signature was verified
    /// </summary>
    [Required]
    public bool SignatureVerified { get; set; } = false;

    /// <summary>
    /// Gets or sets the source IP address of the webhook
    /// </summary>
    [MaxLength(45)]
    public string? SourceIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the webhook request
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional headers from the webhook request (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? Headers { get; set; }

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public int? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the webhook (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets whether this webhook is a duplicate
    /// </summary>
    [Required]
    public bool IsDuplicate { get; set; } = false;

    /// <summary>
    /// Gets or sets the original webhook ID if this is a duplicate
    /// </summary>
    public Guid? OriginalWebhookId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; }
}
