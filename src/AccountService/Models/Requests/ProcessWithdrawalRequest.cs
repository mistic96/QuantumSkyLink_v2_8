using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class ProcessWithdrawalRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    [StringLength(200)]
    public string? DestinationAccount { get; set; }

    [StringLength(1000)]
    public string? Metadata { get; set; }

    public Guid? ProcessedBy { get; set; }

    [StringLength(100)]
    public string? ConfirmationCode { get; set; }

    public bool RequiresApproval { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(200)]
    public string? BankDetails { get; set; }

    [StringLength(100)]
    public string? RoutingNumber { get; set; }

    public bool BypassLimits { get; set; } = false;

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
