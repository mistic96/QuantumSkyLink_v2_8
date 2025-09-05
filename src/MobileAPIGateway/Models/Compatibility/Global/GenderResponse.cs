using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class GenderResponse
    {
        [Required]
        public string GenderCode { get; set; } = string.Empty;
        
        [Required]
        public string GenderName { get; set; } = string.Empty;
    }

    public class GendersListResponse
    {
        public int Status { get; set; } = 200;
        public List<GenderResponse> Data { get; set; } = new();
    }
}
