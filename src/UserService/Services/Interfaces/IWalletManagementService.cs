using UserService.Models.Infrastructure;
using UserService.Models.Responses;

namespace UserService.Services.Interfaces;

public interface IWalletManagementService
{
    // Existing methods
    Task<UserWalletResponse> CreateMultiSigWalletAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserWalletResponse> GetWalletAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserWalletResponse?> GetWalletByAddressAsync(string walletAddress, CancellationToken cancellationToken = default);
    Task<bool> VerifyWalletAsync(Guid userId, string signature, string message, CancellationToken cancellationToken = default);
    Task<UserWalletResponse> UpdateWalletBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateWalletAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> SyncWalletWithInfrastructureAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TokenBalance>> GetTokenBalancesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<object> GetWalletTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    
    // Missing methods needed by controller
    Task<UserWalletResponse> CreateWalletAsync(Models.Requests.CreateWalletRequest request, CancellationToken cancellationToken = default);
    Task<UserWalletResponse> UpdateWalletAsync(Guid userId, Models.Requests.UpdateWalletRequest request, CancellationToken cancellationToken = default);
    Task<WalletBalanceResponse> GetWalletBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WalletBalanceResponse> UpdateWalletBalanceAsync(Guid userId, Models.Requests.UpdateWalletBalanceRequest request, CancellationToken cancellationToken = default);
    Task<bool> FreezeWalletAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task<bool> UnfreezeWalletAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateWalletAddressAsync(string address, CancellationToken cancellationToken = default);
}
