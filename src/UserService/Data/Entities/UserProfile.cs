using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Data.Entities;

[Table("UserProfiles")]
public class UserProfile
{
    [Key]
    [ForeignKey("User")]
    public Guid UserId { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(255)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    [MaxLength(50)]
    public string? Occupation { get; set; }

    [MaxLength(100)]
    public string? Company { get; set; }

    [MaxLength(255)]
    public string? Website { get; set; }

    [MaxLength(50)]
    public string? TimeZone { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; } = "en";

    [MaxLength(10)]
    public string? PreferredCurrency { get; set; } = "USD";

    public bool IsProfileComplete { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public virtual User User { get; set; } = null!;
}
