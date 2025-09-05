using System.ComponentModel.DataAnnotations;

namespace TreasuryService.Models.Requests;

public class GetTreasuryAccountsRequest
{
    [MaxLength(50)]
    public string? AccountType { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    public bool? IsDefault { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    [MaxLength(50)]
    public string? SortBy { get; set; } = "CreatedAt";

    [MaxLength(10)]
    public string? SortDirection { get; set; } = "DESC";

    public Guid? CreatedBy { get; set; }
}
