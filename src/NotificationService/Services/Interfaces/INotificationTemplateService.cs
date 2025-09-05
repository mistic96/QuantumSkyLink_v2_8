using NotificationService.Models.Requests;
using NotificationService.Models.Responses;

namespace NotificationService.Services.Interfaces;

public interface INotificationTemplateService
{
    // Template CRUD Operations
    Task<NotificationTemplateResponse> CreateTemplateAsync(CreateNotificationTemplateRequest request);
    Task<NotificationTemplateResponse?> GetTemplateAsync(Guid templateId);
    Task<NotificationTemplateResponse?> GetTemplateByNameAsync(string name);
    Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesAsync(GetNotificationTemplatesRequest request);
    Task<NotificationTemplateResponse?> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateRequest request);
    Task<bool> DeleteTemplateAsync(Guid templateId);
    Task<bool> ActivateTemplateAsync(Guid templateId);
    Task<bool> DeactivateTemplateAsync(Guid templateId);

    // Template Processing
    Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, object> variables);
    Task<(string subject, string body, string? htmlBody)> RenderTemplateContentAsync(Guid templateId, Dictionary<string, object> variables);
    Task<string> RenderTemplateSubjectAsync(Guid templateId, Dictionary<string, object> variables);
    Task<string> RenderTemplateBodyAsync(Guid templateId, Dictionary<string, object> variables);
    Task<string?> RenderTemplateHtmlBodyAsync(Guid templateId, Dictionary<string, object> variables);

    // Template Validation
    Task<TemplateValidationResponse> ValidateTemplateAsync(Guid templateId, Dictionary<string, object>? variables = null);
    Task<TemplateValidationResponse> ValidateTemplateContentAsync(string subject, string body, string? htmlBody, List<string> variables, Dictionary<string, object>? testVariables = null);
    Task<List<string>> ExtractVariablesFromTemplateAsync(string content);
    Task<bool> IsTemplateValidAsync(Guid templateId);

    // Template Categories and Types
    Task<List<string>> GetAvailableTypesAsync();
    Task<List<string>> GetAvailableCategoriesAsync();
    Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesByTypeAsync(string type, int page = 1, int pageSize = 20);
    Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesByCategoryAsync(string category, int page = 1, int pageSize = 20);

    // Template Analytics
    Task<Dictionary<string, int>> GetTemplateUsageStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTemplateUsageCountAsync(Guid templateId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<NotificationTemplateResponse>> GetMostUsedTemplatesAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<NotificationTemplateResponse>> GetUnusedTemplatesAsync(DateTime? sinceDate = null);

    // Template Import/Export
    Task<NotificationTemplateResponse> ImportTemplateAsync(string templateJson);
    Task<string> ExportTemplateAsync(Guid templateId);
    Task<List<NotificationTemplateResponse>> ImportTemplatesAsync(List<string> templateJsonList);
    Task<List<string>> ExportAllTemplatesAsync();
}
