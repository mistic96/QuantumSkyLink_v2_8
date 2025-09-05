using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data.Entities;

[Table("Accounts")]
[Index(nameof(UserId), nameof(AccountType), IsUnique = false)]
[Index(nameof(AccountNumber), IsUnique = true)]
[Index(nameof(Status), IsUnique = false)]
public class Account
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    public AccountType AccountType { get; set; }

    [Required]
    public AccountStatus Status { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal Balance { get; set; } = 0;

    [Column(TypeName = "decimal(18,8)")]
    public decimal DailyLimit { get; set; } = 10000; // Default $10,000 daily limit

    [Column(TypeName = "decimal(18,8)")]
    public decimal MonthlyLimit { get; set; } = 100000; // Default $100,000 monthly limit

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }

    public DateTime? LastTransactionAt { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
    public ICollection<AccountVerification> Verifications { get; set; } = new List<AccountVerification>();
    public ICollection<AccountLimit> Limits { get; set; } = new List<AccountLimit>();
}

public enum AccountType
{
    Individual = 1,
    Business = 2,
    Institutional = 3,
    Trading = 4,
    Savings = 5,
    Escrow = 6
}

public enum AccountStatus
{
    Pending = 1,
    Active = 2,
    Suspended = 3,
    Closed = 4,
    UnderReview = 5,
    Restricted = 6
}
