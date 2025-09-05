using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class UpdateAccountLimitRequest
{
    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; }

    [StringLength(50)]
    public string? Period { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Range(0, 100)]
    public int? Priority { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool? RequiresApproval { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? WarningThreshold { get; set; }

    // Additional properties expected by service layer
    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime? EffectiveTo { get; set; }
}
