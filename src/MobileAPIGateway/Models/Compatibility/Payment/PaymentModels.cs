using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Payment
{
    public class PaymentRequestResponse
    {
        [Required]
        public string PaymentRequestId { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        [Required]
        public string Currency { get; set; } = string.Empty;
        
        public List<string> PaymentMethods { get; set; } = new();
        
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentRequestDataResponse
    {
        public int Status { get; set; } = 200;
        public PaymentRequestResponse Data { get; set; } = new();
    }

    public class CryptoChargeResponse
    {
        [Required]
        public string ChargeId { get; set; } = string.Empty;
        
        [Required]
        public string Cryptocurrency { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public string QrCode { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
    }

    public class CryptoChargeDataResponse
    {
        public int Status { get; set; } = 200;
        public CryptoChargeResponse Data { get; set; } = new();
    }
}
