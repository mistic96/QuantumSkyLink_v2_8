using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Requests;

public class UpdateLogtoUserProfileRequest
{
    [StringLength(500)]
    public string? Bio { get; set; }
    
    [StringLength(500)]
    public string? Avatar { get; set; }
    
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
    
    [Url]
    [StringLength(200)]
    public string? Website { get; set; }
    
    [StringLength(50)]
    public string? Locale { get; set; }
    
    [StringLength(50)]
    public string? TimeZone { get; set; }
    
    [StringLength(10)]
    public string? PreferredCurrency { get; set; }
    
    [StringLength(1000)]
    public string? CustomData { get; set; }
}
