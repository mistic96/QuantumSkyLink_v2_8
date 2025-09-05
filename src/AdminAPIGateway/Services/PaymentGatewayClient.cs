using AdminAPIGateway.Controllers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdminAPIGateway.Services;

/// <summary>
/// HTTP client implementation for communicating with PaymentGatewayService
/// </summary>
public class PaymentGatewayClient : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentGatewayClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    // Deposit Code Review Management
    public async Task<PaginatedResponse<DepositCodeReviewItem>> GetPendingReviewCodesAsync(
        int page, int pageSize, string? sortBy, bool sortDescending, string? filterReason)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}",
                $"sortBy={sortBy ?? "createdAt"}",
                $"sortDescending={sortDescending}"
            };

            if (!string.IsNullOrEmpty(filterReason))
            {
                queryParams.Add($"filterReason={Uri.EscapeDataString(filterReason)}");
            }

            var response = await _httpClient.GetAsync($"api/deposit-codes/pending-review?{string.Join("&", queryParams)}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaginatedResponse<DepositCodeReviewItem>>(content, _jsonOptions) 
                ?? new PaginatedResponse<DepositCodeReviewItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending review codes");
            throw;
        }
    }

    public async Task<DepositCodeReviewDetail?> GetDepositCodeReviewDetailAsync(Guid depositCodeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/deposit-codes/review/{depositCodeId}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepositCodeReviewDetail>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit code review detail for {DepositCodeId}", depositCodeId);
            throw;
        }
    }

    public async Task<DepositCodeApprovalResult> ApproveDepositCodeAsync(Guid depositCodeId, DepositCodeApprovalRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/deposit-codes/review/{depositCodeId}/approve", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepositCodeApprovalResult>(responseContent, _jsonOptions) 
                ?? new DepositCodeApprovalResult { Success = false, ErrorMessage = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving deposit code {DepositCodeId}", depositCodeId);
            throw;
        }
    }

    public async Task<DepositCodeRejectionResult> RejectDepositCodeAsync(Guid depositCodeId, DepositCodeRejectionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/deposit-codes/review/{depositCodeId}/reject", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepositCodeRejectionResult>(responseContent, _jsonOptions) 
                ?? new DepositCodeRejectionResult { Success = false, ErrorMessage = "Failed to deserialize response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting deposit code {DepositCodeId}", depositCodeId);
            throw;
        }
    }

    public async Task<BulkActionResult> BulkDepositCodeActionAsync(BulkDepositCodeActionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/deposit-codes/bulk-action", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BulkActionResult>(responseContent, _jsonOptions) 
                ?? new BulkActionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk action on deposit codes");
            throw;
        }
    }

    // Statistics and Analytics
    public async Task<DuplicateStatistics> GetDuplicateStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var queryParams = $"startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync($"api/deposit-codes/duplicate-statistics?{queryParams}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DuplicateStatistics>(content, _jsonOptions) 
                ?? new DuplicateStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting duplicate statistics");
            throw;
        }
    }

    // Search and Export
    public async Task<PaginatedResponse<DepositCodeSearchResult>> SearchDepositCodesAsync(DepositCodeSearchRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/deposit-codes/search", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PaginatedResponse<DepositCodeSearchResult>>(responseContent, _jsonOptions) 
                ?? new PaginatedResponse<DepositCodeSearchResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching deposit codes");
            throw;
        }
    }

    public async Task<List<DepositCodeAuditEntry>> GetDepositCodeAuditTrailAsync(Guid depositCodeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/deposit-codes/{depositCodeId}/audit-trail");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DepositCodeAuditEntry>>(content, _jsonOptions) 
                ?? new List<DepositCodeAuditEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit trail for deposit code {DepositCodeId}", depositCodeId);
            throw;
        }
    }

    public async Task<byte[]> ExportDepositCodesAsync(DepositCodeExportRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/deposit-codes/export", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting deposit codes");
            throw;
        }
    }

    // Monitoring endpoints (existing)
    public async Task<DepositCodeMetrics> GetDepositCodeMetricsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var url = "api/monitoring/deposit-codes/metrics";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DepositCodeMetrics>(content, _jsonOptions) 
                ?? new DepositCodeMetrics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit code metrics");
            throw;
        }
    }

    public async Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var url = "api/monitoring/deposit-codes/security";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SecurityEvent>>(content, _jsonOptions) 
                ?? new List<SecurityEvent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security events");
            throw;
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var queryParams = new List<string>();
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            var url = "api/monitoring/deposit-codes/performance";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PerformanceMetrics>(content, _jsonOptions) 
                ?? new PerformanceMetrics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            throw;
        }
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/monitoring/deposit-codes/dashboard");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<DashboardData>(content, _jsonOptions) 
                ?? new DashboardData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            throw;
        }
    }

    public async Task<HealthCheckResult> GetHealthStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/monitoring/deposit-codes/health");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<HealthCheckResult>(content, _jsonOptions) 
                ?? new HealthCheckResult { Status = "Unknown" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            throw;
        }
    }
}