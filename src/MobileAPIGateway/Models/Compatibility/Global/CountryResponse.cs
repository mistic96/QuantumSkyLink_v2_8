using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class CountryResponse
    {
        [Required]
        public string CountryCode { get; set; } = string.Empty;
        
        [Required]
        public string CountryName { get; set; } = string.Empty;
        
        public bool IsSupported { get; set; }
        
        public string Currency { get; set; } = string.Empty;
    }

    public class CountriesListResponse
    {
        public int Status { get; set; } = 200;
        public List<CountryResponse> Data { get; set; } = new();
    }
}
