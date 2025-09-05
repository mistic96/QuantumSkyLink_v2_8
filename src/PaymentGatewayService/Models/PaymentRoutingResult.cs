using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models;

public class PaymentRoutingResult
{
    public bool IsSuccessful { get; set; }
    public bool Success { get; set; }
    public PaymentProvider? SelectedProvider { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RecommendedAction { get; set; }
    public Dictionary<string, string>? RoutingMetadata { get; set; }
    public decimal? EstimatedFee { get; set; }
    public int? EstimatedProcessingTimeMinutes { get; set; }
    public string? RoutingReason { get; set; }
    public List<string>? AlternativeProviders { get; set; }
    public bool RequiresAdditionalVerification { get; set; } = false;
    public DateTime RoutedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties used in PaymentRouter
    public string? PaymentId { get; set; }
    public string? ExternalReference { get; set; }
    public string? RedirectUrl { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}
