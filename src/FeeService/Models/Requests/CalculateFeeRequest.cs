using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class CalculateFeeRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FeeType { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string ReferenceId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ReferenceType { get; set; } = string.Empty;

    [Required]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EntityType { get; set; }

    [StringLength(255)]
    public string? EntityId { get; set; }

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
