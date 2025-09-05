using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using NotificationService.Data;
using NotificationService.Data.Entities;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;
using Mapster;

namespace NotificationService.Services;

public class NotificationTemplateService : INotificationTemplateService
{
    private readonly NotificationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<NotificationTemplateService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
    private const string TEMPLATE_CACHE_PREFIX = "template:";
    private const string TEMPLATE_LIST_CACHE_KEY = "templates:list";
    private const string TEMPLATE_STATS_CACHE_KEY = "templates:stats";

    public NotificationTemplateService(
        NotificationDbContext context,
        IDistributedCache cache,
        ILogger<NotificationTemplateService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<NotificationTemplateResponse> CreateTemplateAsync(CreateNotificationTemplateRequest request)
    {
        try
        {
            _logger.LogInformation("Creating notification template: {Name}", request.Name);

            // Validate template syntax
            var validationResult = await ValidateTemplateContentAsync(
                request.Subject, request.Body, request.HtmlBody, request.Variables ?? new List<string>());
            
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Check for duplicate name
            var existingTemplate = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == request.Name);

            if (existingTemplate != null)
            {
                throw new InvalidOperationException("A template with this name already exists");
            }

            var template = new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Category = request.Category,
                Subject = request.Subject,
                Body = request.Body,
                HtmlBody = request.HtmlBody,
                Variables = JsonSerializer.Serialize(request.Variables ?? new List<string>()),
                Priority = request.Priority,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateTemplateCacheAsync();

            _logger.LogInformation("Template created successfully: {TemplateId}", template.Id);

            return template.Adapt<NotificationTemplateResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template: {Name}", request.Name);
            throw;
        }
    }

    public async Task<NotificationTemplateResponse?> GetTemplateAsync(Guid templateId)
    {
        try
        {
            _logger.LogInformation("Retrieving template: {TemplateId}", templateId);

            // Try cache first
            var cacheKey = $"{TEMPLATE_CACHE_PREFIX}{templateId}";
            var cachedTemplate = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedTemplate))
            {
                var template = JsonSerializer.Deserialize<NotificationTemplate>(cachedTemplate);
                return template?.Adapt<NotificationTemplateResponse>();
            }

            // Get from database
            var dbTemplate = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (dbTemplate == null)
            {
                return null;
            }

