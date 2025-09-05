using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models.Requests;
using NotificationService.Models.Responses;
using NotificationService.Services.Interfaces;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationTemplateController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationTemplateController> _logger;

    public NotificationTemplateController(
        INotificationService notificationService,
        ILogger<NotificationTemplateController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new notification template
    /// </summary>
    /// <param name="request">Template creation details</param>
    /// <returns>Created template</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(NotificationTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationTemplateResponse>> CreateTemplate([FromBody] CreateNotificationTemplateRequest request)
    {
        try
        {
            _logger.LogInformation("Creating notification template '{TemplateName}' of type {Type}", request.Name, request.Type);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate template type
            var validTypes = new[] { "Email", "SMS", "Push", "InApp" };
            if (!validTypes.Contains(request.Type))
            {
                return BadRequest($"Invalid template type. Valid types are: {string.Join(", ", validTypes)}");
            }

            // Validate priority
            var validPriorities = new[] { "Low", "Normal", "High", "Critical" };
            if (!validPriorities.Contains(request.Priority))
            {
                return BadRequest($"Invalid priority. Valid priorities are: {string.Join(", ", validPriorities)}");
            }

            var template = await _notificationService.CreateTemplateAsync(request);
            
            _logger.LogInformation("Notification template '{TemplateName}' created successfully with ID {TemplateId}", 
                request.Name, template.Id);
            
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating template");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification template '{TemplateName}'", request.Name);
            return StatusCode(500, "An error occurred while creating the notification template");
        }
    }

    /// <summary>
    /// Get a specific notification template by ID
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Template details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationTemplateResponse>> GetTemplate(Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving notification template {TemplateId}", id);

            var template = await _notificationService.GetTemplateAsync(id);
            if (template == null)
            {
                _logger.LogWarning("Notification template {TemplateId} not found", id);
                return NotFound($"Notification template with ID {id} not found");
            }

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification template {TemplateId}", id);
            return StatusCode(500, "An error occurred while retrieving the notification template");
        }
    }

    /// <summary>
    /// Get notification templates with filtering and pagination
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <returns>Paginated list of templates</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<NotificationTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResponse<NotificationTemplateResponse>>> GetTemplates([FromQuery] GetNotificationTemplatesRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving notification templates with filters - Page: {Page}, PageSize: {PageSize}", 
                request.Page, request.PageSize);

            if (request.PageSize > 100)
            {
                return BadRequest("Maximum page size is 100");
            }

            // Validate type filter if provided
            if (!string.IsNullOrEmpty(request.Type))
            {
                var validTypes = new[] { "Email", "SMS", "Push", "InApp" };
                if (!validTypes.Contains(request.Type))
                {
                    return BadRequest($"Invalid template type filter. Valid types are: {string.Join(", ", validTypes)}");
                }
            }

            // Validate sort direction
            if (!string.IsNullOrEmpty(request.SortDirection) && 
                !new[] { "asc", "desc" }.Contains(request.SortDirection.ToLower()))
            {
                return BadRequest("Sort direction must be 'asc' or 'desc'");
            }

            var result = await _notificationService.GetTemplatesAsync(request);
            
            _logger.LogInformation("Retrieved {Count} notification templates (Page {Page} of {TotalPages})", 
                result.Data.Count, result.Page, result.TotalPages);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for getting templates");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification templates");
            return StatusCode(500, "An error occurred while retrieving notification templates");
        }
    }

    /// <summary>
    /// Update an existing notification template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="request">Template update details</param>
    /// <returns>Updated template</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Service")]
    [ProducesResponseType(typeof(NotificationTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationTemplateResponse>> UpdateTemplate(Guid id, [FromBody] UpdateNotificationTemplateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating notification template {TemplateId}", id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate priority if provided
            if (!string.IsNullOrEmpty(request.Priority))
            {
                var validPriorities = new[] { "Low", "Normal", "High", "Critical" };
                if (!validPriorities.Contains(request.Priority))
                {
                    return BadRequest($"Invalid priority. Valid priorities are: {string.Join(", ", validPriorities)}");
                }
            }

            var template = await _notificationService.UpdateTemplateAsync(id, request);
            if (template == null)
            {
                _logger.LogWarning("Notification template {TemplateId} not found for update", id);
                return NotFound($"Notification template with ID {id} not found");
            }

            _logger.LogInformation("Notification template {TemplateId} updated successfully", id);
            return Ok(template);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating template");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification template {TemplateId}", id);
            return StatusCode(500, "An error occurred while updating the notification template");
        }
    }

    /// <summary>
    /// Delete a notification template
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTemplate(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting notification template {TemplateId}", id);

            var success = await _notificationService.DeleteTemplateAsync(id);
            if (!success)
            {
                _logger.LogWarning("Notification template {TemplateId} not found for deletion", id);
                return NotFound($"Notification template with ID {id} not found");
            }

            _logger.LogInformation("Notification template {TemplateId} deleted successfully", id);
            return Ok(new { message = "Notification template deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete notification template {TemplateId} - template is in use", id);
            return Conflict("Cannot delete template that is currently in use by notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification template {TemplateId}", id);
            return StatusCode(500, "An error occurred while deleting the notification template");
        }
    }

    /// <summary>
    /// Validate a notification template with optional variables
    /// </summary>
    /// <param name="id">Template ID</param>
    /// <param name="variables">Optional variables for validation</param>
    /// <returns>Template validation result</returns>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(TemplateValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TemplateValidationResponse>> ValidateTemplate(
        Guid id, 
        [FromBody] Dictionary<string, object>? variables = null)
    {
        try
        {
            _logger.LogInformation("Validating notification template {TemplateId}", id);

            // First check if template exists
            var template = await _notificationService.GetTemplateAsync(id);
            if (template == null)
            {
                _logger.LogWarning("Notification template {TemplateId} not found for validation", id);
                return NotFound($"Notification template with ID {id} not found");
            }

            var validationResult = await _notificationService.ValidateTemplateAsync(id, variables);
            
            _logger.LogInformation("Notification template {TemplateId} validation completed - Valid: {IsValid}", 
                id, validationResult.IsValid);
            
            return Ok(validationResult);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for template validation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating notification template {TemplateId}", id);
            return StatusCode(500, "An error occurred while validating the notification template");
        }
    }
}
