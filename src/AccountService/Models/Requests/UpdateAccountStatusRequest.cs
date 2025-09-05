using System.ComponentModel.DataAnnotations;
using AccountService.Data.Entities;

namespace AccountService.Models.Requests;

public class UpdateAccountStatusRequest
{
    [Required]
    public AccountStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }
}
