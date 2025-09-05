using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitHistoryResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Exceeded, Reset

    [Column(TypeName = "decimal(18,8)")]
    public decimal? PreviousAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? NewAmount { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    public Guid? ActionBy { get; set; }

    [StringLength(100)]
    public string? ActionByName { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(50)]
    public string? Period { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? UsageAtTime { get; set; }

    public decimal? UsagePercentageAtTime { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [StringLength(2000)]
    public string? AdditionalData { get; set; }

    public bool IsSystemGenerated { get; set; } = false;

    [StringLength(100)]
    public string? ChangeCategory { get; set; }

    public Guid? LimitId { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? LimitAmount { get; set; }

    public Guid? SetBy { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
