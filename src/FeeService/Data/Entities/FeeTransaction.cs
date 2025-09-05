using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Data.Entities;

[Table("FeeTransactions")]
[Index(nameof(UserId), nameof(Status))]
[Index(nameof(TransactionType), nameof(CreatedAt))]
[Index(nameof(ReferenceId), nameof(ReferenceType))]
[Index(nameof(Status), nameof(CreatedAt))]
public class FeeTransaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TransactionType { get; set; } = string.Empty; // "Collection", "Refund", "Distribution"

    [Required]
    [MaxLength(255)]
    public string ReferenceId { get; set; } = string.Empty; // Reference to the original transaction

    [Required]
    [MaxLength(100)]
    public string ReferenceType { get; set; } = string.Empty; // "TokenTransfer", "MarketplaceTrade", etc.

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ConvertedAmount { get; set; }

    [MaxLength(10)]
    public string? ConvertedCurrency { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ExchangeRate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Failed", "Refunded"

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(255)]
    public string? PaymentMethod { get; set; }

    [MaxLength(255)]
    public string? PaymentReference { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; } // JSON for additional transaction data

    // Foreign Keys
    [ForeignKey("FeeConfiguration")]
    public Guid? FeeConfigurationId { get; set; }

    [ForeignKey("FeeCalculationResult")]
    public Guid? CalculationResultId { get; set; }

    // Navigation properties
    public virtual FeeConfiguration? FeeConfiguration { get; set; }
    public virtual FeeCalculationResult? CalculationResult { get; set; }
    public virtual ICollection<FeeDistribution> Distributions { get; set; } = new List<FeeDistribution>();
}
