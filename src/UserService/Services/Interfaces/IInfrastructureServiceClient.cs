using Refit;
using UserService.Models.Infrastructure;

namespace UserService.Services.Interfaces;

public interface IInfrastructureServiceClient
{
    [Post("/api/wallets")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> CreateWalletAsync(
        [Body] CreateWalletRequest request, 
        CancellationToken cancellationToken = default);

    [Get("/api/wallets/{walletAddress}")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> GetWalletAsync(
        string walletAddress, 
        CancellationToken cancellationToken = default);

    [Get("/api/wallets/user/{userId}")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> GetWalletByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    [Put("/api/wallets/{walletAddress}")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> UpdateWalletAsync(
        string walletAddress,
        [Body] UpdateWalletRequest request, 
        CancellationToken cancellationToken = default);

    [Post("/api/wallets/{walletAddress}/verify")]
    Task<ApiResponse<InfrastructureApiResponse<WalletVerificationResponse>>> VerifyWalletAsync(
        string walletAddress,
        [Body] VerifyWalletRequest request, 
        CancellationToken cancellationToken = default);

    [Post("/api/wallets/{walletAddress}/balance")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> UpdateWalletBalanceAsync(
        string walletAddress,
        [Body] WalletBalanceRequest request, 
        CancellationToken cancellationToken = default);

    [Get("/api/wallets/{walletAddress}/balance")]
    Task<ApiResponse<InfrastructureApiResponse<WalletResponse>>> GetWalletBalanceAsync(
        string walletAddress,
        [Query] string network = "Ethereum",
        CancellationToken cancellationToken = default);

    [Delete("/api/wallets/{walletAddress}")]
    Task<ApiResponse<InfrastructureApiResponse<bool>>> DeactivateWalletAsync(
        string walletAddress, 
        CancellationToken cancellationToken = default);

    [Get("/api/wallets/{walletAddress}/transactions")]
    Task<ApiResponse<InfrastructureApiResponse<object>>> GetWalletTransactionsAsync(
        string walletAddress,
        [Query] int page = 1,
        [Query] int pageSize = 10,
        CancellationToken cancellationToken = default);

    [Get("/api/health")]
    Task<ApiResponse<object>> GetHealthAsync(CancellationToken cancellationToken = default);
}
