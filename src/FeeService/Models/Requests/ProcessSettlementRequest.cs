using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class ProcessSettlementRequest
{
    [Required]
    public IEnumerable<Guid> DistributionIds { get; set; } = new List<Guid>();

    [Required]
    [StringLength(100)]
    public string SettlementMethod { get; set; } = string.Empty; // "Blockchain", "BankTransfer", "Internal"

    [StringLength(255)]
    public string? SettlementReference { get; set; }

    [Required]
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}
