using System.ComponentModel.DataAnnotations;

namespace TokenService.Models.Requests;

public class TokenSubmissionRequest
{
    [Required]
    public Guid CreatorId { get; set; }
    
    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string TokenPurpose { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000, MinimumLength = 20)]
    public string UseCase { get; set; } = string.Empty;
    
    [Required]
    public TokenConfiguration Configuration { get; set; } = new();
    
    public AssetTokenizationDetails? AssetDetails { get; set; }
}

public class TokenConfiguration
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20, MinimumLength = 2)]
    [RegularExpression(@"^[A-Z][A-Z0-9]*$", ErrorMessage = "Symbol must start with a letter and contain only uppercase letters and numbers")]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 1000000000000000000)]
    public decimal TotalSupply { get; set; }
    
    [Required]
    [Range(0, 18)]
    public int Decimals { get; set; } = 18;
    
    [Required]
    [RegularExpression(@"^(ERC20|ERC721|ERC1155)$", ErrorMessage = "TokenType must be ERC20, ERC721, or ERC1155")]
    public string TokenType { get; set; } = "ERC20";
    
    public bool CrossChainEnabled { get; set; }
    
    [StringLength(20)]
    public string Network { get; set; } = "MultiChain";
    
    [StringLength(50)]
    public string? AssetType { get; set; }
    
    public Dictionary<string, object>? AssetMetadata { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
}

public class AssetTokenizationDetails
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

public class AssetDocument
{
    [Required]
    [StringLength(100)]
    public string DocumentType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? DocumentHash { get; set; }
}
