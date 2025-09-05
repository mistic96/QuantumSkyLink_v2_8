using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class ProcessTransferRequest
{
    [Required]
    public Guid FromAccountId { get; set; }

    [Required]
    public Guid ToAccountId { get; set; }

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
    public string? TransferType { get; set; } // Internal, External, Wire, ACH

    [StringLength(1000)]
    public string? Metadata { get; set; }

    public Guid? ProcessedBy { get; set; }

    [StringLength(100)]
    public string? ConfirmationCode { get; set; }

    public bool RequiresApproval { get; set; } = false;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Fee { get; set; }

    public bool BypassLimits { get; set; } = false;

    public DateTime? ScheduledDate { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
