using System.ComponentModel.DataAnnotations;

namespace SecurityService.Models.Requests;

public class CreateSecurityPolicyRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string PolicyType { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public object Configuration { get; set; } = new();

    public bool IsRequired { get; set; } = false;
}
