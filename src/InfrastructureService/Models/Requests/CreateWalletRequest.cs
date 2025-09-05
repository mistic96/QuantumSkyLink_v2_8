using System.ComponentModel.DataAnnotations;

namespace InfrastructureService.Models.Requests;

public class CreateWalletRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string WalletType { get; set; } = "Standard"; // Standard, MultiSig, Cold, Hot

    [Required]
    [StringLength(10)]
    public string Network { get; set; } = "Ethereum"; // Ethereum, Polygon, BSC

    public int RequiredSignatures { get; set; } = 1;

    public List<Guid> SignerUserIds { get; set; } = new();

    [StringLength(2000)]
    public string? Metadata { get; set; }
}

public class AddWalletSignerRequest
{
    [Required]
    public Guid WalletId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(42)]
    public string SignerAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Signer"; // Owner, Signer, Observer

    public int SigningWeight { get; set; } = 1;

    [StringLength(500)]
    public string? PublicKey { get; set; }

    [StringLength(1000)]
    public string? Permissions { get; set; }
}

public class CreateTransactionRequest
{
    [Required]
    public Guid WalletId { get; set; }

    [Required]
    [StringLength(42)]
    public string ToAddress { get; set; } = string.Empty;

    [Required]
    [Range(0.000000001, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string TokenSymbol { get; set; } = "ETH";

    [StringLength(42)]
    public string? TokenAddress { get; set; }

    [Range(1, long.MaxValue)]
    public decimal GasPrice { get; set; } = 20; // Gwei

    [Range(21000, 10000000)]
    public long GasLimit { get; set; } = 21000;

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = "Transfer";

    [StringLength(2000)]
    public string? Data { get; set; }

    [StringLength(1000)]
    public string? Metadata { get; set; }
}

public class SignTransactionRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    public Guid SignerId { get; set; }

    [Required]
    [StringLength(132)]
    public string Signature { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? SignatureData { get; set; }
}

public class RejectTransactionRequest
{
    [Required]
    public Guid TransactionId { get; set; }

    [Required]
    public Guid SignerId { get; set; }

    [Required]
    [StringLength(500)]
    public string RejectionReason { get; set; } = string.Empty;
}
