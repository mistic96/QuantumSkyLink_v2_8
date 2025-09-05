using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TreasuryService.Data.Entities;

[Table("TreasuryAccounts")]
[Index(nameof(AccountNumber), IsUnique = true)]
[Index(nameof(AccountType))]
[Index(nameof(Currency))]
[Index(nameof(Status))]
public class TreasuryAccount
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string AccountType { get; set; } = string.Empty; // Operating, Reserve, Investment, Emergency

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal AvailableBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal ReservedBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal MinimumBalance { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal MaximumBalance { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Active, Inactive, Frozen, Closed

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ExternalAccountId { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? RoutingNumber { get; set; }

    [MaxLength(50)]
    public string? SwiftCode { get; set; }

    public bool IsDefault { get; set; }

    public bool RequiresApproval { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal InterestRate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [Required]
    public Guid CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    // Navigation Properties
    [InverseProperty("SourceAccount")]
    public ICollection<FundAllocation> SourceAllocations { get; set; } = new List<FundAllocation>();
    
    [InverseProperty("TargetAccount")]
    public ICollection<FundAllocation> TargetAllocations { get; set; } = new List<FundAllocation>();
    
    [InverseProperty("Account")]
    public ICollection<TreasuryTransaction> Transactions { get; set; } = new List<TreasuryTransaction>();
    
    [InverseProperty("RelatedAccount")]
    public ICollection<TreasuryTransaction> RelatedTransactions { get; set; } = new List<TreasuryTransaction>();
    
    public ICollection<TreasuryBalance> BalanceHistory { get; set; } = new List<TreasuryBalance>();
    public ICollection<ReserveRequirement> ReserveRequirements { get; set; } = new List<ReserveRequirement>();
}
