using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class ProcessRefundRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public decimal RefundAmount { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ProcessedBy { get; set; } = string.Empty;

    public Dictionary<string, object>? Metadata { get; set; }
}
