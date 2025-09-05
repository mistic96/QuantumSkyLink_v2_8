using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;

namespace InfrastructureService.Services.Interfaces;

public interface IInfrastructureService
{
    // Wallet Management
    Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request);
    Task<WalletResponse> GetWalletAsync(Guid walletId);
    Task<WalletResponse> GetWalletByAddressAsync(string address);
    Task<List<WalletResponse>> GetUserWalletsAsync(Guid userId);
    Task<WalletResponse> UpdateWalletStatusAsync(Guid walletId, string status);
    Task<bool> DeleteWalletAsync(Guid walletId);

    // Wallet Signer Management
    Task<WalletSignerResponse> AddWalletSignerAsync(AddWalletSignerRequest request);
    Task<List<WalletSignerResponse>> GetWalletSignersAsync(Guid walletId);
    Task<WalletSignerResponse> UpdateSignerStatusAsync(Guid signerId, string status);
    Task<bool> RemoveWalletSignerAsync(Guid signerId);

    // Balance Management
    Task<List<WalletBalanceResponse>> GetWalletBalancesAsync(Guid walletId);
    Task<WalletBalanceResponse> GetWalletBalanceAsync(Guid walletId, string tokenSymbol);
    Task<WalletBalanceResponse> UpdateWalletBalanceAsync(Guid walletId, string tokenSymbol, decimal balance);
    Task SyncWalletBalancesAsync(Guid walletId);

    // Transaction Management
    Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
    Task<TransactionResponse> GetTransactionAsync(Guid transactionId);
    Task<TransactionResponse> GetTransactionByHashAsync(string hash);
    Task<List<TransactionResponse>> GetWalletTransactionsAsync(Guid walletId, int page = 1, int pageSize = 50);
    Task<List<TransactionResponse>> GetUserTransactionsAsync(Guid userId, int page = 1, int pageSize = 50);
    Task<List<TransactionResponse>> GetPendingTransactionsAsync(Guid walletId);

    // Transaction Signing
    Task<TransactionSignatureResponse> SignTransactionAsync(SignTransactionRequest request);
    Task<TransactionSignatureResponse> RejectTransactionAsync(RejectTransactionRequest request);
    Task<List<TransactionSignatureResponse>> GetTransactionSignaturesAsync(Guid transactionId);
    Task<TransactionResponse> BroadcastTransactionAsync(Guid transactionId);

    // Broadcast a signed raw transaction directly to the specified network (EVM/Tron/etc).
    // Accepts the signed transaction payload and returns the broadcast result.
    Task<BroadcastResponse> BroadcastSignedTransactionAsync(BroadcastSignedTransactionRequest request);

    // Blockchain Operations
    Task<string> GenerateWalletAddressAsync(string network, string walletType);
    Task<decimal> GetNetworkBalanceAsync(string address, string network, string? tokenAddress = null);
    Task<string> EstimateGasAsync(string fromAddress, string toAddress, decimal amount, string? data = null);
    Task<decimal> GetCurrentGasPriceAsync(string network);
    Task<long> GetNextNonceAsync(string address, string network);

    // Statistics and Monitoring
    Task<WalletStatsResponse> GetWalletStatsAsync(Guid userId);
    Task<List<WalletNetworkStatsResponse>> GetNetworkStatsAsync();
    Task<bool> ValidateAddressAsync(string address, string network);
    Task<bool> IsContractAddressAsync(string address, string network);

    // Security and Validation
    Task<bool> ValidateTransactionAsync(Guid transactionId);
    Task<bool> ValidateSignatureAsync(Guid transactionId, Guid signerId, string signature);
    Task<bool> CanUserSignTransactionAsync(Guid userId, Guid transactionId);
    Task<bool> IsTransactionReadyToBroadcastAsync(Guid transactionId);
}
