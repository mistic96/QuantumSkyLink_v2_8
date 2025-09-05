using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.ShoppingCart
{
    public class CheckoutCodeResponse
    {
        [Required]
        public string CheckoutCode { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
    }

    public class CheckoutCodeDataResponse
    {
        public int Status { get; set; } = 200;
        public CheckoutCodeResponse Data { get; set; } = new();
    }

    public class PostCheckoutRequest
    {
        [Required]
        public string CheckoutCode { get; set; } = string.Empty;
        
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        
        public PaymentDetails PaymentDetails { get; set; } = new();
    }

    public class PaymentDetails
    {
        public string Cryptocurrency { get; set; } = string.Empty;
        
        public string WalletAddress { get; set; } = string.Empty;
    }

    public class PaymentInstructions
    {
        public decimal Amount { get; set; }
        
        [Required]
        public string Cryptocurrency { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
    }

    public class PostCheckoutResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string OrderId { get; set; } = string.Empty;
        
        public bool PaymentRequired { get; set; }
        
        public PaymentInstructions PaymentInstructions { get; set; } = new();
    }

    public class PostCheckoutDataResponse
    {
        public int Status { get; set; } = 200;
        public PostCheckoutResponse Data { get; set; } = new();
    }

    public class UpdatePaymentTypeResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class UpdatePaymentTypeDataResponse
    {
        public int Status { get; set; } = 200;
        public UpdatePaymentTypeResponse Data { get; set; } = new();
    }

    public class CompleteCheckoutResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string OrderId { get; set; } = string.Empty;
        
        [Required]
        public string TransactionId { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class CompleteCheckoutDataResponse
    {
        public int Status { get; set; } = 200;
        public CompleteCheckoutResponse Data { get; set; } = new();
    }

    public class CancelSaleResponse
    {
        public bool Success { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }

    public class CancelSaleDataResponse
    {
        public int Status { get; set; } = 200;
        public CancelSaleResponse Data { get; set; } = new();
    }
}
