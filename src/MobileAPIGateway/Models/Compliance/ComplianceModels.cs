namespace MobileAPIGateway.Models.Compliance;

/// <summary>
/// KYC status response for mobile
/// </summary>
public class KycStatusResponse
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public IEnumerable<string> RequiredDocuments { get; set; } = new List<string>();
    public IEnumerable<string> SubmittedDocuments { get; set; } = new List<string>();
    public IEnumerable<string> PendingDocuments { get; set; } = new List<string>();
    public string NextStep { get; set; } = string.Empty;
}

/// <summary>
/// Transaction compliance request
/// </summary>
public class TransactionComplianceRequest
{
    public Guid UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CounterpartyId { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Transaction compliance response
/// </summary>
public class TransactionComplianceResponse
{
    public bool IsCompliant { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<string> Violations { get; set; } = new List<string>();
    public IEnumerable<string> Warnings { get; set; } = new List<string>();
    public string RiskLevel { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public string? ApprovalWorkflow { get; set; }
    public DateTime CheckedAt { get; set; }
}

/// <summary>
/// Compliance alert response
/// </summary>
public class ComplianceAlertResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionRequired { get; set; }
    public string? ActionUrl { get; set; }
}

/// <summary>
/// Compliance requirement response
/// </summary>
public class ComplianceRequirementResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public DateTime? DueDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public IEnumerable<string> RequiredActions { get; set; } = new List<string>();
}

/// <summary>
/// Document submission request
/// </summary>
public class DocumentSubmissionRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Document submission response
/// </summary>
public class DocumentSubmissionResponse
{
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Document status response
/// </summary>
public class DocumentStatusResponse
{
    public Guid DocumentId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public IEnumerable<string> Issues { get; set; } = new List<string>();
}

/// <summary>
/// Compliance summary response
/// </summary>
public class ComplianceSummaryResponse
{
    public Guid UserId { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public int ComplianceScore { get; set; }
    public int TotalRequirements { get; set; }
    public int CompletedRequirements { get; set; }
    public int PendingRequirements { get; set; }
    public int AlertsCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public IEnumerable<string> QuickActions { get; set; } = new List<string>();
}

/// <summary>
/// Compliance history response
/// </summary>
public class ComplianceHistoryResponse
{
    public IEnumerable<ComplianceHistoryItem> Items { get; set; } = new List<ComplianceHistoryItem>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Compliance history item
/// </summary>
public class ComplianceHistoryItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Details { get; set; }
}
