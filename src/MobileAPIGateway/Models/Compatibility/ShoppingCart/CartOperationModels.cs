using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.ShoppingCart
{
    public class CreateCartResponse
    {
        [Required]
        public string CartId { get; set; } = string.Empty;
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string CartType { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class CreateCartDataResponse
    {
        public int Status { get; set; } = 200;
        public CreateCartResponse Data { get; set; } = new();
    }

    public class AddToCartRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        public List<CartItemRequest> Items { get; set; } = new();
        
        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class CartItemRequest
    {
        [Required]
        public string Item { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        public bool IsSecondaryMarketItem { get; set; }
    }

    public class AddToCartResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string CartId { get; set; } = string.Empty;
        
        public int ItemsAdded { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class AddToCartDataResponse
    {
        public int Status { get; set; } = 200;
        public AddToCartResponse Data { get; set; } = new();
    }

    public class UpdateCartRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        public List<CartItemRequest> Items { get; set; } = new();
        
        [Required]
        public string Type { get; set; } = string.Empty;
    }

    public class UpdateCartResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string CartId { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class UpdateCartDataResponse
    {
        public int Status { get; set; } = 200;
        public UpdateCartResponse Data { get; set; } = new();
    }

    public class RemoveFromCartResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class RemoveFromCartDataResponse
    {
        public int Status { get; set; } = 200;
        public RemoveFromCartResponse Data { get; set; } = new();
    }

    public class CartItemCountResponse
    {
        public int ItemCount { get; set; }
        
        [Required]
        public string CartId { get; set; } = string.Empty;
    }

    public class CartItemCountDataResponse
    {
        public int Status { get; set; } = 200;
        public CartItemCountResponse Data { get; set; } = new();
    }
}
