using System.Text;
using System.Text.Json;
using UnifiedCartService.Models.Entities;
using UnifiedCartService.Repository;

namespace UnifiedCartService.Services;

/// <summary>
/// Orchestrates checkout across Primary and Secondary markets with fail-fast approach
/// </summary>
public class CheckoutOrchestrator : ICheckoutOrchestrator
{
    private readonly IUnifiedCartRepository _cartRepository;
    private readonly IUnifiedCartService _cartService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CheckoutOrchestrator> _logger;

    public CheckoutOrchestrator(
        IUnifiedCartRepository cartRepository,
        IUnifiedCartService cartService,
        IHttpClientFactory httpClientFactory,
        ILogger<CheckoutOrchestrator> logger)
    {
        _cartRepository = cartRepository;
        _cartService = cartService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<CheckoutResult> CheckoutCartAsync(
        string userId, 
        string cartId, 
        CheckoutRequest request, 
        CancellationToken cancellationToken = default)
    {
        var result = new CheckoutResult();
        var sessionId = Guid.NewGuid().ToString();

        try
        {
            // Get and validate cart
            var cart = await _cartService.GetCartByIdAsync(userId, cartId, cancellationToken);
            if (cart == null)
            {
                result.ErrorMessage = "Cart not found or access denied";
                return result;
            }

            if (cart.Status != CartStatus.Active)
            {
                result.ErrorMessage = $"Cannot checkout cart in {cart.Status} status";
                return result;
            }

            // Lock cart for checkout
            var locked = await _cartRepository.LockCartForCheckoutAsync(cartId, sessionId, cancellationToken);
            if (!locked)
            {
                result.ErrorMessage = "Cart is already being processed";
                return result;
            }

            try
            {
                // Validate all items are still available
                var (isValid, errors) = await _cartService.ValidateCartAsync(cartId, cancellationToken);
                if (!isValid)
                {
                    result.ErrorMessage = $"Cart validation failed: {string.Join(", ", errors)}";
                    return result;
                }

                // Get cart items
                var items = await _cartRepository.GetCartItemsAsync(cartId, cancellationToken);
                
                // Group items by market type
                var primaryItems = items.Where(i => i.MarketType == MarketType.Primary).ToList();
                var secondaryItems = items.Where(i => i.MarketType == MarketType.Secondary).ToList();

                // Process Primary Market items
                if (primaryItems.Any())
                {
                    var primaryResult = await ProcessPrimaryMarketCheckoutAsync(
                        userId, primaryItems, request, cancellationToken);
                    
                    if (!primaryResult.success)
                    {
                        result.ErrorMessage = $"Primary market checkout failed: {primaryResult.error}";
                        return result;
                    }
                    
                    result.PrimaryMarketOrderIds.AddRange(primaryResult.orderIds);
                }

                // Process Secondary Market items
                if (secondaryItems.Any())
                {
                    var secondaryResult = await ProcessSecondaryMarketCheckoutAsync(
                        userId, secondaryItems, request, cancellationToken);
                    
                    if (!secondaryResult.success)
                    {
                        result.ErrorMessage = $"Secondary market checkout failed: {secondaryResult.error}";
                        return result;
                    }
                    
                    result.SecondaryMarketOrderIds.AddRange(secondaryResult.orderIds);
                    result.EscrowIds.AddRange(secondaryResult.escrowIds);
                }

                // Process payment
                var paymentResult = await ProcessPaymentAsync(
                    userId, cart.TotalValue, request, cancellationToken);
                
                if (!paymentResult.success)
                {
                    // Rollback orders if payment fails
                    await RollbackOrdersAsync(result, cancellationToken);
                    result.ErrorMessage = $"Payment processing failed: {paymentResult.error}";
                    return result;
                }
                
                result.TransactionId = paymentResult.transactionId;

                // Mark cart as checked out
                cart.Status = CartStatus.CheckedOut;
                await _cartRepository.UpdateCartAsync(cart, cancellationToken);

                // Delete cart from SurrealDB (clean handoff)
                await _cartRepository.DeleteCartAsync(cartId, cancellationToken);

                result.Success = true;
                result.OrderId = $"ORD-{sessionId}";
                
                _logger.LogInformation("Successfully checked out cart {CartId} for user {UserId}", cartId, userId);
            }
            finally
            {
                // Unlock cart if checkout failed
                if (!result.Success)
                {
                    await _cartRepository.UnlockCartAsync(cartId, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checkout failed for cart {CartId}", cartId);
            result.ErrorMessage = "An unexpected error occurred during checkout";
            
            // Ensure cart is unlocked
            try
            {
                await _cartRepository.UnlockCartAsync(cartId, cancellationToken);
            }
            catch { }
        }

        return result;
    }

    private async Task<(bool success, List<string> orderIds, string? error)> ProcessPrimaryMarketCheckoutAsync(
        string userId,
        List<UnifiedCartItem> items,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MarketplaceService");
            var orderIds = new List<string>();

            var orderRequest = new
            {
                UserId = userId,
                Items = items.Select(i => new
                {
                    ListingId = i.ListingId,
                    Quantity = i.Quantity,
                    PricePerUnit = i.PricePerUnit
                }),
                PaymentMethodId = request.PaymentMethodId,
                WalletAddress = request.WalletAddress
            };

            var json = JsonSerializer.Serialize(orderRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/primary/orders", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, orderIds, error);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var order = JsonSerializer.Deserialize<OrderResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (order?.OrderId != null)
            {
                orderIds.Add(order.OrderId);
            }

            return (true, orderIds, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Primary Market checkout");
            return (false, new List<string>(), ex.Message);
        }
    }

    private async Task<(bool success, List<string> orderIds, List<string> escrowIds, string? error)> ProcessSecondaryMarketCheckoutAsync(
        string userId,
        List<UnifiedCartItem> items,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MarketplaceService");
            var orderIds = new List<string>();
            var escrowIds = new List<string>();

            // Group by seller for efficient processing
            var itemsBySeller = items.GroupBy(i => i.SellerId);

            foreach (var sellerGroup in itemsBySeller)
            {
                var escrowRequest = new
                {
                    BuyerId = userId,
                    SellerId = sellerGroup.Key,
                    Items = sellerGroup.Select(i => new
                    {
                        ListingId = i.ListingId,
                        Quantity = i.Quantity,
                        PricePerUnit = i.PricePerUnit,
                        UseEscrow = i.UseEscrow
                    }),
                    PaymentMethodId = request.PaymentMethodId,
                    WalletAddress = request.WalletAddress
                };

                var json = JsonSerializer.Serialize(escrowRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/secondary/escrow", content, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    return (false, orderIds, escrowIds, error);
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var escrow = JsonSerializer.Deserialize<EscrowResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (escrow != null)
                {
                    if (escrow.OrderId != null)
                        orderIds.Add(escrow.OrderId);
                    if (escrow.EscrowId != null)
                        escrowIds.Add(escrow.EscrowId);
                }
            }

            return (true, orderIds, escrowIds, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Secondary Market checkout");
            return (false, new List<string>(), new List<string>(), ex.Message);
        }
    }

    private async Task<(bool success, string? transactionId, string? error)> ProcessPaymentAsync(
        string userId,
        decimal amount,
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PaymentGatewayService");

            var paymentRequest = new
            {
                UserId = userId,
                Amount = amount,
                Currency = request.PaymentCurrency,
                PaymentMethodId = request.PaymentMethodId,
                WalletCurrency = request.WalletCurrency
            };

            var json = JsonSerializer.Serialize(paymentRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/payments/process", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, null, error);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var payment = JsonSerializer.Deserialize<PaymentResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            return (true, payment?.TransactionId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment");
            return (false, null, ex.Message);
        }
    }

    private async Task RollbackOrdersAsync(CheckoutResult result, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MarketplaceService");

            // Rollback Primary Market orders
            foreach (var orderId in result.PrimaryMarketOrderIds)
            {
                try
                {
                    await client.DeleteAsync($"/api/primary/orders/{orderId}", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback Primary Market order {OrderId}", orderId);
                }
            }

            // Rollback Secondary Market escrows
            foreach (var escrowId in result.EscrowIds)
            {
                try
                {
                    await client.DeleteAsync($"/api/secondary/escrow/{escrowId}", cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback escrow {EscrowId}", escrowId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback orders");
        }
    }

    // Response DTOs
    private class OrderResponse
    {
        public string? OrderId { get; set; }
    }

    private class EscrowResponse
    {
        public string? OrderId { get; set; }
        public string? EscrowId { get; set; }
    }

    private class PaymentResponse
    {
        public string? TransactionId { get; set; }
    }
}