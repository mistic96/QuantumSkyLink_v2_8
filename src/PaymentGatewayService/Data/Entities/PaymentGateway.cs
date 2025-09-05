using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents a payment gateway configuration in the system
/// </summary>
[Table("PaymentGateways")]
[Index(nameof(GatewayType))]
[Index(nameof(IsActive))]
[Index(nameof(Name), IsUnique = true)]
public class PaymentGateway : ITimestampEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment gateway
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the payment gateway
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of payment gateway
    /// </summary>
    [Required]
    public PaymentGatewayType GatewayType { get; set; }

    /// <summary>
    /// Gets or sets whether the gateway is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the gateway is in test mode
    /// </summary>
    [Required]
    public bool IsTestMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the gateway configuration as JSON
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the fee percentage charged by this gateway
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(5,4)")]
    public decimal FeePercentage { get; set; } = 0;

    /// <summary>
    /// Gets or sets the fixed fee amount charged by this gateway
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal FixedFee { get; set; } = 0;

    /// <summary>
    /// Gets or sets the minimum transaction amount for this gateway
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumAmount { get; set; }

    /// <summary>
    /// Gets or sets the maximum transaction amount for this gateway
    /// </summary>
    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaximumAmount { get; set; }

    /// <summary>
    /// Gets or sets the supported currencies (comma-separated)
    /// </summary>
    [MaxLength(500)]
    public string SupportedCurrencies { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the supported countries (comma-separated ISO codes)
    /// </summary>
    [MaxLength(1000)]
    public string? SupportedCountries { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL for this gateway
    /// </summary>
    [MaxLength(500)]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets the webhook secret for verification
    /// </summary>
    [MaxLength(255)]
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint URL for this gateway
    /// </summary>
    [MaxLength(500)]
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the priority order for gateway selection
    /// </summary>
    [Required]
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets additional metadata for the gateway (JSON)
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string Metadata { get; set; } = "{}";

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
    /// Gets or sets the collection of payments processed through this gateway
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Gets or sets the collection of payment methods associated with this gateway
    /// </summary>
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}
