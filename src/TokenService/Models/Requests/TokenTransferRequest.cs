using System.ComponentModel.DataAnnotations;

namespace TokenService.Models.Requests;

public class TokenTransferRequest
{
    [Required]
    public Guid TokenId { get; set; }
    
    [Required]
    public Guid FromAccountId { get; set; }
    
    [Required]
    public Guid ToAccountId { get; set; }
    
    [Required]
    [Range(0.000000000000000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class TokenApprovalRequest
{
    [Required]
    public Guid SubmissionId { get; set; }
    
    [Required]
    public Guid AdminId { get; set; }
    
    [Required]
    [RegularExpression(@"^(Approved|Rejected)$", ErrorMessage = "Decision must be 'Approved' or 'Rejected'")]
    public string Decision { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Comments { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ReviewedBy { get; set; } = string.Empty;
}

public class AssetVerificationRequest
{
    [Required]
    [StringLength(50)]
    public string AssetType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string AssetId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? OwnershipProof { get; set; }
    
    public List<AssetDocument>? Documents { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CreateQuantumLedgerAccountRequest
{
    [Required]
    [StringLength(500)]
    public string ExternalOwnerId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? OwnerIdType { get; set; } = "TokenId";
    
    [StringLength(100)]
    public string? VendorSystem { get; set; } = "TokenService";
    
    [Required]
    public string OwnerType { get; set; } = "System";
    
    public List<string> Algorithms { get; set; } = new() { "Dilithium", "Falcon", "EC256" };
    
    public bool GenerateInternalReferenceId { get; set; } = true;
    
    public bool GenerateSubstitutionKey { get; set; } = true;
}

public class CreateQuantumLedgerTransactionRequest
{
    [Required]
    public Guid AccountId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [Range(0.000000000000000001, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Currency { get; set; } = string.Empty;
    
    public Guid? ToAccountId { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}
