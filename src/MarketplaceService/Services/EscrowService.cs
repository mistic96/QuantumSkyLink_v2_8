using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

public class EscrowService : IEscrowService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<EscrowService> _logger;

    public EscrowService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<EscrowService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<EscrowAccountDto> CreateEscrowAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating escrow account for order {OrderId}", orderId);

        // Get the order to extract escrow details
        var order = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        var escrowAccount = new EscrowAccount
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            BuyerId = order.BuyerId,
            SellerId = order.SellerId,
            Status = EscrowStatus.Created,
            EscrowAmount = order.TotalAmount,
            Currency = order.Currency,
            TokenQuantity = order.Quantity,
            AssetType = order.MarketListing.AssetType,
            AssetSymbol = order.MarketListing.AssetSymbol,
            EscrowFee = order.TotalFees,
            NetReleaseAmount = order.FinalAmount,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Default 30 days
            AutoReleaseEnabled = true,
            AutoReleaseDelayHours = 24,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowAccounts.Add(escrowAccount);

        // Create initial escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Created,
            Action = "Created",
            Description = "Escrow account created",
            PerformedByUserId = order.BuyerId,
            IsSystemAction = false,
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created escrow account {EscrowId} for {Amount} {Currency}", 
            escrowAccount.Id, escrowAccount.EscrowAmount, escrowAccount.Currency);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto?> GetEscrowAsync(Guid escrowId, Guid userId, CancellationToken cancellationToken = default)
    {
        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId && (e.BuyerId == userId || e.SellerId == userId), cancellationToken);

        return escrowAccount != null ? MapToDto(escrowAccount) : null;
    }

    public async Task<EscrowAccountDto> LockAssetsAsync(Guid escrowId, string? lockTransactionHash = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Locking assets for escrow account {EscrowId}", escrowId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status != EscrowStatus.Created)
        {
            throw new InvalidOperationException($"Cannot lock assets for escrow in {escrowAccount.Status} status");
        }

        escrowAccount.Status = EscrowStatus.Locked;
        escrowAccount.LockedAt = DateTime.UtcNow;
        escrowAccount.LockTransactionHash = lockTransactionHash;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Locked,
            PreviousStatus = EscrowStatus.Created,
            Action = "AssetsLocked",
            Description = "Assets locked in escrow",
            TransactionHash = lockTransactionHash,
            IsSystemAction = true,
            SystemComponent = "EscrowService",
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Locked assets for escrow account {EscrowId}", escrowId);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto> FundEscrowAsync(Guid escrowId, string paymentTransactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Funding escrow account {EscrowId} with transaction {TransactionId}", 
            escrowId, paymentTransactionId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status != EscrowStatus.Locked && escrowAccount.Status != EscrowStatus.Created)
        {
            throw new InvalidOperationException($"Cannot fund escrow in {escrowAccount.Status} status");
        }

        escrowAccount.Status = EscrowStatus.Funded;
        escrowAccount.FundedAt = DateTime.UtcNow;
        escrowAccount.PaymentTransactionId = paymentTransactionId;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Set auto-release time if enabled
        if (escrowAccount.AutoReleaseEnabled)
        {
            escrowAccount.AutoReleaseAt = DateTime.UtcNow.AddHours(escrowAccount.AutoReleaseDelayHours);
        }

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Funded,
            PreviousStatus = escrowAccount.Status == EscrowStatus.Locked ? EscrowStatus.Locked : EscrowStatus.Created,
            Action = "EscrowFunded",
            Description = $"Escrow funded with payment transaction {paymentTransactionId}",
            TransactionHash = paymentTransactionId,
            IsSystemAction = true,
            SystemComponent = "PaymentGateway",
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Funded escrow account {EscrowId} with {Amount} {Currency}", 
            escrowId, escrowAccount.EscrowAmount, escrowAccount.Currency);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto> ReleaseEscrowAsync(Guid escrowId, Guid userId, string? releaseTransactionHash = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Releasing escrow account {EscrowId} by user {UserId}", escrowId, userId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status != EscrowStatus.Funded)
        {
            throw new InvalidOperationException($"Cannot release escrow in {escrowAccount.Status} status");
        }

        // Validate user has permission to release
        if (userId != escrowAccount.BuyerId && userId != escrowAccount.SellerId)
        {
            throw new InvalidOperationException("Only buyer or seller can release escrow");
        }

        escrowAccount.Status = EscrowStatus.Released;
        escrowAccount.ReleasedAt = DateTime.UtcNow;
        escrowAccount.ReleaseTransactionHash = releaseTransactionHash;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Released,
            PreviousStatus = EscrowStatus.Funded,
            Action = "EscrowReleased",
            Description = "Escrow released to seller",
            PerformedByUserId = userId,
            TransactionHash = releaseTransactionHash,
            IsSystemAction = false,
            Amount = escrowAccount.NetReleaseAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Released escrow account {EscrowId} with {Amount} {Currency} to seller", 
            escrowId, escrowAccount.NetReleaseAmount, escrowAccount.Currency);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto> CancelEscrowAsync(Guid escrowId, Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling escrow account {EscrowId} by user {UserId}", escrowId, userId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status == EscrowStatus.Released || escrowAccount.Status == EscrowStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel escrow in {escrowAccount.Status} status");
        }

        // Validate user has permission to cancel
        if (userId != escrowAccount.BuyerId && userId != escrowAccount.SellerId)
        {
            throw new InvalidOperationException("Only buyer or seller can cancel escrow");
        }

        var previousStatus = escrowAccount.Status;
        escrowAccount.Status = EscrowStatus.Cancelled;
        escrowAccount.CancelledAt = DateTime.UtcNow;
        escrowAccount.CancellationReason = reason;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Cancelled,
            PreviousStatus = previousStatus,
            Action = "EscrowCancelled",
            Description = $"Escrow cancelled: {reason ?? "No reason provided"}",
            PerformedByUserId = userId,
            IsSystemAction = false,
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled escrow account {EscrowId}", escrowId);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto> InitiateDisputeAsync(Guid escrowId, Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating dispute for escrow account {EscrowId} by user {UserId}", escrowId, userId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status != EscrowStatus.Funded)
        {
            throw new InvalidOperationException($"Cannot dispute escrow in {escrowAccount.Status} status");
        }

        // Validate that the disputer is either buyer or seller
        if (userId != escrowAccount.BuyerId && userId != escrowAccount.SellerId)
        {
            throw new InvalidOperationException("Only buyer or seller can dispute the escrow");
        }

        escrowAccount.IsDisputed = true;
        escrowAccount.DisputeReason = reason;
        escrowAccount.DisputeInitiatedBy = userId;
        escrowAccount.DisputeInitiatedAt = DateTime.UtcNow;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = escrowAccount.Status,
            Action = "DisputeInitiated",
            Description = $"Dispute initiated: {reason}",
            PerformedByUserId = userId,
            IsSystemAction = false,
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initiated dispute for escrow account {EscrowId}", escrowId);

        return MapToDto(escrowAccount);
    }

    public async Task<EscrowAccountDto> ResolveDisputeAsync(Guid escrowId, Guid adminUserId, string resolution, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resolving dispute for escrow account {EscrowId} by admin {AdminUserId}", escrowId, adminUserId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (!escrowAccount.IsDisputed)
        {
            throw new InvalidOperationException("Cannot resolve dispute for non-disputed escrow");
        }

        escrowAccount.DisputeResolution = resolution;
        escrowAccount.DisputeResolvedBy = adminUserId;
        escrowAccount.DisputeResolvedAt = DateTime.UtcNow;
        escrowAccount.IsDisputed = false;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = escrowAccount.Status,
            Action = "DisputeResolved",
            Description = $"Dispute resolved: {resolution}",
            PerformedByUserId = adminUserId,
            IsSystemAction = false,
            Amount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Resolved dispute for escrow account {EscrowId}", escrowId);

        return MapToDto(escrowAccount);
    }

    public async Task<IEnumerable<EscrowHistoryDto>> GetEscrowHistoryAsync(Guid escrowId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Validate user has access to this escrow
        var hasAccess = await _context.EscrowAccounts
            .AnyAsync(e => e.Id == escrowId && (e.BuyerId == userId || e.SellerId == userId), cancellationToken);

        if (!hasAccess)
        {
            throw new InvalidOperationException("User does not have access to this escrow account");
        }

        var history = await _context.EscrowHistory
            .Where(eh => eh.EscrowAccountId == escrowId)
            .OrderByDescending(eh => eh.CreatedAt)
            .ToListAsync(cancellationToken);

        return history.Select(h => new EscrowHistoryDto
        {
            Id = h.Id,
            Status = h.Status,
            PreviousStatus = h.PreviousStatus,
            Action = h.Action,
            Description = h.Description,
            PerformedByUserId = h.PerformedByUserId,
            IsSystemAction = h.IsSystemAction,
            SystemComponent = h.SystemComponent,
            TransactionHash = h.TransactionHash,
            Amount = h.Amount,
            Currency = h.Currency,
            ErrorMessage = h.ErrorMessage,
            CreatedAt = h.CreatedAt
        });
    }

    public async Task<IEnumerable<EscrowAccountDto>> GetEscrowsReadyForAutoReleaseAsync(CancellationToken cancellationToken = default)
    {
        var readyForRelease = await _context.EscrowAccounts
            .Where(e => e.Status == EscrowStatus.Funded &&
                       e.AutoReleaseEnabled &&
                       e.AutoReleaseAt.HasValue &&
                       e.AutoReleaseAt.Value <= DateTime.UtcNow &&
                       !e.IsDisputed)
            .ToListAsync(cancellationToken);

        return readyForRelease.Select(MapToDto);
    }

    public async Task<EscrowAccountDto> ProcessAutoReleaseAsync(Guid escrowId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing auto-release for escrow account {EscrowId}", escrowId);

        var escrowAccount = await _context.EscrowAccounts
            .FirstOrDefaultAsync(e => e.Id == escrowId, cancellationToken);

        if (escrowAccount == null)
        {
            throw new InvalidOperationException($"Escrow account {escrowId} not found");
        }

        if (escrowAccount.Status != EscrowStatus.Funded)
        {
            throw new InvalidOperationException($"Cannot auto-release escrow in {escrowAccount.Status} status");
        }

        if (!escrowAccount.AutoReleaseEnabled)
        {
            throw new InvalidOperationException("Auto-release is not enabled for this escrow");
        }

        if (escrowAccount.IsDisputed)
        {
            throw new InvalidOperationException("Cannot auto-release disputed escrow");
        }

        escrowAccount.Status = EscrowStatus.Released;
        escrowAccount.ReleasedAt = DateTime.UtcNow;
        escrowAccount.UpdatedAt = DateTime.UtcNow;

        // Create escrow history entry
        var escrowHistory = new EscrowHistory
        {
            Id = Guid.NewGuid(),
            EscrowAccountId = escrowAccount.Id,
            Status = EscrowStatus.Released,
            PreviousStatus = EscrowStatus.Funded,
            Action = "AutoReleased",
            Description = "Escrow automatically released to seller",
            IsSystemAction = true,
            SystemComponent = "AutoReleaseService",
            Amount = escrowAccount.NetReleaseAmount,
            Currency = escrowAccount.Currency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EscrowHistory.Add(escrowHistory);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Auto-released escrow account {EscrowId} with {Amount} {Currency}", 
            escrowId, escrowAccount.NetReleaseAmount, escrowAccount.Currency);

        return MapToDto(escrowAccount);
    }

    private static EscrowAccountDto MapToDto(EscrowAccount escrowAccount)
    {
        return new EscrowAccountDto
        {
            Id = escrowAccount.Id,
            OrderId = escrowAccount.OrderId,
            Status = escrowAccount.Status,
            EscrowAmount = escrowAccount.EscrowAmount,
            Currency = escrowAccount.Currency,
            TokenQuantity = escrowAccount.TokenQuantity,
            AssetType = escrowAccount.AssetType,
            AssetSymbol = escrowAccount.AssetSymbol,
            EscrowFee = escrowAccount.EscrowFee,
            NetReleaseAmount = escrowAccount.NetReleaseAmount,
            LockedAt = escrowAccount.LockedAt,
            FundedAt = escrowAccount.FundedAt,
            ReleasedAt = escrowAccount.ReleasedAt,
            ExpiresAt = escrowAccount.ExpiresAt,
            IsDisputed = escrowAccount.IsDisputed,
            DisputeReason = escrowAccount.DisputeReason,
            AutoReleaseEnabled = escrowAccount.AutoReleaseEnabled,
            AutoReleaseDelayHours = escrowAccount.AutoReleaseDelayHours,
            AutoReleaseAt = escrowAccount.AutoReleaseAt,
            CreatedAt = escrowAccount.CreatedAt,
            UpdatedAt = escrowAccount.UpdatedAt
        };
    }
}
