using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Responses;

public class LimitCheckResponse
{
    public bool CanProceed { get; set; }

    public bool IsWithinLimit { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal RequestedAmount { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal AvailableLimit { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal CurrentUsage { get; set; }

    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Message { get; set; }

    [StringLength(500)]
    public string? RecommendedAction { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public List<string> Warnings { get; set; } = new();
}
