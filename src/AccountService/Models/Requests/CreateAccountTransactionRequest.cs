using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class CreateAccountTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Transfer, Fee, etc.

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public Guid? RelatedAccountId { get; set; }

    [StringLength(200)]
    public string? ExternalReference { get; set; }

    [StringLength(1000)]
    public string? Metadata { get; set; }

    public Guid? InitiatedBy { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    public DateTime? ScheduledDate { get; set; }

    [StringLength(100)]
    public string? ExternalTransactionId { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Fee { get; set; }

    public Guid? ProcessedBy { get; set; }
}
