namespace ComplianceService.Models.Responses;

public class KycStatusResponse
{
    public Guid VerificationId { get; set; }
    public Guid UserId { get; set; }
    public string ComplyCubeClientId { get; set; } = string.Empty;
    public string? ComplyCubeCheckId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string KycLevel { get; set; } = string.Empty;
    public string TriggerReason { get; set; } = string.Empty;
    public decimal? RiskScore { get; set; }
    public string? FailureReason { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class KycListResponse
{
    public List<KycStatusResponse> Verifications { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
