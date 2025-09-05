using MobileAPIGateway.Models.Compatibility.ShoppingCart;

namespace MobileAPIGateway.Services.Compatibility
{
    public interface IShoppingCartCompatibilityService
    {
        // CloudCart APIs
        Task<CartDataResponse> GetCartAsync(string email, string cartType, CancellationToken cancellationToken = default);
        Task<CreateCartDataResponse> CreateCartAsync(string email, string cartType, CancellationToken cancellationToken = default);
        Task<AddToCartDataResponse> AddToCartAsync(string email, string cartType, AddToCartRequest request, CancellationToken cancellationToken = default);
        Task<UpdateCartDataResponse> UpdateCartAsync(UpdateCartRequest request, CancellationToken cancellationToken = default);
        Task<RemoveFromCartDataResponse> RemoveFromCartAsync(string email, string cartType, string item, CancellationToken cancellationToken = default);
        Task<CartItemCountDataResponse> GetCartItemCountAsync(string email, string cartType, CancellationToken cancellationToken = default);
        Task<CheckoutCodeDataResponse> GetCheckoutCodeAsync(string email, string cartType, string type, CancellationToken cancellationToken = default);
        Task<PostCheckoutDataResponse> PostCheckoutAsync(PostCheckoutRequest request, CancellationToken cancellationToken = default);

        // ShoppingCart APIs
        Task<UpdatePaymentTypeDataResponse> UpdateRequestedPaymentTypeByCheckoutIdAsync(string emailAddress, string clientIpAddress, string checkoutId, CancellationToken cancellationToken = default);
        Task<CompleteCheckoutDataResponse> PostCompleteCheckoutAsync(string emailAddress, string clientIpAddress, string cartId, string cartType, string authCode, CancellationToken cancellationToken = default);
        Task<CancelSaleDataResponse> CancelPendingSaleByCheckoutIdAsync(string emailAddress, bool useForceCancel, string checkoutCode, CancellationToken cancellationToken = default);
    }
}
