using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class CreateAccountLimitRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Transaction, etc.

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    [Required]
    [StringLength(50)]
    public string Period { get; set; } = string.Empty; // Daily, Weekly, Monthly, PerTransaction

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Range(0, 100)]
    public int Priority { get; set; } = 0;

    public Guid? CreatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool RequiresApproval { get; set; } = false;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? WarningThreshold { get; set; }

    // Additional properties expected by service layer
    public Guid? SetBy { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }
}
