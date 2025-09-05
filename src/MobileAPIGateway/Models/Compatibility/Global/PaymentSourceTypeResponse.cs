using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class PaymentSourceTypeResponse
    {
        [Required]
        public string TypeId { get; set; } = string.Empty;
        
        [Required]
        public string TypeName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public List<string> SupportedCurrencies { get; set; } = new();
    }

    public class PaymentSourceTypesListResponse
    {
        public int Status { get; set; } = 200;
        public List<PaymentSourceTypeResponse> Data { get; set; } = new();
    }
}
