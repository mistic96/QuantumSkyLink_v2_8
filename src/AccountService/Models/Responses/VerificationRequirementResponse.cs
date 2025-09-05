using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class VerificationRequirementResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty; // Identity, Address, Income, etc.

    [StringLength(100)]
    public string RequirementType { get; set; } = string.Empty; // Document, Check, Verification

    [StringLength(100)]
    public string RequirementName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Required, Optional, Completed, Failed

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Instructions { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }

    [StringLength(1000)]
    public string? AdditionalNotes { get; set; }

    public List<string> AcceptableDocuments { get; set; } = new();

    public List<string> RequiredFields { get; set; } = new();

    [StringLength(2000)]
    public string? ValidationCriteria { get; set; }
}
