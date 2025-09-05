using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TokenService.Data.Entities;

[Table("Tokens")]
[Index(nameof(Symbol), nameof(Network), IsUnique = true)]
[Index(nameof(CreatorId), nameof(Status))]
[Index(nameof(QuantumLedgerAccountId), IsUnique = true)]
[Index(nameof(ApprovalStatus))]
public class Token
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(36,0)")]
    public decimal TotalSupply { get; set; }
    
    [Required]
    [Range(0, 18)]
    public int Decimals { get; set; }
    
    [Required]
    [StringLength(20)]
    public string TokenType { get; set; } = "ERC20"; // ERC20, ERC721, ERC1155
    
    [Required]
    public Guid CreatorId { get; set; }
    
    // QuantumLedger Integration
    [Required]
    public Guid QuantumLedgerAccountId { get; set; }
    
    [StringLength(100)]
    public string? QuantumLedgerExternalOwnerId { get; set; } // Format: "token_{TokenId}"
    
    [StringLength(200)]
    public string? QuantumLedgerSubstitutionKeyId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Active, Suspended, Burned
    
    [Required]
    [StringLength(20)]
    public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
    
    // Asset tokenization support
    [StringLength(50)]
    public string? AssetType { get; set; } // RealEstate, Commodity, Security, Digital
    
    [Column(TypeName = "jsonb")]
    public string? AssetMetadata { get; set; }
    
    // MultiChain support (since QL uses MultiChain backend)
    public bool CrossChainEnabled { get; set; }
    
    [StringLength(20)]
    public string Network { get; set; } = "MultiChain";
    
    [StringLength(100)]
    public string? MultiChainAssetName { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ApprovedAt { get; set; }
    
    [StringLength(100)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? LastUpdated { get; set; }
    
    // Navigation properties
    public ICollection<TokenSubmission> Submissions { get; set; } = new List<TokenSubmission>();
    public ICollection<TokenTransfer> Transfers { get; set; } = new List<TokenTransfer>();
    public ICollection<TokenBalance> Balances { get; set; } = new List<TokenBalance>();
    
    /// <summary>
    /// Updates the last updated timestamp
    /// </summary>
    public void Touch()
    {
        LastUpdated = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Validates the token entity
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 100) return false;
        if (string.IsNullOrWhiteSpace(Symbol) || Symbol.Length > 20) return false;
        if (TotalSupply <= 0) return false;
        if (Decimals < 0 || Decimals > 18) return false;
        if (CreatorId == Guid.Empty) return false;
        if (QuantumLedgerAccountId == Guid.Empty) return false;
        
        return true;
    }
}
