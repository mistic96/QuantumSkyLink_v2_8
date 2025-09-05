using System.ComponentModel.DataAnnotations;

namespace MobileAPIGateway.Models.Compatibility.Global
{
    public class UserLimitsResponse
    {
        public decimal DailyLimit { get; set; }
        
        public decimal MonthlyLimit { get; set; }
        
        public decimal YearlyLimit { get; set; }
        
        public decimal RemainingDaily { get; set; }
        
        public decimal RemainingMonthly { get; set; }
        
        public decimal RemainingYearly { get; set; }
        
        [Required]
        public string Currency { get; set; } = string.Empty;
    }

    public class UserLimitsDataResponse
    {
        public int Status { get; set; } = 200;
        public UserLimitsResponse Data { get; set; } = new();
    }
}
