using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGatewayService.Data.Entities;

[Table("PaymentProviders")]
public class PaymentProvider
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public PaymentGatewayType GatewayType { get; set; } = PaymentGatewayType.Square;

    public bool IsActive { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Configuration { get; set; }

    [StringLength(100)]
    public string? ApiEndpoint { get; set; }

    [StringLength(50)]
    public string? ApiVersion { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal ProcessingFeePercentage { get; set; } = 0.029m; // 2.9%

    [Column(TypeName = "decimal(18,8)")]
    public decimal FixedFeeAmount { get; set; } = 0.30m; // $0.30

    [StringLength(10)]
    public string FeeCurrency { get; set; } = "USD";

    public int Priority { get; set; } = 100; // Lower number = higher priority

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinTransactionAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaxTransactionAmount { get; set; }

    [StringLength(1000)]
    public string? SupportedCurrencies { get; set; } // JSON array of supported currencies

    [StringLength(1000)]
    public string? SupportedCountries { get; set; } // JSON array of supported countries

    [StringLength(500)]
    public string? SupportedPaymentMethods { get; set; } // JSON array of supported payment methods

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? LastHealthCheckAt { get; set; }

    public bool IsHealthy { get; set; } = true;

    [StringLength(500)]
    public string? HealthCheckMessage { get; set; }

    // Navigation properties
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
}
