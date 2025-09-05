namespace InfrastructureService.Services.Interfaces;

public interface IBlockchainService
{
    // Wallet Generation and Management
    Task<(string address, string privateKey, string publicKey)> GenerateWalletAsync(string network);
    Task<(string address, string privateKey, string publicKey)> GenerateHdWalletAsync(string network, string mnemonic, int accountIndex = 0);
    Task<string> GetPublicKeyFromPrivateKeyAsync(string privateKey);
    Task<string> GetAddressFromPrivateKeyAsync(string privateKey);

    // Balance Operations
    Task<decimal> GetEthBalanceAsync(string address, string network);
    Task<decimal> GetTokenBalanceAsync(string address, string tokenAddress, string network);
    Task<decimal> GetTokenBalanceWithDecimalsAsync(string address, string tokenAddress, int decimals, string network);

    // Transaction Operations
    Task<string> SendEthTransactionAsync(string fromPrivateKey, string toAddress, decimal amount, decimal gasPrice, long gasLimit, string network);
    Task<string> SendTokenTransactionAsync(string fromPrivateKey, string toAddress, string tokenAddress, decimal amount, decimal gasPrice, long gasLimit, string network);
    Task<string> SendRawTransactionAsync(string signedTransaction, string network);

    // Gas Estimation and Pricing
    Task<long> EstimateGasAsync(string fromAddress, string toAddress, decimal amount, string? data, string network);
    Task<long> EstimateTokenGasAsync(string fromAddress, string toAddress, string tokenAddress, decimal amount, string network);
    Task<decimal> GetCurrentGasPriceAsync(string network);
    Task<decimal> GetFastGasPriceAsync(string network);

    // Transaction Information
    Task<long> GetTransactionCountAsync(string address, string network);
    Task<(bool isConfirmed, long? blockNumber, string? status)> GetTransactionStatusAsync(string transactionHash, string network);
    Task<(decimal gasUsed, bool success, string? errorMessage)> GetTransactionReceiptAsync(string transactionHash, string network);

    // Network Information
    Task<long> GetLatestBlockNumberAsync(string network);
    Task<bool> IsValidAddressAsync(string address);
    Task<bool> IsContractAddressAsync(string address, string network);

    // Multi-Signature Operations
    Task<string> CreateMultiSigTransactionAsync(string[] signerAddresses, int requiredSignatures, string toAddress, decimal amount, string network);
    Task<string> SignMultiSigTransactionAsync(string privateKey, string transactionData);
    Task<bool> VerifySignatureAsync(string message, string signature, string signerAddress);

    // Token Information
    Task<(string name, string symbol, int decimals)> GetTokenInfoAsync(string tokenAddress, string network);
    Task<decimal> GetTokenTotalSupplyAsync(string tokenAddress, string network);

    // Utility Functions
    Task<string> ConvertWeiToEthAsync(decimal wei);
    Task<decimal> ConvertEthToWeiAsync(decimal eth);
    Task<decimal> ConvertTokenAmountAsync(decimal amount, int fromDecimals, int toDecimals);
    Task<bool> ValidateTransactionDataAsync(string data);
}