            // Cache the template
            var templateJson = JsonSerializer.Serialize(dbTemplate);
            await _cache.SetStringAsync(cacheKey, templateJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });

            return dbTemplate.Adapt<NotificationTemplateResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<NotificationTemplateResponse?> GetTemplateByNameAsync(string name)
    {
        try
        {
            _logger.LogInformation("Retrieving template by name: {Name}", name);

            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == name);

            return template?.Adapt<NotificationTemplateResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template by name: {Name}", name);
            throw;
        }
    }

    public async Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesAsync(GetNotificationTemplatesRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving templates with filters");

            var query = _context.NotificationTemplates.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(t => t.Type == request.Type);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(t => t.Category == request.Category);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == request.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDirection?.ToLower() == "desc" 
                    ? query.OrderByDescending(t => t.Name)
                    : query.OrderBy(t => t.Name),
                "type" => request.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.Type)
                    : query.OrderBy(t => t.Type),
                "createdat" => request.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderBy(t => t.Name)
            };

            // Apply pagination
            var templates = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var templateResponses = templates.Adapt<List<NotificationTemplateResponse>>();

            return new PagedResponse<NotificationTemplateResponse>
            {
                Data = templateResponses,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasNextPage = request.Page < (int)Math.Ceiling((double)totalCount / request.PageSize),
                HasPreviousPage = request.Page > 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            throw;
        }
    }

    public async Task<NotificationTemplateResponse?> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating template: {TemplateId}", templateId);

            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                return null;
            }

            // Validate template syntax if content is being updated
            if (!string.IsNullOrEmpty(request.Body) || !string.IsNullOrEmpty(request.HtmlBody) || !string.IsNullOrEmpty(request.Subject))
            {
                var validationResult = await ValidateTemplateContentAsync(
                    request.Subject ?? template.Subject,
                    request.Body ?? template.Body,
                    request.HtmlBody ?? template.HtmlBody,
                    request.Variables ?? JsonSerializer.Deserialize<List<string>>(template.Variables) ?? new List<string>());
                
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationResult.Errors)}");
                }
            }

            // Check for duplicate name if name is being changed
            if (!string.IsNullOrEmpty(request.Name) && request.Name != template.Name)
            {
                var existingTemplate = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != templateId);

                if (existingTemplate != null)
                {
                    throw new InvalidOperationException("A template with this name already exists");
                }
            }

            // Update fields
            if (!string.IsNullOrEmpty(request.Name))
                template.Name = request.Name;
            if (request.Description != null)
                template.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Subject))
                template.Subject = request.Subject;
            if (!string.IsNullOrEmpty(request.Body))
                template.Body = request.Body;
            if (request.HtmlBody != null)
                template.HtmlBody = request.HtmlBody;
            if (request.Variables != null)
                template.Variables = JsonSerializer.Serialize(request.Variables);
            if (request.IsActive.HasValue)
                template.IsActive = request.IsActive.Value;
            if (!string.IsNullOrEmpty(request.Category))
                template.Category = request.Category;
            if (!string.IsNullOrEmpty(request.Priority))
                template.Priority = request.Priority;

            template.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateTemplateCacheAsync();
            await _cache.RemoveAsync($"{TEMPLATE_CACHE_PREFIX}{templateId}");

            _logger.LogInformation("Template updated successfully: {TemplateId}", templateId);

            return template.Adapt<NotificationTemplateResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId)
    {
        try
        {
            _logger.LogInformation("Deleting template: {TemplateId}", templateId);

            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
            {
                return false;
            }

            // Check if template is in use
            var isInUse = await _context.Notifications
                .AnyAsync(n => n.TemplateId == templateId);

            if (isInUse)
            {
                throw new InvalidOperationException("Cannot delete template that is currently in use");
            }

            // Hard delete since there's no IsDeleted property
            _context.NotificationTemplates.Remove(template);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await InvalidateTemplateCacheAsync();
            await _cache.RemoveAsync($"{TEMPLATE_CACHE_PREFIX}{templateId}");

            _logger.LogInformation("Template deleted successfully: {TemplateId}", templateId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> ActivateTemplateAsync(Guid templateId)
    {
        try
        {
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return false;

            template.IsActive = true;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"{TEMPLATE_CACHE_PREFIX}{templateId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> DeactivateTemplateAsync(Guid templateId)
    {
        try
        {
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                return false;

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _cache.RemoveAsync($"{TEMPLATE_CACHE_PREFIX}{templateId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<string> RenderTemplateAsync(Guid templateId, Dictionary<string, object> variables)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return string.Empty;

            return RenderContent(template.Body, variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<(string subject, string body, string? htmlBody)> RenderTemplateContentAsync(Guid templateId, Dictionary<string, object> variables)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return (string.Empty, string.Empty, null);

            var renderedSubject = RenderContent(template.Subject, variables);
            var renderedBody = RenderContent(template.Body, variables);
            var renderedHtmlBody = !string.IsNullOrEmpty(template.HtmlBody) 
                ? RenderContent(template.HtmlBody, variables) 
                : null;

            return (renderedSubject, renderedBody, renderedHtmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template content: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<string> RenderTemplateSubjectAsync(Guid templateId, Dictionary<string, object> variables)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return string.Empty;

            return RenderContent(template.Subject, variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template subject: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<string> RenderTemplateBodyAsync(Guid templateId, Dictionary<string, object> variables)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
                return string.Empty;

            return RenderContent(template.Body, variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template body: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<string?> RenderTemplateHtmlBodyAsync(Guid templateId, Dictionary<string, object> variables)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null || string.IsNullOrEmpty(template.HtmlBody))
                return null;

            return RenderContent(template.HtmlBody, variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template HTML body: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<TemplateValidationResponse> ValidateTemplateAsync(Guid templateId, Dictionary<string, object>? variables = null)
    {
        try
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                return new TemplateValidationResponse
                {
                    IsValid = false,
                    Errors = new List<string> { "Template not found" }
                };
            }

            return await ValidateTemplateContentAsync(template.Subject, template.Body, template.HtmlBody, template.Variables, variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<TemplateValidationResponse> ValidateTemplateContentAsync(string subject, string body, string? htmlBody, List<string> variables, Dictionary<string, object>? testVariables = null)
    {
        try
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var missingVariables = new List<string>();
            var unusedVariables = new List<string>();

            // Validate content
            if (string.IsNullOrEmpty(subject))
                errors.Add("Subject cannot be empty");
            if (string.IsNullOrEmpty(body))
                errors.Add("Body cannot be empty");

            // Extract variables from content
            var subjectVariables = ExtractVariables(subject);
            var bodyVariables = ExtractVariables(body);
            var htmlVariables = !string.IsNullOrEmpty(htmlBody) ? ExtractVariables(htmlBody) : new List<string>();

            var allContentVariables = subjectVariables.Union(bodyVariables).Union(htmlVariables).Distinct().ToList();

            // Check for missing variables
            foreach (var variable in allContentVariables)
            {
                if (!variables.Contains(variable))
                {
                    missingVariables.Add(variable);
                }
            }

            // Check for unused variables
            foreach (var variable in variables)
            {
                if (!allContentVariables.Contains(variable))
                {
                    unusedVariables.Add(variable);
                }
            }

            // Validate HTML if provided
            if (!string.IsNullOrEmpty(htmlBody) && !IsValidHtml(htmlBody))
            {
                errors.Add("Invalid HTML structure");
            }

            // Test variable substitution if test variables provided
            if (testVariables != null)
            {
                foreach (var variable in allContentVariables)
                {
                    if (!testVariables.ContainsKey(variable))
                    {
                        warnings.Add($"Test variable '{variable}' not provided");
                    }
                }
            }

            return new TemplateValidationResponse
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings,
                MissingVariables = missingVariables,
                UnusedVariables = unusedVariables
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template content");
            throw;
        }
    }

    public async Task<List<string>> ExtractVariablesFromTemplateAsync(string content)
    {
        return await Task.FromResult(ExtractVariables(content));
    }

    public async Task<bool> IsTemplateValidAsync(Guid templateId)
    {
        try
        {
            var validationResult = await ValidateTemplateAsync(templateId);
            return validationResult.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking template validity: {TemplateId}", templateId);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableTypesAsync()
    {
        try
        {
            return await _context.NotificationTemplates
                .Select(t => t.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available types");
            throw;
        }
    }

    public async Task<List<string>> GetAvailableCategoriesAsync()
    {
        try
        {
            return await _context.NotificationTemplates
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available categories");
            throw;
        }
    }

    public async Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesByTypeAsync(string type, int page = 1, int pageSize = 20)
    {
        var request = new GetNotificationTemplatesRequest
        {
            Type = type,
            Page = page,
            PageSize = pageSize
        };
        return await GetTemplatesAsync(request);
    }

    public async Task<PagedResponse<NotificationTemplateResponse>> GetTemplatesByCategoryAsync(string category, int page = 1, int pageSize = 20)
    {
        var request = new GetNotificationTemplatesRequest
        {
            Category = category,
            Page = page,
            PageSize = pageSize
        };
        return await GetTemplatesAsync(request);
    }

    public async Task<Dictionary<string, int>> GetTemplateUsageStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            var stats = await query
                .Where(n => n.TemplateId.HasValue)
                .Join(_context.NotificationTemplates, n => n.TemplateId, t => t.Id, (n, t) => t.Name)
                .GroupBy(name => name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Name, x => x.Count);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template usage stats");
            throw;
        }
    }

    public async Task<int> GetTemplateUsageCountAsync(Guid templateId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Notifications
                .Where(n => n.TemplateId == templateId);

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template usage count: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<List<NotificationTemplateResponse>> GetMostUsedTemplatesAsync(int count = 10, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Notifications.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(n => n.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(n => n.CreatedAt <= toDate.Value);

            var mostUsedTemplateIds = await query
                .Where(n => n.TemplateId.HasValue)
                .GroupBy(n => n.TemplateId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key!.Value)
                .ToListAsync();

            var templates = await _context.NotificationTemplates
                .Where(t => mostUsedTemplateIds.Contains(t.Id))
                .ToListAsync();

            return templates.Adapt<List<NotificationTemplateResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most used templates");
            throw;
        }
    }

    public async Task<List<NotificationTemplateResponse>> GetUnusedTemplatesAsync(DateTime? sinceDate = null)
    {
        try
        {
            var usedTemplateIds = _context.Notifications
                .Where(n => n.TemplateId.HasValue);

            if (sinceDate.HasValue)
                usedTemplateIds = usedTemplateIds.Where(n => n.CreatedAt >= sinceDate.Value);

            var usedIds = await usedTemplateIds
                .Select(n => n.TemplateId!.Value)
                .Distinct()
                .ToListAsync();

            var unusedTemplates = await _context.NotificationTemplates
                .Where(t => !usedIds.Contains(t.Id))
                .ToListAsync();

            return unusedTemplates.Adapt<List<NotificationTemplateResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused templates");
            throw;
        }
    }

    public async Task<NotificationTemplateResponse> ImportTemplateAsync(string templateJson)
    {
        try
        {
            var templateData = JsonSerializer.Deserialize<NotificationTemplate>(templateJson);
            if (templateData == null)
                throw new InvalidOperationException("Invalid template JSON");

            // Generate new ID to avoid conflicts
            templateData.Id = Guid.NewGuid();
            templateData.CreatedAt = DateTime.UtcNow;
            templateData.UpdatedAt = DateTime.UtcNow;

            // Check for name conflicts
            var existingTemplate = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == templateData.Name);

            if (existingTemplate != null)
            {
                templateData.Name = $"{templateData.Name}_imported_{DateTime.UtcNow:yyyyMMddHHmmss}";
            }

            _context.NotificationTemplates.Add(templateData);
            await _context.SaveChangesAsync();

            await InvalidateTemplateCacheAsync();

            return templateData.Adapt<NotificationTemplateResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing template");
            throw;
        }
    }

    public async Task<string> ExportTemplateAsync(Guid templateId)
    {
        try
        {
            var template = await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == templateId);

            if (template == null)
                throw new InvalidOperationException("Template not found");

            return JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<List<NotificationTemplateResponse>> ImportTemplatesAsync(List<string> templateJsonList)
    {
        var results = new List<NotificationTemplateResponse>();

        foreach (var templateJson in templateJsonList)
        {
            try
            {
                var result = await ImportTemplateAsync(templateJson);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to import template from JSON");
                // Continue with other templates
            }
        }

        return results;
    }

    public async Task<List<string>> ExportAllTemplatesAsync()
    {
        try
        {
            var templates = await _context.NotificationTemplates.ToListAsync();
            var results = new List<string>();

            foreach (var template in templates)
            {
                var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
                results.Add(json);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting all templates");
            throw;
        }
    }

    private List<string> ExtractVariables(string content)
    {
        if (string.IsNullOrEmpty(content))
            return new List<string>();

        var variables = new List<string>();
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                variables.Add(match.Groups[1].Value);
            }
        }

        return variables.Distinct().ToList();
    }

    private bool IsValidHtml(string html)
    {
        try
        {
            // Basic HTML validation - check for balanced tags
            var openTags = new Stack<string>();
            var regex = new Regex(@"<(/?)(\w+)[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = regex.Matches(html);

            foreach (Match match in matches)
            {
                var isClosing = match.Groups[1].Value == "/";
                var tagName = match.Groups[2].Value.ToLower();

                // Skip self-closing tags
                if (new[] { "br", "hr", "img", "input", "meta", "link" }.Contains(tagName))
                    continue;

                if (isClosing)
                {
                    if (openTags.Count == 0 || openTags.Pop() != tagName)
                        return false;
                }
                else
                {
                    openTags.Push(tagName);
                }
            }

            return openTags.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    private string RenderContent(string content, Dictionary<string, object> variables)
    {
        if (string.IsNullOrEmpty(content) || variables == null || variables.Count == 0)
            return content;

        var result = content;
        var regex = new Regex(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

        result = regex.Replace(result, match =>
        {
            var variableName = match.Groups[1].Value;
            if (variables.TryGetValue(variableName, out var value))
            {
                return value?.ToString() ?? "";
            }
            return match.Value; // Keep original if variable not found
        });

        return result;
    }

    private async Task InvalidateTemplateCacheAsync()
    {
        await _cache.RemoveAsync(TEMPLATE_LIST_CACHE_KEY);
        await _cache.RemoveAsync(TEMPLATE_STATS_CACHE_KEY);
    }
}
