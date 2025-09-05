using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class FreezeWalletRequest
{
    [Required]
    public Guid WalletId { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    public Guid? FrozenBy { get; set; }

    public DateTime? FreezeUntil { get; set; }

    [StringLength(100)]
    public string? FreezeType { get; set; } // Temporary, Permanent, Compliance

    [StringLength(1000)]
    public string? AdditionalNotes { get; set; }
}
