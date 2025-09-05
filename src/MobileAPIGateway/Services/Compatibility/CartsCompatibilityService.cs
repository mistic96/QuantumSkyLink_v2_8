//using Microsoft.Extensions.Logging;
//using MobileAPIGateway.Models.Carts;
//using MobileAPIGateway.Models.Compatibility.Carts;

//namespace MobileAPIGateway.Services.Compatibility;

///// <summary>
///// Implementation of the carts compatibility service
///// </summary>
//public class CartsCompatibilityService : ICartsCompatibilityService
//{
//    private readonly ICartsService _cartsService;
//    private readonly ILogger<CartsCompatibilityService> _logger;
    
//    /// <summary>
//    /// Initializes a new instance of the <see cref="CartsCompatibilityService"/> class
//    /// </summary>
//    /// <param name="cartsService">The carts service</param>
//    /// <param name="logger">The logger</param>
//    public CartsCompatibilityService(ICartsService cartsService, ILogger<CartsCompatibilityService> logger)
//    {
//        _cartsService = cartsService ?? throw new ArgumentNullException(nameof(cartsService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }
    
//    /// <inheritdoc />
//    public async Task<CreateCartCompatibilityResponse> CreateCartAsync(CreateCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Creating cart for customer {CustomerId}", request.CustomerId);
            
//            // Map from compatibility request to new request model
//            var createCartRequest = new CreateCartRequest
//            {
//                CustomerId = request.CustomerId,
//                CartName = request.CartName,
//                CartDescription = request.CartDescription,
//                Items = request.Items?.Select(item => new CartItem
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                Metadata = request.Metadata
//            };
            
//            // Call the new service
//            var response = await _cartsService.CreateCartAsync(createCartRequest);
            
//            // Map from new response model to compatibility response
//            return new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Cart created successfully",
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                CartName = response.CartName,
//                CartDescription = response.CartDescription,
//                Items = response.Items?.Select(item => new CartItemCompatibility
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                Status = response.Status,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                Currency = response.Currency,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error creating cart for customer {CustomerId}", request.CustomerId);
//            return new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error creating cart: {ex.Message}"
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<UpdateCartCompatibilityResponse> UpdateCartAsync(UpdateCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Updating cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
            
//            // Map from compatibility request to new request model
//            var updateCartRequest = new UpdateCartRequest
//            {
//                CartId = request.CartId,
//                CustomerId = request.CustomerId,
//                CartName = request.CartName,
//                CartDescription = request.CartDescription,
//                Items = request.Items?.Select(item => new CartItem
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                Metadata = request.Metadata
//            };
            
//            // Call the new service
//            var response = await _cartsService.UpdateCartAsync(updateCartRequest);
            
//            // Map from new response model to compatibility response
//            return new UpdateCartCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Cart updated successfully",
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                CartName = response.CartName,
//                CartDescription = response.CartDescription,
//                Items = response.Items?.Select(item => new CartItemCompatibility
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                Status = response.Status,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                Currency = response.Currency,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error updating cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
//            return new UpdateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error updating cart: {ex.Message}"
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<CheckoutCartCompatibilityResponse> CheckoutCartAsync(CheckoutCartCompatibilityRequest request)
//    {
//        try
//        {
//            _logger.LogInformation("Checking out cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
            
//            // Map from compatibility request to new request model
//            var checkoutCartRequest = new CheckoutCartRequest
//            {
//                CartId = request.CartId,
//                CustomerId = request.CustomerId,
//                PaymentMethodId = request.PaymentMethodId,
//                PaymentMethodType = request.PaymentMethodType,
//                ShippingAddress = request.ShippingAddress != null ? new ShippingAddress
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
//                } : null,
//                BillingAddress = request.BillingAddress != null ? new BillingAddress
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
//                } : null,
//                Metadata = request.Metadata
//            };
            
//            // Call the new service
//            var response = await _cartsService.CheckoutCartAsync(checkoutCartRequest);
            
//            // Map from new response model to compatibility response
//            return new CheckoutCartCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Cart checked out successfully",
//                OrderId = response.OrderId,
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                PaymentMethodId = response.PaymentMethodId,
//                PaymentMethodType = response.PaymentMethodType,
//                PaymentStatus = response.PaymentStatus,
//                OrderStatus = response.OrderStatus,
//                Items = response.Items?.Select(item => new CartItemCompatibility
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                ShippingAddress = response.ShippingAddress != null ? new ShippingAddressCompatibility
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
//                } : null,
//                BillingAddress = response.BillingAddress != null ? new BillingAddressCompatibility
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
//                } : null,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                ShippingCost = response.ShippingCost,
//                Total = response.Total,
//                Currency = response.Currency,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate,
//                PaymentTransactionId = response.PaymentTransactionId,
//                PaymentReceiptUrl = response.PaymentReceiptUrl
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error checking out cart {CartId} for customer {CustomerId}", request.CartId, request.CustomerId);
//            return new CheckoutCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error checking out cart: {ex.Message}"
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<CreateCartCompatibilityResponse> GetCartAsync(string cartId, string customerId)
//    {
//        try
//        {
//            _logger.LogInformation("Getting cart {CartId} for customer {CustomerId}", cartId, customerId);
            
//            // Call the new service
//            var response = await _cartsService.GetCartAsync(cartId, customerId);
            
//            // Map from new response model to compatibility response
//            return new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = true,
//                Message = "Cart retrieved successfully",
//                CartId = response.CartId,
//                CustomerId = response.CustomerId,
//                CartName = response.CartName,
//                CartDescription = response.CartDescription,
//                Items = response.Items?.Select(item => new CartItemCompatibility
//                {
//                    ProductId = item.ProductId,
//                    ProductName = item.ProductName,
//                    ProductDescription = item.ProductDescription,
//                    Quantity = item.Quantity,
//                    UnitPrice = item.UnitPrice,
//                    Currency = item.Currency,
//                    Metadata = item.Metadata
//                }).ToList(),
//                Status = response.Status,
//                Subtotal = response.Subtotal,
//                Tax = response.Tax,
//                Total = response.Total,
//                Currency = response.Currency,
//                Metadata = response.Metadata,
//                CreatedDate = response.CreatedDate,
//                LastUpdatedDate = response.LastUpdatedDate
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error getting cart {CartId} for customer {CustomerId}", cartId, customerId);
//            return new CreateCartCompatibilityResponse
//            {
//                IsSuccessful = false,
//                Message = $"Error getting cart: {ex.Message}"
//            };
//        }
//    }
    
//    /// <inheritdoc />
//    public async Task<bool> DeleteCartAsync(string cartId, string customerId)
//    {
//        try
//        {
//            _logger.LogInformation("Deleting cart {CartId} for customer {CustomerId}", cartId, customerId);
            
//            // Call the new service
//            return await _cartsService.DeleteCartAsync(cartId, customerId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error deleting cart {CartId} for customer {CustomerId}", cartId, customerId);
//            return false;
//        }
//    }
//}
