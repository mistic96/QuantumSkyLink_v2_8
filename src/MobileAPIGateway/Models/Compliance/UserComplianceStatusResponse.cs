namespace MobileAPIGateway.Models.Compliance;

/// <summary>
/// User compliance status response for mobile
/// </summary>
public class UserComplianceStatusResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Overall compliance status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Compliance level (Basic, Standard, Premium)
    /// </summary>
    public string ComplianceLevel { get; set; } = string.Empty;

    /// <summary>
    /// Compliance score (0-100)
    /// </summary>
    public int ComplianceScore { get; set; }

    /// <summary>
    /// Whether user is compliant
    /// </summary>
    public bool IsCompliant { get; set; }

    /// <summary>
    /// KYC completion status
    /// </summary>
    public bool KycCompleted { get; set; }

    /// <summary>
    /// AML check status
    /// </summary>
    public bool AmlCleared { get; set; }

    /// <summary>
    /// Document verification status
    /// </summary>
    public bool DocumentsVerified { get; set; }

    /// <summary>
    /// Last compliance check date
    /// </summary>
    public DateTime LastCheckDate { get; set; }

    /// <summary>
    /// Next compliance review date
    /// </summary>
    public DateTime? NextReviewDate { get; set; }

    /// <summary>
    /// List of pending requirements
    /// </summary>
    public IEnumerable<string> PendingRequirements { get; set; } = new List<string>();

    /// <summary>
    /// Number of unread compliance alerts
    /// </summary>
    public int UnreadAlertsCount { get; set; }

    /// <summary>
    /// Compliance restrictions
    /// </summary>
    public IEnumerable<string> Restrictions { get; set; } = new List<string>();

    /// <summary>
    /// Risk level assessment
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;
}
