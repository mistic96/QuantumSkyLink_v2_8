using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class UpdateWalletRequest
{
    [Required]
    public Guid WalletId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(200)]
    public string? Label { get; set; }

    [StringLength(100)]
    public string? WalletType { get; set; }
}
