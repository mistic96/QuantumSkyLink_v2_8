using System.ComponentModel.DataAnnotations;

namespace SecurityService.Models.Requests;

public class ApproveMultiSigRequest
{
    [Required]
    public Guid RequestId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Approved, Rejected

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Comments { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string SignatureData { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SignatureMethod { get; set; } = "ECDSA";
}
