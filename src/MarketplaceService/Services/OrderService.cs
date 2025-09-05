using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MarketplaceService.Data;
using MarketplaceService.Data.Entities;
using MarketplaceService.Services.Interfaces;

namespace MarketplaceService.Services;

public class OrderService : IOrderService
{
    private readonly MarketplaceDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrderService> _logger;
    private readonly IEscrowService _escrowService;
    private readonly IPricingService _pricingService;

    public OrderService(
        MarketplaceDbContext context,
        IDistributedCache cache,
        ILogger<OrderService> logger,
        IEscrowService escrowService,
        IPricingService pricingService)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _escrowService = escrowService;
        _pricingService = pricingService;
    }

    public async Task<MarketplaceOrderDto> CreateOrderAsync(CreateOrderRequest request, Guid buyerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new order for buyer {BuyerId} on listing {ListingId}", buyerId, request.ListingId);

        // Get the listing
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == request.ListingId && l.Status == ListingStatus.Active, cancellationToken);

        if (listing == null)
        {
            throw new InvalidOperationException($"Listing {request.ListingId} not found or not active");
        }

        // Validate quantity
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (request.Quantity > listing.RemainingQuantity)
        {
            throw new InvalidOperationException("Insufficient quantity available");
        }

        if (request.Quantity < listing.MinimumPurchaseQuantity)
        {
            throw new InvalidOperationException($"Minimum purchase quantity is {listing.MinimumPurchaseQuantity}");
        }

        if (listing.MaximumPurchaseQuantity.HasValue && request.Quantity > listing.MaximumPurchaseQuantity.Value)
        {
            throw new InvalidOperationException($"Maximum purchase quantity is {listing.MaximumPurchaseQuantity}");
        }

        // Calculate pricing
        var pricePerToken = listing.BasePrice ?? 0;
        var totalAmount = pricePerToken * request.Quantity;
        var platformFee = totalAmount * 0.025m; // 2.5% platform fee
        var transactionFee = totalAmount * 0.005m; // 0.5% transaction fee
        var totalFees = platformFee + transactionFee;
        var finalAmount = totalAmount + totalFees;

        // Create order
        var order = new MarketplaceOrder
        {
            Id = Guid.NewGuid(),
            ListingId = request.ListingId,
            BuyerId = buyerId,
            SellerId = listing.SellerId,
            Status = OrderStatus.Pending,
            TransactionType = TransactionType.Purchase,
            Quantity = request.Quantity,
            PricePerToken = pricePerToken,
            TotalAmount = totalAmount,
            Currency = listing.Currency,
            PlatformFee = platformFee,
            TransactionFee = transactionFee,
            TotalFees = totalFees,
            FinalAmount = finalAmount,
            BuyerNotes = request.BuyerNotes,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hour expiry
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(1),
            MaxRetryAttempts = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MarketplaceOrders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created order {OrderId} for buyer {BuyerId}", order.Id, buyerId);

        return MapToDto(order);
    }

    public async Task<MarketplaceOrderDto?> GetOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        return order == null ? null : MapToDto(order);
    }

    public async Task<MarketplaceOrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid userId, string? notes = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, status);

        var order = await _context.MarketplaceOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found or access denied");
        }

        var previousStatus = order.Status;
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        // Update status-specific fields
        switch (status)
        {
            case OrderStatus.Confirmed:
                order.ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Paid:
                order.PaidAt = DateTime.UtcNow;
                break;
            case OrderStatus.Completed:
                order.CompletedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                order.CancellationReason = notes;
                break;
            case OrderStatus.Failed:
                order.FailureReason = notes;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated order {OrderId} status from {PreviousStatus} to {NewStatus}", orderId, previousStatus, status);

        return MapToDto(order);
    }

    public async Task<MarketplaceOrderDto> CancelOrderAsync(Guid orderId, Guid userId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling order {OrderId} with reason: {Reason}", orderId, reason);

        var order = await _context.MarketplaceOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found or access denied");
        }

        if (order.Status == OrderStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel completed order");
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = reason;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled order {OrderId}", orderId);

        return MapToDto(order);
    }

    public async Task<PaginatedOrderResponse> GetUserOrdersAsync(Guid userId, OrderStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.BuyerId == userId || o.SellerId == userId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedOrderResponse
        {
            Orders = orders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<PaginatedOrderResponse> GetListingOrdersAsync(Guid listingId, Guid sellerId, OrderStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .Where(o => o.ListingId == listingId && o.SellerId == sellerId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedOrderResponse
        {
            Orders = orders.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<MarketplaceOrderDto> ProcessPaymentConfirmationAsync(Guid orderId, string paymentTransactionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment confirmation for order {OrderId} with transaction {TransactionId}", orderId, paymentTransactionId);

        var order = await _context.MarketplaceOrders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        if (order.Status != OrderStatus.Confirmed)
        {
            throw new InvalidOperationException($"Order must be in Confirmed status to process payment. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Paid;
        order.PaymentTransactionId = paymentTransactionId;
        order.PaidAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Processed payment confirmation for order {OrderId}", orderId);

        return MapToDto(order);
    }

    public async Task<MarketplaceOrderDto> CompleteOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing order {OrderId}", orderId);

        var order = await _context.MarketplaceOrders
            .Include(o => o.MarketListing)
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found or access denied");
        }

        if (order.Status != OrderStatus.Paid)
        {
            throw new InvalidOperationException($"Order must be paid to complete. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Update listing quantities
        if (order.MarketListing != null)
        {
            order.MarketListing.RemainingQuantity -= order.Quantity;
            order.MarketListing.VolumeSold += order.Quantity;
            order.MarketListing.OrderCount++;
            order.MarketListing.TotalRevenue += order.TotalAmount;
            order.MarketListing.UpdatedAt = DateTime.UtcNow;

            // Check if listing is sold out
            if (order.MarketListing.RemainingQuantity <= 0)
            {
                order.MarketListing.Status = ListingStatus.SoldOut;
                order.MarketListing.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Completed order {OrderId}", orderId);

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderHistoryDto>> GetOrderHistoryAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        var order = await _context.MarketplaceOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found or access denied");
        }

        // For now, return a simple history based on the current order state
        // In a full implementation, you would have a separate OrderHistory table
        var history = new List<OrderHistoryDto>
        {
            new OrderHistoryDto
            {
                Id = Guid.NewGuid(),
                Status = order.Status,
                Action = "Order Created",
                Description = "Order was created",
                IsSystemAction = true,
                SystemComponent = "OrderService",
                CreatedAt = order.CreatedAt
            }
        };

        if (order.ConfirmedAt.HasValue)
        {
            history.Add(new OrderHistoryDto
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Confirmed,
                PreviousStatus = OrderStatus.Pending,
                Action = "Order Confirmed",
                Description = "Order was confirmed",
                IsSystemAction = true,
                SystemComponent = "OrderService",
                CreatedAt = order.ConfirmedAt.Value
            });
        }

        if (order.PaidAt.HasValue)
        {
            history.Add(new OrderHistoryDto
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Paid,
                PreviousStatus = OrderStatus.Confirmed,
                Action = "Payment Processed",
                Description = "Payment was processed successfully",
                IsSystemAction = true,
                SystemComponent = "PaymentService",
                CreatedAt = order.PaidAt.Value
            });
        }

        if (order.CompletedAt.HasValue)
        {
            history.Add(new OrderHistoryDto
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Completed,
                PreviousStatus = OrderStatus.Paid,
                Action = "Order Completed",
                Description = "Order was completed successfully",
                IsSystemAction = true,
                SystemComponent = "OrderService",
                CreatedAt = order.CompletedAt.Value
            });
        }

        if (order.CancelledAt.HasValue)
        {
            history.Add(new OrderHistoryDto
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Cancelled,
                Action = "Order Cancelled",
                Description = $"Order was cancelled. Reason: {order.CancellationReason}",
                IsSystemAction = false,
                CreatedAt = order.CancelledAt.Value
            });
        }

        return history.OrderBy(h => h.CreatedAt);
    }

    public async Task<MarketplaceOrderDto> RetryOrderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying order {OrderId}", orderId);

        var order = await _context.MarketplaceOrders
            .FirstOrDefaultAsync(o => o.Id == orderId && (o.BuyerId == userId || o.SellerId == userId), cancellationToken);

        if (order == null)
        {
            throw new InvalidOperationException($"Order {orderId} not found or access denied");
        }

        if (order.Status != OrderStatus.Failed)
        {
            throw new InvalidOperationException($"Can only retry failed orders. Current status: {order.Status}");
        }

        if (order.RetryCount >= order.MaxRetryAttempts)
        {
            throw new InvalidOperationException($"Maximum retry attempts ({order.MaxRetryAttempts}) exceeded");
        }

        order.Status = OrderStatus.Pending;
        order.RetryCount++;
        order.FailureReason = null;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retried order {OrderId} (attempt {RetryCount})", orderId, order.RetryCount);

        return MapToDto(order);
    }

    private static MarketplaceOrderDto MapToDto(MarketplaceOrder order)
    {
        return new MarketplaceOrderDto
        {
            Id = order.Id,
            ListingId = order.ListingId,
            BuyerId = order.BuyerId,
            SellerId = order.SellerId,
            Status = order.Status,
            TransactionType = order.TransactionType,
            Quantity = order.Quantity,
            PricePerToken = order.PricePerToken,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            PlatformFee = order.PlatformFee,
            TransactionFee = order.TransactionFee,
            TotalFees = order.TotalFees,
            FinalAmount = order.FinalAmount,
            EscrowAccountId = order.EscrowAccountId,
            PaymentTransactionId = order.PaymentTransactionId,
            BlockchainTransactionHash = order.BlockchainTransactionHash,
            BuyerNotes = order.BuyerNotes,
            SellerNotes = order.SellerNotes,
            CancellationReason = order.CancellationReason,
            FailureReason = order.FailureReason,
            ConfirmedAt = order.ConfirmedAt,
            PaidAt = order.PaidAt,
            CompletedAt = order.CompletedAt,
            CancelledAt = order.CancelledAt,
            ExpiresAt = order.ExpiresAt,
            EstimatedCompletionAt = order.EstimatedCompletionAt,
            RetryCount = order.RetryCount,
            MaxRetryAttempts = order.MaxRetryAttempts,
            RequiresManualReview = order.RequiresManualReview,
            IsReviewed = order.IsReviewed,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Listing = order.MarketListing != null ? MapListingToDto(order.MarketListing) : null
        };
    }

    private static MarketListingDto MapListingToDto(MarketListing listing)
    {
        return new MarketListingDto
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            MarketType = listing.MarketType,
            AssetType = listing.AssetType,
            AssetSymbol = listing.AssetSymbol,
            TotalQuantity = listing.TotalQuantity,
            RemainingQuantity = listing.RemainingQuantity,
            BasePrice = listing.BasePrice,
            Currency = listing.Currency,
            PricingStrategy = listing.PricingStrategy,
            IsFeatured = listing.IsFeatured,
            IsVerified = listing.IsVerified,
            ViewCount = listing.ViewCount,
            OrderCount = listing.OrderCount,
            CreatedAt = listing.CreatedAt,
            Tags = listing.Tags
        };
    }
}
