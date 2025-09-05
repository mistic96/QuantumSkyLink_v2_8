using System.ComponentModel.DataAnnotations;

namespace TreasuryService.Models.Requests;

public class CreateTreasuryAccountRequest
{
    [Required]
    [MaxLength(100)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string AccountType { get; set; } = string.Empty; // Operating, Reserve, Investment, Emergency

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinimumBalance { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaximumBalance { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? ExternalAccountId { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? RoutingNumber { get; set; }

    [MaxLength(50)]
    public string? SwiftCode { get; set; }

    public bool IsDefault { get; set; }

    public bool RequiresApproval { get; set; }

    [Range(0, 1)]
    public decimal InterestRate { get; set; }

    public Guid? CreatedBy { get; set; }
}
