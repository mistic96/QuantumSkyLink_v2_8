namespace SecurityService.Models.Responses;

public class SecurityPolicyResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PolicyType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object Configuration { get; set; } = new();
    public bool IsActive { get; set; }
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public class SecurityPolicyListResponse
{
    public List<SecurityPolicyResponse> Policies { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
