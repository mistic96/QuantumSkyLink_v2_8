namespace TokenService.Models.Responses;

public class QuantumLedgerAccountCreationResponse
{
    public bool Success { get; set; }
    public string? Address { get; set; }
    public QuantumLedgerAccount? Account { get; set; }
    public QuantumLedgerSubstitutionKey? SubstitutionKey { get; set; }
    public string? ClassicKeyId { get; set; }
    public string? QuantumKeyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public string? ErrorMessage { get; set; }
}

public class QuantumLedgerAccount
{
    public Guid AccountId { get; set; }
    public string ExternalOwnerId { get; set; } = string.Empty;
    public string? VendorSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class QuantumLedgerSubstitutionKey
{
    public string SubstitutionKeyId { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string LinkedAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class QuantumLedgerTransactionResponse
{
    public Guid TransactionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid FromAccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? BlockHash { get; set; }
    public decimal TransactionFee { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class QuantumLedgerBalanceResponse
{
    public Guid AccountId { get; set; }
    public List<QuantumLedgerBalance> Balances { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public decimal TotalValueUSD { get; set; }
    public decimal Balance { get; set; }
}

public class QuantumLedgerBalance
{
    public string Currency { get; set; } = string.Empty;
    public decimal Available { get; set; }
    public decimal Pending { get; set; }
    public decimal Total { get; set; }
}

public class QuantumLedgerErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string ErrorDescription { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
