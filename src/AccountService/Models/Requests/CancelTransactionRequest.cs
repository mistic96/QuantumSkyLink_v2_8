using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class CancelTransactionRequest
{
    [Required]
    [StringLength(500)]
    public string CancellationReason { get; set; } = string.Empty;

    public Guid? CancelledBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool ForceCancel { get; set; } = false;

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
