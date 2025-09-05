using Mapster;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Data.Entities;
using UserService.Models.Infrastructure;
using UserService.Models.Requests;
using UserService.Models.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services;

public class WalletManagementService : IWalletManagementService
{
    private readonly UserDbContext _context;
    private readonly IInfrastructureServiceClient _infrastructureClient;
    private readonly ILogger<WalletManagementService> _logger;

    public WalletManagementService(
        UserDbContext context,
        IInfrastructureServiceClient infrastructureClient,
        ILogger<WalletManagementService> logger)
    {
        _context = context;
        _infrastructureClient = infrastructureClient;
        _logger = logger;
    }

    public async Task<UserWalletResponse> CreateMultiSigWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Creating multi-sig wallet for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            // Check if user already has a wallet
            var existingWallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (existingWallet != null)
            {
                _logger.LogWarning("User {UserId} already has a wallet: {WalletAddress}", userId, existingWallet.WalletAddress);
                return existingWallet.Adapt<UserWalletResponse>();
            }

            // Get user information
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Create wallet request for Infrastructure Service
            var createRequest = new Models.Infrastructure.CreateWalletRequest
            {
                UserId = userId,
                UserEmail = user.Email,
                WalletType = "MultiSig",
                RequiredSignatures = 2,
                TotalSigners = 3,
                Network = "Ethereum",
                Metadata = new Dictionary<string, object>
                {
                    ["correlationId"] = correlationId,
                    ["userFirstName"] = user.FirstName,
                    ["userLastName"] = user.LastName
                }
            };

            // Call Infrastructure Service to create wallet
            var response = await _infrastructureClient.CreateWalletAsync(createRequest, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content?.Success != true || response.Content.Data == null)
            {
                var errorMessage = response.Content?.ErrorMessage ?? "Failed to create wallet in Infrastructure Service";
                throw new InvalidOperationException($"Infrastructure Service error: {errorMessage}");
            }

            var walletData = response.Content.Data;

            // Create local wallet record
            var userWallet = new UserWallet
            {
                UserId = userId,
                WalletAddress = walletData.WalletAddress,
                PublicKey = walletData.PublicKey,
                RequiredSignatures = walletData.RequiredSignatures,
                TotalSigners = walletData.TotalSigners,
                WalletType = walletData.WalletType,
                Network = walletData.Network,
                IsActive = walletData.IsActive,
                IsVerified = walletData.IsVerified,
                Balance = walletData.Balance,
                BalanceCurrency = walletData.BalanceCurrency
            };

