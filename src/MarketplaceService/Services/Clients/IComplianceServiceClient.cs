using Refit;

namespace MarketplaceService.Services.Clients;

/// <summary>
/// Refit client interface for ComplianceService integration
/// </summary>
public interface IComplianceServiceClient
{
    /// <summary>
    /// Verify user compliance for marketplace operations
    /// </summary>
    [Post("/api/compliance/verify/marketplace")]
    Task<ComplianceVerificationResponse> VerifyMarketplaceComplianceAsync(
        [Body] MarketplaceComplianceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check transaction compliance
    /// </summary>
    [Post("/api/compliance/verify/transaction")]
    Task<TransactionComplianceResponse> VerifyTransactionComplianceAsync(
        [Body] TransactionComplianceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Screen user for AML/KYC compliance
    /// </summary>
    [Post("/api/compliance/screen/user")]
    Task<UserComplianceScreeningResponse> ScreenUserComplianceAsync(
        [Body] UserComplianceScreeningRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Report suspicious marketplace activity
    /// </summary>
    [Post("/api/compliance/report/suspicious-activity")]
    Task<SuspiciousActivityReportResponse> ReportSuspiciousActivityAsync(
        [Body] SuspiciousActivityReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get compliance requirements for asset type
    /// </summary>
    [Get("/api/compliance/requirements/{assetType}")]
    Task<ComplianceRequirementsResponse> GetComplianceRequirementsAsync(
        string assetType,
        [Query] string? jurisdiction = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request models for ComplianceService integration
/// </summary>
public class MarketplaceComplianceRequest
{
    public Guid UserId { get; set; }
    public string OperationType { get; set; } = string.Empty; // "CreateListing", "PurchaseToken", "SellToken"
    public string AssetType { get; set; } = string.Empty; // "PlatformToken", "ExternalCrypto"
    public string? AssetSymbol { get; set; }
    public decimal? TransactionAmount { get; set; }
    public string? Currency { get; set; }
    public string? UserJurisdiction { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TransactionComplianceRequest
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string AssetSymbol { get; set; } = string.Empty;
    public decimal TransactionAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string TransactionType { get; set; } = string.Empty; // "Primary", "Secondary"
    public string? BuyerJurisdiction { get; set; }
    public string? SellerJurisdiction { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UserComplianceScreeningRequest
{
    public Guid UserId { get; set; }
    public string ScreeningType { get; set; } = string.Empty; // "KYC", "AML", "Sanctions", "PEP"
    public string? UserJurisdiction { get; set; }
    public decimal? TransactionAmount { get; set; }
    public string? Currency { get; set; }
    public Dictionary<string, object>? UserData { get; set; }
}

public class SuspiciousActivityReportRequest
{
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // "UnusualTrading", "HighVelocity", "SuspiciousPattern"
    public string Description { get; set; } = string.Empty;
    public decimal? TransactionAmount { get; set; }
    public string? Currency { get; set; }
    public Guid? RelatedTransactionId { get; set; }
    public Guid? RelatedListingId { get; set; }
    public DateTime ActivityDate { get; set; }
    public Dictionary<string, object>? Evidence { get; set; }
}

/// <summary>
/// Response models for ComplianceService integration
/// </summary>
public class ComplianceVerificationResponse
{
    public bool IsCompliant { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty; // "Approved", "Rejected", "RequiresReview"
    public List<ComplianceCheck> ComplianceChecks { get; set; } = new();
    public List<string> RequiredActions { get; set; } = new();
    public string? RejectionReason { get; set; }
    public DateTime VerifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class ComplianceCheck
{
    public string CheckType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Details { get; set; }
    public decimal? RiskScore { get; set; }
}

public class TransactionComplianceResponse
{
    public bool IsApproved { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty; // "Approved", "Rejected", "RequiresManualReview"
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
    public List<ComplianceFlag> Flags { get; set; } = new();
    public List<string> RequiredDocuments { get; set; } = new();
    public string? RejectionReason { get; set; }
    public DateTime AssessedAt { get; set; }
}

public class ComplianceFlag
{
    public string FlagType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "Low", "Medium", "High", "Critical"
    public string Description { get; set; } = string.Empty;
    public bool RequiresAction { get; set; }
}

public class UserComplianceScreeningResponse
{
    public bool Passed { get; set; }
    public string ScreeningStatus { get; set; } = string.Empty; // "Clear", "Flagged", "Blocked"
    public decimal RiskScore { get; set; }
    public List<ScreeningResult> ScreeningResults { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime ScreenedAt { get; set; }
    public DateTime? NextScreeningDue { get; set; }
}

public class ScreeningResult
{
    public string ScreeningType { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Matches { get; set; } = new();
    public decimal ConfidenceScore { get; set; }
}

public class SuspiciousActivityReportResponse
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid ReportId { get; set; }
    public string ReportStatus { get; set; } = string.Empty; // "Filed", "UnderReview", "Escalated"
    public string? CaseNumber { get; set; }
    public DateTime ReportedAt { get; set; }
    public bool RequiresImmediateAction { get; set; }
}

public class ComplianceRequirementsResponse
{
    public string AssetType { get; set; } = string.Empty;
    public string? Jurisdiction { get; set; }
    public List<ComplianceRequirement> Requirements { get; set; } = new();
    public List<string> ProhibitedJurisdictions { get; set; } = new();
    public decimal? MaxTransactionAmount { get; set; }
    public string? Currency { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class ComplianceRequirement
{
    public string RequirementType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public decimal? ThresholdAmount { get; set; }
    public string? ApplicableJurisdictions { get; set; }
    public List<string> RequiredDocuments { get; set; } = new();
}
