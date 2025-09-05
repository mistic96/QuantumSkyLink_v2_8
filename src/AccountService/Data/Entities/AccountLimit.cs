using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Data.Entities;

[Table("AccountLimits")]
[Index(nameof(AccountId), nameof(LimitType), IsUnique = true)]
[Index(nameof(IsActive), IsUnique = false)]
public class AccountLimit
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey("Account")]
    public Guid AccountId { get; set; }

    [Required]
    public LimitType LimitType { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal UsedAmount { get; set; } = 0;

    [Required]
    public LimitPeriod Period { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? SetBy { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

    public DateTime? EffectiveTo { get; set; }

    public DateTime LastResetAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Account Account { get; set; } = null!;
}

public enum LimitType
{
    DailyWithdrawal = 1,
    DailyDeposit = 2,
    DailyTransfer = 3,
    WeeklyWithdrawal = 4,
    WeeklyDeposit = 5,
    WeeklyTransfer = 6,
    MonthlyWithdrawal = 7,
    MonthlyDeposit = 8,
    MonthlyTransfer = 9,
    SingleTransaction = 10,
    AccountBalance = 11
}

public enum LimitPeriod
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
    PerTransaction = 5,
    Lifetime = 6
}
