using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeeService.Data.Entities;

[Table("Settlements")]
public class Settlement
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(4000)]
    public string DistributionIds { get; set; } = string.Empty; // JSON array of distribution IDs

    [Required]
    [StringLength(100)]
    public string SettlementMethod { get; set; } = string.Empty; // "Blockchain", "BankTransfer", "Internal"

    [StringLength(255)]
    public string? SettlementReference { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Failed"

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [Required]
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(1000)]
    public string? FailureReason { get; set; }

    public string? Metadata { get; set; } // JSON for additional data

    // Navigation properties
    public ICollection<FeeDistribution>? Distributions { get; set; }
}
