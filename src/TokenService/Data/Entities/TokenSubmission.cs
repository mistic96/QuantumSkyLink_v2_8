using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TokenService.Data.Entities;

[Table("TokenSubmissions")]
[Index(nameof(CreatorId), nameof(SubmissionDate))]
[Index(nameof(AiComplianceScore))]
[Index(nameof(ApprovalStatus))]
public class TokenSubmission
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid CreatorId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string TokenPurpose { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string UseCase { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "jsonb")]
    public string ConfigurationJson { get; set; } = "{}";
    
    [Range(0, 100)]
    public decimal AiComplianceScore { get; set; }
    
    [Required]
    [StringLength(20)]
    public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, Requires_Improvement
    
    [Required]
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReviewedAt { get; set; }
    
    [StringLength(100)]
    public string? ReviewedBy { get; set; }
    
    [StringLength(500)]
    public string? ReviewComments { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? AiRecommendations { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? AiRedFlags { get; set; }
    
    // Asset verification details
    [StringLength(50)]
    public string? AssetType { get; set; }
    
    [StringLength(100)]
    public string? AssetVerificationId { get; set; }
    
    [StringLength(20)]
    public string? AssetVerificationStatus { get; set; } // Pending, Verified, Failed
    
    public DateTime? AssetVerificationDate { get; set; }
    
    // Foreign key to Token (nullable until token is created)
    public Guid? TokenId { get; set; }
    public Token? Token { get; set; }
    
    /// <summary>
    /// Validates the token submission
    /// </summary>
    public bool IsValid()
    {
        if (CreatorId == Guid.Empty) return false;
        if (string.IsNullOrWhiteSpace(TokenPurpose) || TokenPurpose.Length > 500) return false;
        if (string.IsNullOrWhiteSpace(UseCase) || UseCase.Length > 1000) return false;
        if (AiComplianceScore < 0 || AiComplianceScore > 100) return false;
        
        return true;
    }
    
    /// <summary>
    /// Marks the submission as reviewed
    /// </summary>
    public void MarkAsReviewed(string reviewedBy, string status, string? comments = null)
    {
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        ApprovalStatus = status;
        ReviewComments = comments;
    }
}
