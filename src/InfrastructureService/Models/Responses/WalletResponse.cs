namespace InfrastructureService.Models.Responses;

public class WalletResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }
    public int RequiredSignatures { get; set; }
    public int TotalSigners { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    public List<WalletSignerResponse> Signers { get; set; } = new();
    public List<WalletBalanceResponse> Balances { get; set; } = new();
}

public class WalletSignerResponse
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid UserId { get; set; }
    public string SignerAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int SigningWeight { get; set; }
    public string? PublicKey { get; set; }
    public string? Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastSignedAt { get; set; }
}

public class WalletBalanceResponse
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string? TokenAddress { get; set; }
    public string TokenName { get; set; } = string.Empty;
    public int TokenDecimals { get; set; }
    public decimal Balance { get; set; }
    public decimal LockedBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal? UsdValue { get; set; }
    public decimal? TokenPrice { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? LastSyncedAt { get; set; }
    public string? Metadata { get; set; }
}

public class TransactionResponse
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string? Hash { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TokenSymbol { get; set; } = string.Empty;
    public string? TokenAddress { get; set; }
    public decimal GasPrice { get; set; }
    public long GasLimit { get; set; }
    public long? GasUsed { get; set; }
    public string Status { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public long? BlockNumber { get; set; }
    public int? TransactionIndex { get; set; }
    public long Nonce { get; set; }
    public string? Data { get; set; }
    public string? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
    public int RequiredSignatures { get; set; }
    public int CurrentSignatures { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? BroadcastAt { get; set; }
    public List<TransactionSignatureResponse> Signatures { get; set; } = new();
}

public class TransactionSignatureResponse
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid SignerId { get; set; }
    public string SignerAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Signature { get; set; }
    public string? R { get; set; }
    public string? S { get; set; }
    public int? V { get; set; }
    public string? SignatureData { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
}

public class WalletStatsResponse
{
    public int TotalWallets { get; set; }
    public int ActiveWallets { get; set; }
    public int MultiSigWallets { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal TotalUsdValue { get; set; }
    public int PendingTransactions { get; set; }
    public int CompletedTransactions { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class WalletNetworkStatsResponse
{
    public string Network { get; set; } = string.Empty;
    public int WalletCount { get; set; }
    public decimal TotalBalance { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageGasPrice { get; set; }
    public DateTime LastUpdated { get; set; }
}
