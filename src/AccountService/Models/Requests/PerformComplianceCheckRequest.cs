using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class PerformComplianceCheckRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string CheckType { get; set; } = string.Empty; // AML, Sanctions, PEP, Watchlist

    [StringLength(500)]
    public string? Reason { get; set; }

    public Guid? RequestedBy { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    public List<string> CheckParameters { get; set; } = new();

    [StringLength(1000)]
    public string? SpecialInstructions { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }
}
