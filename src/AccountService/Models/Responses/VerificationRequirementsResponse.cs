using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class VerificationRequirementsResponse
{
    public Guid AccountId { get; set; }

    [StringLength(50)]
    public string AccountType { get; set; } = string.Empty; // Individual, Business, Premium

    [StringLength(50)]
    public string VerificationLevel { get; set; } = string.Empty; // Basic, Enhanced, Premium

    public List<string> RequiredDocuments { get; set; } = new();

    public List<string> OptionalDocuments { get; set; } = new();

    public List<string> RequiredChecks { get; set; } = new();

    public List<string> CompletedRequirements { get; set; } = new();

    public List<string> PendingRequirements { get; set; } = new();

    public List<string> FailedRequirements { get; set; } = new();

    public decimal CompletionPercentage { get; set; }

    [StringLength(1000)]
    public string? Instructions { get; set; }

    [StringLength(500)]
    public string? NextSteps { get; set; }

    public DateTime? Deadline { get; set; }

    public bool IsUrgent { get; set; } = false;

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }

    [StringLength(2000)]
    public string? AdditionalNotes { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdated { get; set; }

    [StringLength(100)]
    public string? ContactForQuestions { get; set; }

    public Dictionary<string, string> RequirementDetails { get; set; } = new();
}
