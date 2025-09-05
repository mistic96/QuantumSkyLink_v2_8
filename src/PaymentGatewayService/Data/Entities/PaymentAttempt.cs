using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents an attempt to process a payment transaction
/// </summary>
[Table("PaymentAttempts")]
[Index(nameof(PaymentId))]
[Index(nameof(Status))]
[Index(nameof(CreatedAt))]
[Index(nameof(AttemptNumber))]
[Index(nameof(GatewayTransactionId))]
public class PaymentAttempt : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment attempt
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the payment ID this attempt belongs to
    /// </summary>
    [Required]
    [ForeignKey("Payment")]
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the current status of this payment attempt
    /// </summary>
    [Required]
    public PaymentAttemptStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the external gateway transaction ID for this attempt
    /// </summary>
    [MaxLength(255)]
    public string? GatewayTransactionId { get; set; }

    /// <summary>
    /// Gets or sets the error code if the attempt failed
    /// </summary>
    [MaxLength(500)]
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the error message if the attempt failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the full gateway response (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string? GatewayResponse { get; set; }

    /// <summary>
    /// Gets or sets the attempt number (1, 2, 3, etc.)
    /// </summary>
    [Required]
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Gets or sets the amount attempted in this specific attempt
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency for this attempt
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the attempt was processed by the gateway
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Gets or sets when the attempt will be retried (if applicable)
    /// </summary>
    public DateTime? RetryAt { get; set; }

    /// <summary>
    /// Gets or sets the processing time in milliseconds
    /// </summary>
    public int? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the client IP address for security tracking
    /// </summary>
    [MaxLength(45)]
    public string? ClientIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent for security tracking
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the attempt (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the gateway fee for this attempt
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? GatewayFee { get; set; }

    /// <summary>
    /// Gets or sets the network fee for this attempt (for crypto)
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? NetworkFee { get; set; }

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

    // Navigation properties

    /// <summary>
    /// Gets or sets the payment this attempt belongs to
    /// </summary>
    public Payment Payment { get; set; } = null!;
}
