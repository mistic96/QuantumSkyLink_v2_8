using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class InitiateKycWorkflowRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string WorkflowType { get; set; } = string.Empty; // Basic, Enhanced, Premium, Business

    [Required]
    [StringLength(50)]
    public string VerificationLevel { get; set; } = string.Empty; // Level1, Level2, Level3

    [StringLength(500)]
    public string? Reason { get; set; }

    public Guid? InitiatedBy { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    public List<string> RequiredDocuments { get; set; } = new();

    public List<string> RequiredChecks { get; set; } = new();

    [StringLength(1000)]
    public string? SpecialInstructions { get; set; }

    public DateTime? Deadline { get; set; }

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    [StringLength(50)]
    public string? KycLevel { get; set; }
}
