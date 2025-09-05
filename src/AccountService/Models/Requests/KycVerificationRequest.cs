using System.ComponentModel.DataAnnotations;

namespace AccountService.Models.Requests;

public class KycVerificationRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string VerificationType { get; set; } = string.Empty; // Identity, Address, Income, Business

    [StringLength(100)]
    public string? DocumentType { get; set; }

    [StringLength(100)]
    public string? DocumentNumber { get; set; }

    [StringLength(100)]
    public string? IssuingCountry { get; set; }

    [StringLength(100)]
    public string? IssuingAuthority { get; set; }

    public DateTime? DocumentExpiryDate { get; set; }

    [StringLength(2000)]
    public string? DocumentData { get; set; } // Base64 encoded or reference

    [StringLength(500)]
    public string? AdditionalInfo { get; set; }

    public Guid? RequestedBy { get; set; }

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

    [StringLength(1000)]
    public string? SpecialInstructions { get; set; }

    public bool RequiresManualReview { get; set; } = false;

    [StringLength(100)]
    public string? RegulatoryBasis { get; set; }
}
