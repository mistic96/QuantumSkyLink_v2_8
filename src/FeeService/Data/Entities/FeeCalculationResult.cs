using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("FeeCalculationResults")]
[Index(nameof(UserId), nameof(CreatedAt))]
[Index(nameof(ReferenceId), nameof(ReferenceType))]
[Index(nameof(FeeType), nameof(CreatedAt))]
public class FeeCalculationResult
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ReferenceId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ReferenceType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal BaseAmount { get; set; }

    [Required]
    [MaxLength(10)]
    public string BaseCurrency { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal CalculatedFee { get; set; }

    [Required]
    [MaxLength(10)]
    public string FeeCurrency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? DiscountAmount { get; set; }

    [Column(TypeName = "decimal(8,6)")]
    public decimal? DiscountPercentage { get; set; }

    [MaxLength(255)]
    public string? DiscountReason { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal FinalFeeAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? UsedExchangeRate { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    [Column(TypeName = "jsonb")]
    public string? CalculationDetails { get; set; } // JSON for detailed calculation breakdown

    [Column(TypeName = "jsonb")]
    public string? AppliedRules { get; set; } // JSON for applied fee rules and discounts

    // Foreign Keys
    [ForeignKey("FeeConfiguration")]
    public Guid FeeConfigurationId { get; set; }

    [ForeignKey("ExchangeRate")]
    public Guid? ExchangeRateId { get; set; }

    // Navigation properties
    public virtual FeeConfiguration FeeConfiguration { get; set; } = null!;
    public virtual ExchangeRate? ExchangeRate { get; set; }
    public virtual ICollection<FeeTransaction> FeeTransactions { get; set; } = new List<FeeTransaction>();
}
