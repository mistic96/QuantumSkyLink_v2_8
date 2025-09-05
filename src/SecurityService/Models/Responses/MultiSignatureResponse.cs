namespace SecurityService.Models.Responses;

public class MultiSignatureResponse
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid RequestedBy { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? DestinationAddress { get; set; }
    public object OperationData { get; set; } = new();
    public int RequiredSignatures { get; set; }
    public int CurrentSignatures { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public List<MultiSignatureApprovalResponse> Approvals { get; set; } = new();
}

public class MultiSignatureApprovalResponse
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ApprovedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public string SignatureMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}

public class MultiSignatureListResponse
{
    public List<MultiSignatureResponse> Requests { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
