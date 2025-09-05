using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.ShoppingCart
{
    public class CartItem
    {
        [Required]
        public string ItemId { get; set; } = string.Empty;
        
        [Required]
        public string Item { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        public decimal Price { get; set; }
        
        public decimal TotalValue { get; set; }
        
        public bool IsSecondaryMarketItem { get; set; }
    }

    public class CartResponse
    {
        [Required]
        public string CartId { get; set; } = string.Empty;
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string CartType { get; set; } = string.Empty;
        
        public List<CartItem> Items { get; set; } = new();
        
        public decimal TotalAmount { get; set; }
        
        [Required]
        public string Currency { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime LastModified { get; set; }
    }

    public class CartDataResponse
    {
        public int Status { get; set; } = 200;
        public CartResponse Data { get; set; } = new();
    }
}
