using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Models.Requests;

public class ValidateLimitRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Required]
    [StringLength(100)]
    public string LimitType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,8)")]
    public decimal Amount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    [StringLength(100)]
    public string? TransactionType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
