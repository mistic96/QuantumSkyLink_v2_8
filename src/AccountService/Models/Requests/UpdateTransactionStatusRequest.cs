using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class UpdateTransactionStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Completed, Failed, Cancelled, Processing

    [StringLength(500)]
    public string? StatusReason { get; set; }

    [StringLength(1000)]
    public string? ProcessingNotes { get; set; }

    public Guid? ProcessedBy { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [StringLength(200)]
    public string? ExternalReference { get; set; }

    [StringLength(1000)]
    public string? AdditionalMetadata { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
