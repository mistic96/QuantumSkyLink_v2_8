namespace FeeService.Models.Responses;

public class ReceiptResponse
{
    public Guid TransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime GeneratedAt { get; set; }
    public object? TransactionDetails { get; set; }
    public object? FeeBreakdown { get; set; }
}
