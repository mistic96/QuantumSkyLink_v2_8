using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("ExchangeRates")]
[Index(nameof(FromCurrency), nameof(ToCurrency), nameof(Timestamp), IsUnique = true)]
[Index(nameof(Provider), nameof(Timestamp))]
[Index(nameof(IsActive))]
public class ExchangeRate
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string FromCurrency { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string ToCurrency { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Rate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty; // "CoinGecko", "Binance", "Coinbase", "Fixer"

    [Required]
    public DateTime Timestamp { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Bid { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Ask { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Volume24h { get; set; }

    [Column(TypeName = "decimal(8,6)")]
    public decimal? Change24h { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [MaxLength(1000)]
    public string? Metadata { get; set; } // JSON for additional provider-specific data

    // Navigation properties
    public virtual ICollection<FeeCalculationResult> CalculationResults { get; set; } = new List<FeeCalculationResult>();
}
