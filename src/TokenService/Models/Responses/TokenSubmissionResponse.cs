namespace TokenService.Models.Responses;

public class TokenSubmissionResponse
{
    public Guid SubmissionId { get; set; }
    public decimal AiComplianceScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();
    public DateTime SubmissionDate { get; set; }
    public string? AssetVerificationStatus { get; set; }
}

public class TokenCreationResponse
{
    public Guid TokenId { get; set; }
    public Guid QuantumLedgerAccountId { get; set; }
    public Guid QuantumLedgerTransactionId { get; set; }
    public string? QuantumLedgerSubstitutionKeyId { get; set; }
    public string? MultiChainAssetName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TokenTransferResponse
{
    public Guid TransferId { get; set; }
    public Guid QuantumLedgerTransactionId { get; set; }
    public string? ExternalTransactionHash { get; set; }
    public string? MultiChainTransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TransactionFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TokenBalanceResponse
{
    public Guid TokenId { get; set; }
    public string TokenName { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? LastSyncedWithQuantumLedger { get; set; }
}

public class TokenResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalSupply { get; set; }
    public int Decimals { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public Guid QuantumLedgerAccountId { get; set; }
    public string? QuantumLedgerSubstitutionKeyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public Dictionary<string, object>? AssetMetadata { get; set; }
    public bool CrossChainEnabled { get; set; }
    public string Network { get; set; } = string.Empty;
    public string? MultiChainAssetName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}

public class ComplianceScore
{
    public decimal OverallScore { get; set; }
    public decimal CommunityBenefitScore { get; set; }
    public decimal RegulatoryComplianceScore { get; set; }
    public decimal FraudRiskScore { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class AssetVerificationResult
{
    public bool IsVerified { get; set; }
    public string? VerificationId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationMethod { get; set; }
    public decimal ConfidenceScore { get; set; }
    public Dictionary<string, object>? VerificationDetails { get; set; }
    public string? ErrorMessage { get; set; }
}

public class OwnershipVerificationResult
{
    public bool IsOwner { get; set; }
    public string? VerificationMethod { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Dictionary<string, object>? VerificationDetails { get; set; }
    public string? ErrorMessage { get; set; }
}
