namespace ComplianceService.Models.Responses;

public class CaseResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? KycVerificationId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string CaseType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public bool RequiresComplianceOfficerReview { get; set; }
    public bool RequiresAIReview { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public List<CaseDocumentResponse> Documents { get; set; } = new();
    public List<CaseReviewResponse> Reviews { get; set; } = new();
}

public class CaseDocumentResponse
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Comments { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? ProcessingResult { get; set; }
}

public class CaseReviewResponse
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public string ReviewType { get; set; } = string.Empty;
    public string ReviewResult { get; set; } = string.Empty;
    public string ReviewNotes { get; set; } = string.Empty;
    public string? DetailedAnalysis { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public string? RecommendedAction { get; set; }
    public DateTime ReviewedAt { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public string? NextReviewBy { get; set; }
    public DateTime? NextReviewDate { get; set; }
}

public class CaseListResponse
{
    public List<CaseResponse> Cases { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ComplianceStatusResponse
{
    public Guid UserId { get; set; }
    public bool IsKycCompliant { get; set; }
    public string? CurrentKycStatus { get; set; }
    public string? KycLevel { get; set; }
    public int OpenCases { get; set; }
    public int PendingReviews { get; set; }
    public DateTime? LastKycUpdate { get; set; }
    public List<string> RequiredActions { get; set; } = new();
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
}
