using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("TreasuryTransactions")]
[Index(nameof(TransactionNumber), IsUnique = true)]
[Index(nameof(AccountId))]
[Index(nameof(TransactionType))]
[Index(nameof(Status))]
[Index(nameof(TransactionDate))]
[Index(nameof(Reference))]
public class TreasuryTransaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Transfer, Allocation, Interest, Fee

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal BalanceBefore { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal BalanceAfter { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Pending, Processing, Completed, Failed, Cancelled, Reversed

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(100)]
    public string? ExternalTransactionId { get; set; }

    public DateTime TransactionDate { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public DateTime? SettledDate { get; set; }

    [MaxLength(100)]
    public string? CounterpartyName { get; set; }

    [MaxLength(100)]
    public string? CounterpartyAccount { get; set; }

    public Guid? RelatedAccountId { get; set; } // For transfers

    public Guid? FundAllocationId { get; set; } // If part of fund allocation

    public Guid? ParentTransactionId { get; set; } // For reversals or related transactions

    [MaxLength(50)]
    public string? PaymentMethod { get; set; } // Wire, ACH, Check, Internal

    [Column(TypeName = "decimal(18,8)")]
    public decimal? FeeAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? ExchangeRate { get; set; }

    [MaxLength(10)]
    public string? OriginalCurrency { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? OriginalAmount { get; set; }

    [MaxLength(1000)]
    public string? ProcessingDetails { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    [MaxLength(1000)]
    public string? CancellationReason { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public bool RequiresApproval { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [MaxLength(100)]
    public string? CorrelationId { get; set; }

    [MaxLength(2000)]
    public string? Metadata { get; set; } // JSON for additional data

    // Navigation Properties
    [ForeignKey(nameof(AccountId))]
    [InverseProperty("Transactions")]
    public TreasuryAccount Account { get; set; } = null!;

    [ForeignKey(nameof(RelatedAccountId))]
    [InverseProperty("RelatedTransactions")]
    public TreasuryAccount? RelatedAccount { get; set; }

    [ForeignKey(nameof(FundAllocationId))]
    public FundAllocation? FundAllocation { get; set; }

    [ForeignKey(nameof(ParentTransactionId))]
    [InverseProperty("ChildTransactions")]
    public TreasuryTransaction? ParentTransaction { get; set; }

    [InverseProperty("ParentTransaction")]
    public ICollection<TreasuryTransaction> ChildTransactions { get; set; } = new List<TreasuryTransaction>();
}
