using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class CreateWalletRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string WalletType { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [StringLength(200)]
    public string? Label { get; set; }
}
