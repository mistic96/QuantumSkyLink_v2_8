using Refit;

namespace AccountService.Services.Interfaces;

public interface IInfrastructureServiceClient
{
    [Post("/api/wallets")]
    Task<ApiResponse<WalletDto>> CreateWalletAsync([Body] CreateWalletRequest request, CancellationToken cancellationToken = default);

    [Get("/api/wallets/{walletId}")]
    Task<ApiResponse<WalletDto>> GetWalletAsync(Guid walletId, CancellationToken cancellationToken = default);

    [Get("/api/wallets/account/{accountId}")]
    Task<ApiResponse<WalletDto>> GetWalletByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    [Get("/api/wallets/{walletId}/balance")]
    Task<ApiResponse<WalletBalanceDto>> GetWalletBalanceAsync(Guid walletId, CancellationToken cancellationToken = default);

    [Post("/api/wallets/{walletId}/verify")]
    Task<ApiResponse<WalletVerificationDto>> VerifyWalletAsync(Guid walletId, CancellationToken cancellationToken = default);

    [Put("/api/wallets/{walletId}/status")]
    Task<ApiResponse<WalletDto>> UpdateWalletStatusAsync(Guid walletId, [Body] UpdateWalletStatusRequest request, CancellationToken cancellationToken = default);
}

// DTOs for InfrastructureService integration
public class CreateWalletRequest
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string WalletType { get; set; } = "MultiSig";
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateWalletStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public string WalletType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class WalletBalanceDto
{
    public Guid WalletId { get; set; }
    public string WalletAddress { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, decimal>? TokenBalances { get; set; }
}

public class WalletVerificationDto
{
    public Guid WalletId { get; set; }
    public bool IsVerified { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationHash { get; set; }
}
