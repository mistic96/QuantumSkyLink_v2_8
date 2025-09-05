using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class CollectFeeRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CalculationResultId { get; set; }

    [Required]
    [StringLength(255)]
    public string PaymentMethod { get; set; } = string.Empty;

    [StringLength(255)]
    public string? PaymentReference { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}
