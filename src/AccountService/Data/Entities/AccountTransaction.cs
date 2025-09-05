using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data.Entities;

[Table("AccountTransactions")]
[Index(nameof(AccountId), nameof(Timestamp), IsUnique = false)]
[Index(nameof(TransactionType), IsUnique = false)]
[Index(nameof(Status), IsUnique = false)]
[Index(nameof(CorrelationId), IsUnique = false)]
public class AccountTransaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Account")]
    public Guid AccountId { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }

    [Required]
    public TransactionStatus Status { get; set; }

    [MaxLength(50)]
    public string? CorrelationId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Fee { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal BalanceAfter { get; set; }

    [MaxLength(100)]
    public string? ProcessedBy { get; set; }

    [MaxLength(1000)]
    public string? Metadata { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Account Account { get; set; } = null!;
}

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Fee = 4,
    Refund = 5,
    Interest = 6,
    Dividend = 7,
    Purchase = 8,
    Sale = 9,
    Adjustment = 10
}

public enum TransactionStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Reversed = 6
}
