using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class UpdateWalletBalanceRequest
{
    [Required]
    public Guid WalletId { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty; // Credit, Debit

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public Guid? TransactionId { get; set; }
}
