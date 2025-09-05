namespace UserService.Models.Infrastructure;

public class WalletResponse
{
    public string WalletAddress { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public int RequiredSignatures { get; set; }
    public int TotalSigners { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public decimal Balance { get; set; }
    public string BalanceCurrency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<TokenBalance> TokenBalances { get; set; } = new();
}

public class TokenBalance
{
    public string TokenAddress { get; set; } = string.Empty;
    public string TokenSymbol { get; set; } = string.Empty;
    public string TokenName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int Decimals { get; set; }
    public decimal UsdValue { get; set; }
}

public class WalletVerificationResponse
{
    public bool IsValid { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public Dictionary<string, object> VerificationData { get; set; } = new();
}

public class InfrastructureApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
