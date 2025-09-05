using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class LogtoUserProfileResponse
{
    public string Id { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? Username { get; set; }
    
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Avatar { get; set; }
    
    [StringLength(500)]
    public string? Bio { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(20)]
    public string? Gender { get; set; }
    
    [StringLength(100)]
    public string? Occupation { get; set; }
    
    [StringLength(100)]
    public string? Company { get; set; }
    
    [StringLength(200)]
    public string? Website { get; set; }
    
    [StringLength(50)]
    public string? Locale { get; set; }
    
    [StringLength(50)]
    public string? TimeZone { get; set; }
    
    [StringLength(10)]
    public string? PreferredCurrency { get; set; }
    
    public bool EmailVerified { get; set; }
    
    public bool PhoneVerified { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool IsProfileComplete { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? LastLoginAt { get; set; }
}
