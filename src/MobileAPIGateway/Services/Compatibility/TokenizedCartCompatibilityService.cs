//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Compatibility.TokenizedCart;
//using MobileAPIGateway.Models.TokenizedCart;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the tokenized cart compatibility service
///// </summary>
//public class TokenizedCartCompatibilityService : ITokenizedCartCompatibilityService
//{
//    private readonly ITokenizedCartService _tokenizedCartService;
//    private readonly ILogger<TokenizedCartCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="TokenizedCartCompatibilityService"/> class
//    /// </summary>
//    /// <param name="tokenizedCartService">The tokenized cart service</param>
//    /// <param name="logger">The logger</param>
//    public TokenizedCartCompatibilityService(ITokenizedCartService tokenizedCartService, ILogger<TokenizedCartCompatibilityService> logger)
//    {
//        _tokenizedCartService = tokenizedCartService ?? throw new ArgumentNullException(nameof(tokenizedCartService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<CartCreationCompatibilityResponse> CreateCartAsync(CartCreationCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Creating cart for customer ID: {CustomerId}", request.CustomerId);
            
//            // Map to the new model
//            var cartCreationRequest = new CartCreationRequest
//            {
//                CustomerId = request.CustomerId,
//                CartName = request.CartName,
//                CartDescription = request.CartDescription,
//                CurrencyCode = request.CurrencyCode,
//                Metadata = request.Metadata
//            };
            
//            // Map cart items if they exist
//            if (request.CartItems != null && request.CartItems.Any())
//            {
//                cartCreationRequest.CartItems = request.CartItems.Select(item => new TokenizedCartItem
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    ProductImageUrl = item.ProductImageUrl,
//                    ProductPrice = item.ProductPrice,
//                    Quantity = item.Quantity,
//                    CurrencyCode = item.CurrencyCode,
//                    Metadata = item.Metadata
//                }).ToList();
//            }
            
//            // Call the new service
//            var response = await _tokenizedCartService.CreateCartAsync(cartCreationRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new CartCreationCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                CartName = response.CartName,
//                CartDescription = response.CartDescription,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                CurrencyCode = response.CurrencyCode,
//                Status = response.Status,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate,
//                ExpirationDate = response.ExpirationDate
//            };
            
//            // Map cart items if they exist
//            if (response.CartItems != null && response.CartItems.Any())
//            {
//                compatibilityResponse.CartItems = response.CartItems.Select(item => new CartItemCompatibilityResponse
//                {
//                    CartItemId = item.CartItemId,
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    ProductImageUrl = item.ProductImageUrl,
//                    ProductPrice = item.ProductPrice,
//                    Quantity = item.Quantity,
//                    CurrencyCode = item.CurrencyCode,
//                    Metadata = item.Metadata,
//                    CreatedDate = item.CreatedDate,
//                    LastUpdatedDate = item.LastUpdatedDate
//                }).ToList();
//            }
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error creating cart for customer ID: {CustomerId}", request.CustomerId);
            
