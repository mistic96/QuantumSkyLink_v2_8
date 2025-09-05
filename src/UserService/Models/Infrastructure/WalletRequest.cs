namespace UserService.Models.Infrastructure;

public class CreateWalletRequest
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string WalletType { get; set; } = "MultiSig";
    public int RequiredSignatures { get; set; } = 2;
    public int TotalSigners { get; set; } = 3;
    public string Network { get; set; } = "Ethereum";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UpdateWalletRequest
{
    public string WalletAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class VerifyWalletRequest
{
    public string WalletAddress { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}

public class WalletBalanceRequest
{
    public string WalletAddress { get; set; } = string.Empty;
    public string Network { get; set; } = "Ethereum";
    public string[] TokenAddresses { get; set; } = Array.Empty<string>();
}
