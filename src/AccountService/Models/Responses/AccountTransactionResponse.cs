using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class AccountTransactionResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string TransactionType { get; set; } = string.Empty;

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
    public string Status { get; set; } = string.Empty;

    public DateTime? ScheduledDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Fee { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? BalanceAfter { get; set; }

    [StringLength(500)]
    public string? ProcessingNotes { get; set; }
}
