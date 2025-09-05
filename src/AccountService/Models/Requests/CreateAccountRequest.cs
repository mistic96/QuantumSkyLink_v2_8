using System.ComponentModel.DataAnnotations;
using AccountService.Data.Entities;

namespace AccountService.Models.Requests;

public class CreateAccountRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public AccountType AccountType { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    [Range(0, double.MaxValue, ErrorMessage = "Daily limit must be positive")]
    public decimal? DailyLimit { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Monthly limit must be positive")]
    public decimal? MonthlyLimit { get; set; }

    public bool AutoVerify { get; set; } = false;

    public Dictionary<string, object>? Metadata { get; set; }
}
