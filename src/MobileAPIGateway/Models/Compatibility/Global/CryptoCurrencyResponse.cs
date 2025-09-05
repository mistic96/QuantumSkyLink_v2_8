using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class CryptoCurrencyResponse
    {
        [Required]
        public string Symbol { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public decimal MinAmount { get; set; }
        
        public decimal MaxAmount { get; set; }
        
        public int Decimals { get; set; }
    }

    public class CryptoCurrenciesListResponse
    {
        public int Status { get; set; } = 200;
        public List<CryptoCurrencyResponse> Data { get; set; } = new();
    }
}
