using Microsoft.EntityFrameworkCore;
using InfrastructureService.Data;
using InfrastructureService.Data.Entities;
using InfrastructureService.Models.Requests;
using InfrastructureService.Models.Responses;
using InfrastructureService.Services.Interfaces;
using Mapster;
using System.Security.Claims;

namespace InfrastructureService.Services;

public class InfrastructureService : IInfrastructureService
{
    private readonly InfrastructureDbContext _context;
    private readonly IBlockchainService _blockchainService;
    private readonly IUserServiceClient _userServiceClient;
    private readonly INotificationServiceClient _notificationServiceClient;
    private readonly ILogger<InfrastructureService> _logger;

    public InfrastructureService(
        InfrastructureDbContext context,
        IBlockchainService blockchainService,
        IUserServiceClient userServiceClient,
        INotificationServiceClient notificationServiceClient,
        ILogger<InfrastructureService> logger)
    {
        _context = context;
        _blockchainService = blockchainService;
        _userServiceClient = userServiceClient;
        _notificationServiceClient = notificationServiceClient;
        _logger = logger;
    }

    // Wallet Management
    public async Task<WalletResponse> CreateWalletAsync(CreateWalletRequest request)
    {
        _logger.LogInformation("Creating wallet for user {UserId} of type {WalletType}", request.UserId, request.WalletType);

        try
        {
            // Verify user exists
            var userResponse = await _userServiceClient.UserExistsAsync(request.UserId);
            if (!userResponse.IsSuccessStatusCode || !userResponse.Content)
            {
                throw new ArgumentException($"User {request.UserId} does not exist");
            }

            // Generate wallet address and keys
            var (address, privateKey, publicKey) = await _blockchainService.GenerateWalletAsync(request.Network);

            // Create wallet entity
            var wallet = new Wallet
            {
                UserId = request.UserId,
                Address = address,
                WalletType = request.WalletType,
                Network = request.Network,
                RequiredSignatures = request.RequiredSignatures,
                TotalSigners = request.SignerUserIds.Count + 1, // Include owner
                EncryptedPrivateKey = privateKey, // TODO: Implement proper encryption
                PublicKey = publicKey,
                Metadata = request.Metadata
            };

            _context.Wallets.Add(wallet);

            // Add owner as first signer
            var ownerSigner = new WalletSigner
            {
                WalletId = wallet.Id,
                UserId = request.UserId,
                SignerAddress = address,
                Role = "Owner",
                SigningWeight = 1,
                PublicKey = publicKey
            };

            _context.WalletSigners.Add(ownerSigner);

            // Add additional signers if specified
            foreach (var signerUserId in request.SignerUserIds)
            {
                var signerUserResponse = await _userServiceClient.UserExistsAsync(signerUserId);
                if (signerUserResponse.IsSuccessStatusCode && signerUserResponse.Content)
                {
                    var signer = new WalletSigner
                    {
                        WalletId = wallet.Id,
                        UserId = signerUserId,
                        SignerAddress = address, // For now, same address
                        Role = "Signer",
                        SigningWeight = 1
                    };

                    _context.WalletSigners.Add(signer);
                }
            }

            // Create initial ETH balance record
            var ethBalance = new WalletBalance
            {
                WalletId = wallet.Id,
                TokenSymbol = "ETH",
                TokenName = "Ethereum",
                TokenDecimals = 18,
                Balance = 0,
                LockedBalance = 0,
                AvailableBalance = 0
            };

            _context.WalletBalances.Add(ethBalance);

            await _context.SaveChangesAsync();

            // Update user service with wallet information
            try
            {
                await _userServiceClient.CreateUserWalletAsync(request.UserId, new CreateUserWalletRequest
                {
                    WalletAddress = address,
                    WalletType = request.WalletType,
                    Network = request.Network
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update user service with wallet information for user {UserId}", request.UserId);
            }

            // Send notification
            try
            {
                await _notificationServiceClient.SendWalletNotificationAsync(new WalletNotificationRequest
                {
                    UserId = request.UserId,
                    NotificationType = "WalletCreated",
                    Title = "Wallet Created Successfully",
                    Message = $"Your {request.WalletType} wallet has been created on {request.Network}",
                    WalletAddress = address,
                    Network = request.Network
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send wallet creation notification for user {UserId}", request.UserId);
            }

            _logger.LogInformation("Wallet created successfully for user {UserId} with address {Address}", request.UserId, address);

            return await GetWalletAsync(wallet.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<WalletResponse> GetWalletAsync(Guid walletId)
    {
        var wallet = await _context.Wallets
            .Include(w => w.Signers)
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Id == walletId);

        if (wallet == null)
        {
            throw new ArgumentException($"Wallet {walletId} not found");
        }

        return wallet.Adapt<WalletResponse>();
    }

    public async Task<WalletResponse> GetWalletByAddressAsync(string address)
    {
        var wallet = await _context.Wallets
            .Include(w => w.Signers)
            .Include(w => w.Balances)
            .FirstOrDefaultAsync(w => w.Address == address);

        if (wallet == null)
        {
            throw new ArgumentException($"Wallet with address {address} not found");
        }

        return wallet.Adapt<WalletResponse>();
    }

    public async Task<List<WalletResponse>> GetUserWalletsAsync(Guid userId)
    {
        var wallets = await _context.Wallets
            .Include(w => w.Signers)
            .Include(w => w.Balances)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        return wallets.Adapt<List<WalletResponse>>();
    }

    public async Task<WalletResponse> UpdateWalletStatusAsync(Guid walletId, string status)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);
        if (wallet == null)
        {
            throw new ArgumentException($"Wallet {walletId} not found");
        }

        wallet.Status = status;
        wallet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetWalletAsync(walletId);
    }

    public async Task<bool> DeleteWalletAsync(Guid walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);
        if (wallet == null)
        {
            return false;
        }

        wallet.Status = "Archived";
        wallet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Wallet Signer Management
    public async Task<WalletSignerResponse> AddWalletSignerAsync(AddWalletSignerRequest request)
    {
        var wallet = await _context.Wallets.FindAsync(request.WalletId);
        if (wallet == null)
        {
            throw new ArgumentException($"Wallet {request.WalletId} not found");
        }

        var existingSigner = await _context.WalletSigners
            .FirstOrDefaultAsync(s => s.WalletId == request.WalletId && s.UserId == request.UserId);

        if (existingSigner != null)
        {
            throw new InvalidOperationException($"User {request.UserId} is already a signer for wallet {request.WalletId}");
        }

        var signer = new WalletSigner
        {
            WalletId = request.WalletId,
            UserId = request.UserId,
            SignerAddress = request.SignerAddress,
            Role = request.Role,
            SigningWeight = request.SigningWeight,
            PublicKey = request.PublicKey,
            Permissions = request.Permissions
        };

        _context.WalletSigners.Add(signer);

        // Update wallet total signers count
        wallet.TotalSigners = await _context.WalletSigners.CountAsync(s => s.WalletId == request.WalletId && s.Status == "Active") + 1;
        wallet.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return signer.Adapt<WalletSignerResponse>();
    }

    public async Task<List<WalletSignerResponse>> GetWalletSignersAsync(Guid walletId)
    {
        var signers = await _context.WalletSigners
            .Where(s => s.WalletId == walletId)
            .ToListAsync();

        return signers.Adapt<List<WalletSignerResponse>>();
    }

    public async Task<WalletSignerResponse> UpdateSignerStatusAsync(Guid signerId, string status)
    {
        var signer = await _context.WalletSigners.FindAsync(signerId);
        if (signer == null)
        {
            throw new ArgumentException($"Signer {signerId} not found");
        }

        signer.Status = status;
        signer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return signer.Adapt<WalletSignerResponse>();
    }

    public async Task<bool> RemoveWalletSignerAsync(Guid signerId)
    {
        var signer = await _context.WalletSigners.FindAsync(signerId);
        if (signer == null)
        {
            return false;
        }

        signer.Status = "Revoked";
        signer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Balance Management
    public async Task<List<WalletBalanceResponse>> GetWalletBalancesAsync(Guid walletId)
    {
        var balances = await _context.WalletBalances
            .Where(b => b.WalletId == walletId)
            .ToListAsync();

        return balances.Adapt<List<WalletBalanceResponse>>();
    }

    public async Task<WalletBalanceResponse> GetWalletBalanceAsync(Guid walletId, string tokenSymbol)
    {
        var balance = await _context.WalletBalances
            .FirstOrDefaultAsync(b => b.WalletId == walletId && b.TokenSymbol == tokenSymbol);

        if (balance == null)
        {
            throw new ArgumentException($"Balance for {tokenSymbol} not found in wallet {walletId}");
        }

        return balance.Adapt<WalletBalanceResponse>();
    }

    public async Task<WalletBalanceResponse> UpdateWalletBalanceAsync(Guid walletId, string tokenSymbol, decimal balance)
    {
        var walletBalance = await _context.WalletBalances
            .FirstOrDefaultAsync(b => b.WalletId == walletId && b.TokenSymbol == tokenSymbol);

        if (walletBalance == null)
        {
            // Create new balance record
            walletBalance = new WalletBalance
            {
                WalletId = walletId,
                TokenSymbol = tokenSymbol,
                TokenName = tokenSymbol,
                TokenDecimals = tokenSymbol == "ETH" ? 18 : 18, // Default to 18
                Balance = balance,
                LockedBalance = 0,
                AvailableBalance = balance
            };

            _context.WalletBalances.Add(walletBalance);
        }
        else
        {
            walletBalance.Balance = balance;
            walletBalance.AvailableBalance = balance - walletBalance.LockedBalance;
            walletBalance.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return walletBalance.Adapt<WalletBalanceResponse>();
    }

    public async Task SyncWalletBalancesAsync(Guid walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);
        if (wallet == null)
        {
            throw new ArgumentException($"Wallet {walletId} not found");
        }

        try
        {
            // Sync ETH balance
            var ethBalance = await _blockchainService.GetEthBalanceAsync(wallet.Address, wallet.Network);
            await UpdateWalletBalanceAsync(walletId, "ETH", ethBalance);

            // Update wallet's main balance field
            wallet.Balance = ethBalance;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Synced balances for wallet {WalletId}", walletId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing balances for wallet {WalletId}", walletId);
            throw;
        }
    }

    // Transaction Management - Placeholder implementations
    public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
    {
        // TODO: Implement transaction creation logic
        throw new NotImplementedException("Transaction creation will be implemented in the next phase");
    }

    public async Task<TransactionResponse> GetTransactionAsync(Guid transactionId)
    {
        // TODO: Implement transaction retrieval logic
        throw new NotImplementedException("Transaction retrieval will be implemented in the next phase");
    }

    public async Task<TransactionResponse> GetTransactionByHashAsync(string hash)
    {
        // TODO: Implement transaction retrieval by hash logic
        throw new NotImplementedException("Transaction retrieval by hash will be implemented in the next phase");
    }

    public async Task<List<TransactionResponse>> GetWalletTransactionsAsync(Guid walletId, int page = 1, int pageSize = 50)
    {
        // TODO: Implement wallet transactions retrieval logic
        throw new NotImplementedException("Wallet transactions retrieval will be implemented in the next phase");
    }

    public async Task<List<TransactionResponse>> GetUserTransactionsAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        // TODO: Implement user transactions retrieval logic
        throw new NotImplementedException("User transactions retrieval will be implemented in the next phase");
    }

    public async Task<List<TransactionResponse>> GetPendingTransactionsAsync(Guid walletId)
    {
        // TODO: Implement pending transactions retrieval logic
        throw new NotImplementedException("Pending transactions retrieval will be implemented in the next phase");
    }

    // Transaction Signing - Placeholder implementations
    public async Task<TransactionSignatureResponse> SignTransactionAsync(SignTransactionRequest request)
    {
        // TODO: Implement transaction signing logic
        throw new NotImplementedException("Transaction signing will be implemented in the next phase");
    }

    public async Task<TransactionSignatureResponse> RejectTransactionAsync(RejectTransactionRequest request)
    {
        // TODO: Implement transaction rejection logic
        throw new NotImplementedException("Transaction rejection will be implemented in the next phase");
    }

    public async Task<List<TransactionSignatureResponse>> GetTransactionSignaturesAsync(Guid transactionId)
    {
        // TODO: Implement transaction signatures retrieval logic
        throw new NotImplementedException("Transaction signatures retrieval will be implemented in the next phase");
    }

    public async Task<TransactionResponse> BroadcastTransactionAsync(Guid transactionId)
    {
        // TODO: Implement transaction broadcasting logic
        throw new NotImplementedException("Transaction broadcasting will be implemented in the next phase");
    }

    // Broadcast a signed raw transaction directly to the specified network (EVM/Tron/etc).
    // This method will call the lower-level blockchain service to send the raw signed transaction.
    public async Task<BroadcastResponse> BroadcastSignedTransactionAsync(BroadcastSignedTransactionRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Network)) throw new ArgumentException("Network is required", nameof(request.Network));
        if (string.IsNullOrWhiteSpace(request.SignedTransaction)) throw new ArgumentException("SignedTransaction is required", nameof(request.SignedTransaction));

        try
        {
            _logger.LogInformation("Broadcasting signed transaction to network {Network}", request.Network);

            // Delegate to blockchain service to send raw transaction
            var txHash = await _blockchainService.SendRawTransactionAsync(request.SignedTransaction, request.Network);

            var resp = new BroadcastResponse
            {
                TxHash = txHash ?? string.Empty,
                Status = string.IsNullOrWhiteSpace(txHash) ? "failed" : "broadcasted"
            };

            _logger.LogInformation("Broadcast result for network {Network}: {TxHash}", request.Network, resp.TxHash);
            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting signed transaction to network {Network}", request.Network);
            throw;
        }
    }

    // Blockchain Operations - Delegate to BlockchainService
    public async Task<string> GenerateWalletAddressAsync(string network, string walletType)
    {
        var (address, _, _) = await _blockchainService.GenerateWalletAsync(network);
        return address;
    }

    public async Task<decimal> GetNetworkBalanceAsync(string address, string network, string? tokenAddress = null)
    {
        if (string.IsNullOrEmpty(tokenAddress))
        {
            return await _blockchainService.GetEthBalanceAsync(address, network);
        }
        else
        {
            return await _blockchainService.GetTokenBalanceAsync(address, tokenAddress, network);
        }
    }

    public async Task<string> EstimateGasAsync(string fromAddress, string toAddress, decimal amount, string? data = null)
    {
        var gasEstimate = await _blockchainService.EstimateGasAsync(fromAddress, toAddress, amount, data, "Ethereum");
        return gasEstimate.ToString();
    }

    public async Task<decimal> GetCurrentGasPriceAsync(string network)
    {
        return await _blockchainService.GetCurrentGasPriceAsync(network);
    }

    public async Task<long> GetNextNonceAsync(string address, string network)
    {
        return await _blockchainService.GetTransactionCountAsync(address, network);
    }

    // Statistics and Monitoring
    public async Task<WalletStatsResponse> GetWalletStatsAsync(Guid userId)
    {
        var wallets = await _context.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync();

        var transactions = await _context.Transactions
            .Where(t => wallets.Select(w => w.Id).Contains(t.WalletId))
            .ToListAsync();

        return new WalletStatsResponse
        {
            TotalWallets = wallets.Count,
            ActiveWallets = wallets.Count(w => w.Status == "Active"),
            MultiSigWallets = wallets.Count(w => w.WalletType == "MultiSig"),
            TotalBalance = wallets.Sum(w => w.Balance),
            PendingTransactions = transactions.Count(t => t.Status == "Pending"),
            CompletedTransactions = transactions.Count(t => t.Status == "Confirmed"),
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<List<WalletNetworkStatsResponse>> GetNetworkStatsAsync()
    {
        var networkStats = await _context.Wallets
            .GroupBy(w => w.Network)
            .Select(g => new WalletNetworkStatsResponse
            {
                Network = g.Key,
                WalletCount = g.Count(),
                TotalBalance = g.Sum(w => w.Balance),
                TransactionCount = _context.Transactions.Count(t => t.Network == g.Key),
                LastUpdated = DateTime.UtcNow
            })
            .ToListAsync();

        return networkStats;
    }

    public async Task<bool> ValidateAddressAsync(string address, string network)
    {
        return await _blockchainService.IsValidAddressAsync(address);
    }

    public async Task<bool> IsContractAddressAsync(string address, string network)
    {
        return await _blockchainService.IsContractAddressAsync(address, network);
    }

    // Security and Validation - Placeholder implementations
    public async Task<bool> ValidateTransactionAsync(Guid transactionId)
    {
        // TODO: Implement transaction validation logic
        throw new NotImplementedException("Transaction validation will be implemented in the next phase");
    }

    public async Task<bool> ValidateSignatureAsync(Guid transactionId, Guid signerId, string signature)
    {
        // TODO: Implement signature validation logic
        throw new NotImplementedException("Signature validation will be implemented in the next phase");
    }

    public async Task<bool> CanUserSignTransactionAsync(Guid userId, Guid transactionId)
    {
        // TODO: Implement user signing permission logic
        throw new NotImplementedException("User signing permission check will be implemented in the next phase");
    }

    public async Task<bool> IsTransactionReadyToBroadcastAsync(Guid transactionId)
    {
        // TODO: Implement transaction broadcast readiness logic
        throw new NotImplementedException("Transaction broadcast readiness check will be implemented in the next phase");
    }
}
