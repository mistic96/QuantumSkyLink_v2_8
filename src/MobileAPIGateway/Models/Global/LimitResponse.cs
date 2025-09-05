namespace MobileAPIGateway.Models.Global;

/// <summary>
/// Represents a response containing user limits
/// </summary>
public class LimitResponse
{
    /// <summary>
    /// Gets or sets the daily limit
    /// </summary>
    public decimal DailyLimit { get; set; }
    
    /// <summary>
    /// Gets or sets the weekly limit
    /// </summary>
    public decimal WeeklyLimit { get; set; }
    
    /// <summary>
    /// Gets or sets the monthly limit
    /// </summary>
    public decimal MonthlyLimit { get; set; }
    
    /// <summary>
    /// Gets or sets the daily usage
    /// </summary>
    public decimal DailyUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the weekly usage
    /// </summary>
    public decimal WeeklyUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the monthly usage
    /// </summary>
    public decimal MonthlyUsage { get; set; }
    
    /// <summary>
    /// Gets or sets the remaining daily limit
    /// </summary>
    public decimal RemainingDailyLimit => DailyLimit - DailyUsage;
    
    /// <summary>
    /// Gets or sets the remaining weekly limit
    /// </summary>
    public decimal RemainingWeeklyLimit => WeeklyLimit - WeeklyUsage;
    
    /// <summary>
    /// Gets or sets the remaining monthly limit
    /// </summary>
    public decimal RemainingMonthlyLimit => MonthlyLimit - MonthlyUsage;
}
