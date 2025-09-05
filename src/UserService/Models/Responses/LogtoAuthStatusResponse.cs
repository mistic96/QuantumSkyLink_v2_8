using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Responses;

public class LogtoAuthStatusResponse
{
    public bool IsAuthenticated { get; set; }
    
    public bool IsLogtoConnected { get; set; }
    
    [StringLength(100)]
    public string? LogtoVersion { get; set; }
    
    [StringLength(200)]
    public string? LogtoEndpoint { get; set; }
    
    [StringLength(100)]
    public string? ApplicationId { get; set; }
    
    public int TotalUsers { get; set; }
    
    public int ActiveUsers { get; set; }
    
    public DateTime LastSyncAt { get; set; }
    
    [StringLength(50)]
    public string Status { get; set; } = string.Empty; // Connected, Disconnected, Error
    
    [StringLength(500)]
    public string? StatusMessage { get; set; }
    
    public bool HealthCheckPassed { get; set; }
    
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}
