namespace TreasuryService.Models.Responses;

public class TreasuryAccountResponse
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ReservedBalance { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MaximumBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExternalAccountId { get; set; }
    public string? BankName { get; set; }
    public string? RoutingNumber { get; set; }
    public string? SwiftCode { get; set; }
    public bool IsDefault { get; set; }
    public bool RequiresApproval { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
