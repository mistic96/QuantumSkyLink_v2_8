using System.ComponentModel.DataAnnotations;

namespace FeeService.Models.Requests;

public class EstimateFeeRequest
{
    [Required]
    [StringLength(100)]
    public string FeeType { get; set; } = string.Empty;

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

    public Guid? UserId { get; set; } // Optional for discount calculations

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
