using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class UpdateVerificationStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Approved, Rejected, Pending, Review

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public bool RequiresAdditionalDocuments { get; set; } = false;

    public List<string> RequiredDocuments { get; set; } = new();

    [StringLength(500)]
    public string? NextSteps { get; set; }

    public DateTime? ExpiryDate { get; set; }
}