//            return new CartCreationCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to create cart: " + ex.Message,
//                CustomerId = request.CustomerId
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<CartUpdateCompatibilityResponse> UpdateCartAsync(CartUpdateCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Updating cart ID: {CartId} for customer ID: {CustomerId}", request.CartId, request.CustomerId);
            
//            // Map to the new model
//            var cartUpdateRequest = new CartUpdateRequest
//            {
//                CartId = request.CartId,
//                CustomerId = request.CustomerId,
//                CartName = request.CartName,
//                CartDescription = request.CartDescription,
//                CurrencyCode = request.CurrencyCode,
//                Metadata = request.Metadata
//            };
            
//            // Map cart items if they exist
//            if (request.CartItems != null && request.CartItems.Any())
//            {
//                cartUpdateRequest.CartItems = request.CartItems.Select(item => new TokenizedCartItem
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    ProductImageUrl = item.ProductImageUrl,
//                    ProductPrice = item.ProductPrice,
//                    Quantity = item.Quantity,
//                    CurrencyCode = item.CurrencyCode,
//                    Metadata = item.Metadata
//                }).ToList();
//            }
            
//            // Call the new service
//            var response = await _tokenizedCartService.UpdateCartAsync(cartUpdateRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new CartUpdateCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                CartName = response.CartName,
//                CartDescription = response.CartDescription,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                CurrencyCode = response.CurrencyCode,
//                Status = response.Status,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate,
//                ExpirationDate = response.ExpirationDate
//            };
            
//            // Map cart items if they exist
//            if (response.CartItems != null && response.CartItems.Any())
//            {
//                compatibilityResponse.CartItems = response.CartItems.Select(item => new CartItemCompatibilityResponse
//                {
//                    CartItemId = item.CartItemId,
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    ProductImageUrl = item.ProductImageUrl,
//                    ProductPrice = item.ProductPrice,
//                    Quantity = item.Quantity,
//                    CurrencyCode = item.CurrencyCode,
//                    Metadata = item.Metadata,
//                    CreatedDate = item.CreatedDate,
//                    LastUpdatedDate = item.LastUpdatedDate
//                }).ToList();
//            }
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating cart ID: {CartId} for customer ID: {CustomerId}", request.CartId, request.CustomerId);
            
//            return new CartUpdateCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to update cart: " + ex.Message,
//                CartId = request.CartId,
//                CustomerId = request.CustomerId
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<CartCheckoutCompatibilityResponse> CheckoutCartAsync(CartCheckoutCompatibilityRequest request, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            _logger.LogInformation("Checking out cart ID: {CartId} for customer ID: {CustomerId}", request.CartId, request.CustomerId);
            
//            // Map to the new model
//            var cartCheckoutRequest = new CartCheckoutRequest
//            {
//                CartId = request.CartId,
//                CustomerId = request.CustomerId,
//                PaymentMethodId = request.PaymentMethodId,
//                PaymentMethodType = request.PaymentMethodType,
//                ShippingMethod = request.ShippingMethod,
//                ShippingCost = request.ShippingCost,
//                DiscountCode = request.DiscountCode,
//                Notes = request.Notes,
//                Metadata = request.Metadata,
//                UseSameAddressForBillingAndShipping = request.UseSameAddressForBillingAndShipping
//            };
            
//            // Map billing address if it exists
//            if (request.BillingAddress != null)
//            {
//                cartCheckoutRequest.BillingAddress = new Models.TokenizedCart.Address
//                {
//                    FirstName = request.BillingAddress.FirstName,
//                    LastName = request.BillingAddress.LastName,
//                    CompanyName = request.BillingAddress.CompanyName,
//                    StreetAddress1 = request.BillingAddress.StreetAddress1,
//                    StreetAddress2 = request.BillingAddress.StreetAddress2,
//                    City = request.BillingAddress.City,
//                    StateOrProvince = request.BillingAddress.StateOrProvince,
//                    PostalCode = request.BillingAddress.PostalCode,
//                    Country = request.BillingAddress.Country,
//                    PhoneNumber = request.BillingAddress.PhoneNumber,
//                    EmailAddress = request.BillingAddress.EmailAddress
//                };
//            }
            
//            // Map shipping address if it exists
//            if (request.ShippingAddress != null)
//            {
//                cartCheckoutRequest.ShippingAddress = new Models.TokenizedCart.Address
//                {
//                    FirstName = request.ShippingAddress.FirstName,
//                    LastName = request.ShippingAddress.LastName,
//                    CompanyName = request.ShippingAddress.CompanyName,
//                    StreetAddress1 = request.ShippingAddress.StreetAddress1,
//                    StreetAddress2 = request.ShippingAddress.StreetAddress2,
//                    City = request.ShippingAddress.City,
//                    StateOrProvince = request.ShippingAddress.StateOrProvince,
//                    PostalCode = request.ShippingAddress.PostalCode,
//                    Country = request.ShippingAddress.Country,
//                    PhoneNumber = request.ShippingAddress.PhoneNumber,
//                    EmailAddress = request.ShippingAddress.EmailAddress
//                };
//            }
            
//            // Call the new service
//            var response = await _tokenizedCartService.CheckoutCartAsync(cartCheckoutRequest, cancellationToken);
            
//            // Map to the compatibility response
//            var compatibilityResponse = new CartCheckoutCompatibilityResponse
//            {
//                IsSuccessful = response.IsSuccessful,
//                Message = response.Message,
//                OrderId = response.OrderId,
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                PaymentId = response.PaymentId,
//                PaymentStatus = response.PaymentStatus,
//                PaymentMethodId = response.PaymentMethodId,
//                PaymentMethodType = response.PaymentMethodType,
//                ShippingMethod = response.ShippingMethod,
//                ShippingCost = response.ShippingCost,
//                DiscountCode = response.DiscountCode,
//                DiscountAmount = response.DiscountAmount,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                CurrencyCode = response.CurrencyCode,
//                OrderStatus = response.OrderStatus,
//                Notes = response.Notes,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
            
//            // Map billing address if it exists
//            if (response.BillingAddress != null)
//            {
//                compatibilityResponse.BillingAddress = new AddressCompatibility
//                {
//                    FirstName = response.BillingAddress.FirstName,
//                    LastName = response.BillingAddress.LastName,
//                    CompanyName = response.BillingAddress.CompanyName,
//                    StreetAddress1 = response.BillingAddress.StreetAddress1,
//                    StreetAddress2 = response.BillingAddress.StreetAddress2,
//                    City = response.BillingAddress.City,
//                    StateOrProvince = response.BillingAddress.StateOrProvince,
//                    PostalCode = response.BillingAddress.PostalCode,
//                    Country = response.BillingAddress.Country,
//                    PhoneNumber = response.BillingAddress.PhoneNumber,
//                    EmailAddress = response.BillingAddress.EmailAddress
//                };
//            }
            
//            // Map shipping address if it exists
//            if (response.ShippingAddress != null)
//            {
//                compatibilityResponse.ShippingAddress = new AddressCompatibility
//                {
//                    FirstName = response.ShippingAddress.FirstName,
//                    LastName = response.ShippingAddress.LastName,
//                    CompanyName = response.ShippingAddress.CompanyName,
//                    StreetAddress1 = response.ShippingAddress.StreetAddress1,
//                    StreetAddress2 = response.ShippingAddress.StreetAddress2,
//                    City = response.ShippingAddress.City,
//                    StateOrProvince = response.ShippingAddress.StateOrProvince,
//                    PostalCode = response.ShippingAddress.PostalCode,
//                    Country = response.ShippingAddress.Country,
//                    PhoneNumber = response.ShippingAddress.PhoneNumber,
//                    EmailAddress = response.ShippingAddress.EmailAddress
//                };
//            }
            
//            // Map cart items if they exist
//            if (response.CartItems != null && response.CartItems.Any())
//            {
//                compatibilityResponse.CartItems = response.CartItems.Select(item => new CartItemCompatibilityResponse
//                {
//                    CartItemId = item.CartItemId,
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    ProductImageUrl = item.ProductImageUrl,
//                    ProductPrice = item.ProductPrice,
//                    Quantity = item.Quantity,
//                    CurrencyCode = item.CurrencyCode,
//                    Metadata = item.Metadata,
//                    CreatedDate = item.CreatedDate,
//                    LastUpdatedDate = item.LastUpdatedDate
//                }).ToList();
//            }
            
//            return compatibilityResponse;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error checking out cart ID: {CartId} for customer ID: {CustomerId}", request.CartId, request.CustomerId);
            
//            return new CartCheckoutCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = "Failed to checkout cart: " + ex.Message,
//                CartId = request.CartId,
//                CustomerId = request.CustomerId
//            };
//        }
//    }
//}
