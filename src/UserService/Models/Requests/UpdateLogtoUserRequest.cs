using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class UpdateLogtoUserRequest
{
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? Username { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    [StringLength(1000)]
    public string? CustomData { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    [StringLength(50)]
    public string? Locale { get; set; }

    [StringLength(50)]
    public string? TimeZone { get; set; }

    public bool? IsActive { get; set; }
}
