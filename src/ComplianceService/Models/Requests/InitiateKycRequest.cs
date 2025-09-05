using System.ComponentModel.DataAnnotations;

namespace ComplianceService.Models.Requests;

public class InitiateKycRequest
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [Required]
    [StringLength(50)]
    public string KycLevel { get; set; } = "Basic"; // Basic, Enhanced, Premium

    [Required]
    [StringLength(100)]
    public string TriggerReason { get; set; } = string.Empty; // RoleUpgrade, HighValueTransaction, Periodic, Manual

    [StringLength(500)]
    public string? Comments { get; set; }

    [Required]
    [StringLength(100)]
    public string CorrelationId { get; set; } = string.Empty;
}
