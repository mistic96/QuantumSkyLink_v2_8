using System.ComponentModel.DataAnnotations;

namespace SecurityService.Models.Requests;

public class CreateMultiSigRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string OperationType { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }

    [StringLength(100)]
    public string? Currency { get; set; }

    [StringLength(500)]
    public string? DestinationAddress { get; set; }

    public object OperationData { get; set; } = new();

    [Range(1, 10, ErrorMessage = "Required signatures must be between 1 and 10")]
    public int RequiredSignatures { get; set; } = 2;

    public DateTime? ExpiresAt { get; set; }
}