            _context.UserWallets.Add(userWallet);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Multi-sig wallet created successfully for user {UserId}: {WalletAddress} (CorrelationId: {CorrelationId})", 
                userId, walletData.WalletAddress, correlationId);

            return userWallet.Adapt<UserWalletResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create multi-sig wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<UserWalletResponse> GetWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"Wallet not found for user {userId}");
        }

        return wallet.Adapt<UserWalletResponse>();
    }

    public async Task<UserWalletResponse?> GetWalletByAddressAsync(string walletAddress, CancellationToken cancellationToken = default)
    {
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.WalletAddress == walletAddress, cancellationToken);

        return wallet?.Adapt<UserWalletResponse>();
    }

    public async Task<bool> VerifyWalletAsync(Guid userId, string signature, string message, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Verifying wallet for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet not found for user {userId}");
            }

            // Verify with Infrastructure Service
            var verifyRequest = new VerifyWalletRequest
            {
                WalletAddress = wallet.WalletAddress,
                Signature = signature,
                Message = message,
                PublicKey = wallet.PublicKey
            };

            var response = await _infrastructureClient.VerifyWalletAsync(wallet.WalletAddress, verifyRequest, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content?.Success != true || response.Content.Data == null)
            {
                _logger.LogWarning("Wallet verification failed for user {UserId}: {WalletAddress}", userId, wallet.WalletAddress);
                return false;
            }

            var verificationResult = response.Content.Data;

            if (verificationResult.IsValid)
            {
                // Update local wallet verification status
                wallet.IsVerified = true;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Wallet verified successfully for user {UserId}: {WalletAddress} (CorrelationId: {CorrelationId})", 
                    userId, wallet.WalletAddress, correlationId);
            }

            return verificationResult.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<UserWalletResponse> UpdateWalletBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating wallet balance for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet not found for user {userId}");
            }

            // Get updated balance from Infrastructure Service
            var response = await _infrastructureClient.GetWalletBalanceAsync(wallet.WalletAddress, wallet.Network, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content?.Success != true || response.Content.Data == null)
            {
                var errorMessage = response.Content?.ErrorMessage ?? "Failed to get wallet balance";
                throw new InvalidOperationException($"Infrastructure Service error: {errorMessage}");
            }

            var balanceData = response.Content.Data;

            // Update local wallet balance
            wallet.Balance = balanceData.Balance;
            wallet.BalanceCurrency = balanceData.BalanceCurrency;
            wallet.LastTransactionAt = balanceData.LastTransactionAt;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet balance updated for user {UserId}: {Balance} {Currency} (CorrelationId: {CorrelationId})", 
                userId, wallet.Balance, wallet.BalanceCurrency, correlationId);

            return wallet.Adapt<UserWalletResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wallet balance for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<bool> DeactivateWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Deactivating wallet for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                return false;
            }

            // Deactivate in Infrastructure Service
            var response = await _infrastructureClient.DeactivateWalletAsync(wallet.WalletAddress, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                // Update local wallet status
                wallet.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Wallet deactivated for user {UserId}: {WalletAddress} (CorrelationId: {CorrelationId})", 
                    userId, wallet.WalletAddress, correlationId);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<bool> SyncWalletWithInfrastructureAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Syncing wallet with Infrastructure Service for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                return false;
            }

            // Get latest wallet data from Infrastructure Service
            var response = await _infrastructureClient.GetWalletAsync(wallet.WalletAddress, cancellationToken);

            if (!response.IsSuccessStatusCode || response.Content?.Success != true || response.Content.Data == null)
            {
                return false;
            }

            var walletData = response.Content.Data;

            // Update local wallet with Infrastructure Service data
            wallet.IsActive = walletData.IsActive;
            wallet.IsVerified = walletData.IsVerified;
            wallet.Balance = walletData.Balance;
            wallet.BalanceCurrency = walletData.BalanceCurrency;
            wallet.LastTransactionAt = walletData.LastTransactionAt;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet synced successfully for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            return false;
        }
    }

    public async Task<IEnumerable<TokenBalance>> GetTokenBalancesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            return Enumerable.Empty<TokenBalance>();
        }

        try
        {
            var response = await _infrastructureClient.GetWalletBalanceAsync(wallet.WalletAddress, wallet.Network, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content?.Success == true && response.Content.Data != null)
            {
                return response.Content.Data.TokenBalances;
            }

            return Enumerable.Empty<TokenBalance>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get token balances for user {UserId}", userId);
            return Enumerable.Empty<TokenBalance>();
        }
    }

    public async Task<object> GetWalletTransactionsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"Wallet not found for user {userId}");
        }

        try
        {
            var response = await _infrastructureClient.GetWalletTransactionsAsync(wallet.WalletAddress, page, pageSize, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                return response.Content.Data ?? new object();
            }

            return new object();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get wallet transactions for user {UserId}", userId);
            throw;
        }
    }

    // Missing methods needed by controller
    public async Task<UserWalletResponse> CreateWalletAsync(Models.Requests.CreateWalletRequest request, CancellationToken cancellationToken = default)
    {
        return await CreateMultiSigWalletAsync(request.UserId, cancellationToken);
    }

    public async Task<UserWalletResponse> UpdateWalletAsync(Guid userId, Models.Requests.UpdateWalletRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating wallet for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet not found for user {userId}");
            }

            // Update wallet properties
            if (request.IsActive.HasValue)
            {
                wallet.IsActive = request.IsActive.Value;
            }

            if (!string.IsNullOrEmpty(request.WalletType))
            {
                wallet.WalletType = request.WalletType;
            }

            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet updated for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);

            return wallet.Adapt<UserWalletResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<WalletBalanceResponse> GetWalletBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await _context.UserWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet == null)
        {
            throw new InvalidOperationException($"Wallet not found for user {userId}");
        }

        return new WalletBalanceResponse
        {
            UserId = wallet.UserId,
            WalletAddress = wallet.WalletAddress,
            TotalBalance = wallet.Balance,
            AvailableBalance = wallet.Balance,
            Currency = wallet.BalanceCurrency ?? "ETH",
            LastUpdated = wallet.LastTransactionAt ?? DateTime.UtcNow,
            IsActive = wallet.IsActive
        };
    }

    public async Task<WalletBalanceResponse> UpdateWalletBalanceAsync(Guid userId, Models.Requests.UpdateWalletBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Updating wallet balance for user {UserId} with amount {Amount} (CorrelationId: {CorrelationId})", 
            userId, request.Amount, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet not found for user {userId}");
            }

            // Update balance
            wallet.Balance = request.Amount;
            wallet.LastTransactionAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet balance updated for user {UserId}: {Balance} (CorrelationId: {CorrelationId})", 
                userId, wallet.Balance, correlationId);

            return new WalletBalanceResponse
            {
                UserId = wallet.UserId,
                WalletAddress = wallet.WalletAddress,
                TotalBalance = wallet.Balance,
                AvailableBalance = wallet.Balance,
                Currency = wallet.BalanceCurrency ?? "ETH",
                LastUpdated = wallet.LastTransactionAt ?? DateTime.UtcNow,
                IsActive = wallet.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update wallet balance for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<bool> FreezeWalletAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Freezing wallet for user {UserId} with reason: {Reason} (CorrelationId: {CorrelationId})", 
            userId, reason, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                return false;
            }

            // Update wallet status (freeze by setting IsActive to false)
            wallet.IsActive = false;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet frozen for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to freeze wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public async Task<bool> UnfreezeWalletAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("Unfreezing wallet for user {UserId} (CorrelationId: {CorrelationId})", 
            userId, correlationId);

        try
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

            if (wallet == null)
            {
                return false;
            }

            // Update wallet status (unfreeze by setting IsActive to true)
            wallet.IsActive = true;
            wallet.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Wallet unfrozen for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unfreeze wallet for user {UserId} (CorrelationId: {CorrelationId})", 
                userId, correlationId);
            throw;
        }
    }

    public Task<bool> ValidateWalletAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic validation - check if address format is valid
            if (string.IsNullOrWhiteSpace(address))
            {
                return Task.FromResult(false);
            }

            // Ethereum address validation (basic)
            if (address.StartsWith("0x") && address.Length == 42)
            {
                return Task.FromResult(true);
            }

            // Additional validation logic can be added here
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate wallet address {Address}", address);
            return Task.FromResult(false);
        }
    }
}
