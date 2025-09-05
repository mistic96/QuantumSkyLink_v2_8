using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class LanguageResponse
    {
        [Required]
        public string LanguageCode { get; set; } = string.Empty;
        
        [Required]
        public string LanguageName { get; set; } = string.Empty;
        
        public bool IsDefault { get; set; }
    }

    public class LanguagesListResponse
    {
        public int Status { get; set; } = 200;
        public List<LanguageResponse> Data { get; set; } = new();
    }
}
