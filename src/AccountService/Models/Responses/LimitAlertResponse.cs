using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitAlertResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [StringLength(50)]
    public string AlertType { get; set; } = string.Empty; // Warning, Critical, Exceeded

    [Column(TypeName = "decimal(18,8)")]
    public decimal LimitAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentUsage { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal ThresholdAmount { get; set; }

    public decimal UsagePercentage { get; set; }

    [StringLength(1000)]
    public string AlertMessage { get; set; } = string.Empty;

    [StringLength(500)]
    public string? RecommendedAction { get; set; }

    public DateTime AlertTriggeredAt { get; set; } = DateTime.UtcNow;

    public bool IsAcknowledged { get; set; } = false;

    public DateTime? AcknowledgedAt { get; set; }

    public Guid? AcknowledgedBy { get; set; }

    [StringLength(50)]
    public string Severity { get; set; } = string.Empty;

    public bool RequiresImmediateAction { get; set; } = false;

    [StringLength(1000)]
    public string? Message { get; set; }

    public DateTime AlertedAt { get; set; } = DateTime.UtcNow;
}
