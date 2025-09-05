using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class DefaultLimitResponse
{
    public Guid Id { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal DefaultAmount { get; set; }

    [StringLength(50)]
    public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly, Yearly

    [StringLength(100)]
    public string Currency { get; set; } = "USD";

    [StringLength(100)]
    public string AccountType { get; set; } = string.Empty; // Individual, Business, Premium, etc.

    [StringLength(50)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MinimumAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? MaximumAmount { get; set; }

    public int Priority { get; set; } = 0;

    [StringLength(1000)]
    public string? Conditions { get; set; }

    public bool RequiresApproval { get; set; } = false;

    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }
}
