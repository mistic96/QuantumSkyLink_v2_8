using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Responses;

public class VerificationStatusSummaryResponse
{
    public Guid AccountId { get; set; }

    public bool IsFullyVerified { get; set; } = false;

    public List<string> VerificationStatuses { get; set; } = new();

    [StringLength(50)]
    public string OverallStatus { get; set; } = string.Empty; // Verified, Pending, Failed, Expired

    [StringLength(50)]
    public string VerificationLevel { get; set; } = string.Empty; // Basic, Enhanced, Premium

    public decimal CompletionPercentage { get; set; }

    public DateTime? LastVerificationDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public List<string> CompletedVerifications { get; set; } = new();

    public List<string> PendingVerifications { get; set; } = new();

    public List<string> FailedVerifications { get; set; } = new();

    public int TotalRequirements { get; set; }

    public int CompletedRequirements { get; set; }

    public int PendingRequirements { get; set; }

    public int FailedRequirements { get; set; }

    [StringLength(1000)]
    public string? Summary { get; set; }

    [StringLength(500)]
    public string? NextSteps { get; set; }

    public bool RequiresImmediateAction { get; set; } = false;

    public bool IsExpired { get; set; } = false;

    [StringLength(100)]
    public string? RiskLevel { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [StringLength(2000)]
    public string? AdditionalNotes { get; set; }
}
