using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class DistributeFeesRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    [StringLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [StringLength(255)]
    public string? ProcessedBy { get; set; }

    public bool ForceDistribution { get; set; } = false; // Override validation checks

    public Dictionary<string, object>? Metadata { get; set; }
}
