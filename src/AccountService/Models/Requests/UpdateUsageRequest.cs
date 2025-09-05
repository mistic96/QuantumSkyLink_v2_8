using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class UpdateUsageRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal UsageAmount { get; set; }

    [Required]
    [StringLength(50)]
    public string Operation { get; set; } = string.Empty; // Add, Subtract, Set

    [StringLength(500)]
    public string? Reason { get; set; }

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    public Guid? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool BypassValidation { get; set; } = false;

    [StringLength(50)]
    public string? Period { get; set; }

    public DateTime? EffectiveDate { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Amount { get; set; }

    public Guid? TransactionId { get; set; }
}
