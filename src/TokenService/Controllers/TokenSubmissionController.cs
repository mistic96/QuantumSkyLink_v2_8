using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Controllers;

[Authorize]
[ApiController]
[Route("api/token-submissions")]
public class TokenSubmissionController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenSubmissionController> _logger;

    public TokenSubmissionController(
        ITokenService tokenService,
        ILogger<TokenSubmissionController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Submit a new token for approval and compliance review
    /// </summary>
    /// <param name="request">Token submission details including asset information and configuration</param>
    /// <returns>Token submission response with compliance score and approval status</returns>
    [HttpPost]
    public async Task<ActionResult<TokenSubmissionResponse>> SubmitToken([FromBody] TokenSubmissionRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            request.CreatorId = currentUserId; // Ensure user can only submit tokens for themselves
            
            var response = await _tokenService.SubmitTokenAsync(request);
            
            _logger.LogInformation("Token submission created successfully for user {UserId} with submission ID {SubmissionId}", 
                currentUserId, response.SubmissionId);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid token submission request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized token submission attempt: {Error}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting token for user {UserId}", request.CreatorId);
            return StatusCode(500, new { error = "Internal server error occurred while submitting token" });
        }
    }

    /// <summary>
    /// Get details of a specific token submission
    /// </summary>
    /// <param name="submissionId">The unique identifier of the token submission</param>
    /// <returns>Detailed token submission information including compliance analysis</returns>
    [HttpGet("{submissionId}")]
    public async Task<ActionResult<TokenSubmissionDetailResponse>> GetSubmission(Guid submissionId)
    {
        try
        {
            var submission = await _tokenService.GetSubmissionAsync(submissionId);
            
            if (submission == null)
            {
                return NotFound(new { error = "Token submission not found" });
            }
            
            // Ensure user can only access their own submissions
            var currentUserId = GetCurrentUserId();
            if (submission.CreatorId != currentUserId)
            {
                return Forbid("You can only access your own token submissions");
            }
            
            // Convert to response model using actual entity properties
            var response = new TokenSubmissionDetailResponse
            {
                Id = submission.Id,
                CreatorId = submission.CreatorId,
                TokenPurpose = submission.TokenPurpose,
                UseCase = submission.UseCase,
                AiComplianceScore = submission.AiComplianceScore,
                ApprovalStatus = submission.ApprovalStatus,
                SubmissionDate = submission.SubmissionDate,
                ReviewedAt = submission.ReviewedAt,
                ReviewedBy = submission.ReviewedBy,
                ReviewComments = submission.ReviewComments,
                AssetType = submission.AssetType,
                AssetVerificationId = submission.AssetVerificationId,
                AssetVerificationStatus = submission.AssetVerificationStatus,
                AssetVerificationDate = submission.AssetVerificationDate,
                TokenId = submission.TokenId
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token submission {SubmissionId}", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving submission" });
        }
    }

    /// <summary>
    /// Get all token submissions for the current user with optional filtering
    /// </summary>
    /// <param name="status">Filter by approval status (Pending, Approved, Rejected)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of user's token submissions</returns>
    [HttpGet]
    public async Task<ActionResult<TokenSubmissionListResponse>> GetUserSubmissions(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            
            var currentUserId = GetCurrentUserId();
            
            // For now, we'll use the GetPendingSubmissionsAsync method and filter by user
            // In a real implementation, we'd add a GetUserSubmissionsAsync method to the service
            var allSubmissions = await _tokenService.GetPendingSubmissionsAsync();
            var userSubmissions = allSubmissions.Where(s => s.CreatorId == currentUserId);
            
            // Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                userSubmissions = userSubmissions.Where(s => s.ApprovalStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }
            
            // Apply pagination
            var totalCount = userSubmissions.Count();
            var submissions = userSubmissions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var response = new TokenSubmissionListResponse
            {
                Submissions = submissions.Select(s => new TokenSubmissionSummaryResponse
                {
                    Id = s.Id,
                    TokenPurpose = s.TokenPurpose,
                    AiComplianceScore = s.AiComplianceScore,
                    ApprovalStatus = s.ApprovalStatus,
                    SubmissionDate = s.SubmissionDate,
                    ReviewedAt = s.ReviewedAt,
                    AssetType = s.AssetType,
                    AssetVerificationStatus = s.AssetVerificationStatus
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token submissions for user");
            return StatusCode(500, new { error = "Internal server error occurred while retrieving submissions" });
        }
    }

    /// <summary>
    /// Get AI compliance analysis for a token submission
    /// </summary>
    /// <param name="submissionId">The unique identifier of the token submission</param>
    /// <returns>Detailed compliance analysis including score, recommendations, and red flags</returns>
    [HttpGet("{submissionId}/compliance")]
    public async Task<ActionResult<ComplianceAnalysisResponse>> GetComplianceAnalysis(Guid submissionId)
    {
        try
        {
            var submission = await _tokenService.GetSubmissionAsync(submissionId);
            
            if (submission == null)
            {
                return NotFound(new { error = "Token submission not found" });
            }
            
            // Ensure user can only access their own submissions
            var currentUserId = GetCurrentUserId();
            if (submission.CreatorId != currentUserId)
            {
                return Forbid("You can only access compliance analysis for your own submissions");
            }
            
            // Get compliance analysis from the submission using actual entity properties
            var response = new ComplianceAnalysisResponse
            {
                SubmissionId = submission.Id,
                ComplianceScore = submission.AiComplianceScore,
                Recommendations = !string.IsNullOrEmpty(submission.AiRecommendations) 
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(submission.AiRecommendations) ?? new List<string>()
                    : new List<string>(),
                RedFlags = !string.IsNullOrEmpty(submission.AiRedFlags) 
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(submission.AiRedFlags) ?? new List<string>()
                    : new List<string>(),
                AnalyzedAt = submission.SubmissionDate
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance analysis for submission {SubmissionId}", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving compliance analysis" });
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("user_id")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        
        return userId;
    }

    private string GetCorrelationId()
    {
        return HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString();
    }

    #endregion
}

// Additional response models for this controller
public class TokenSubmissionListResponse
{
    public List<TokenSubmissionSummaryResponse> Submissions { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class TokenSubmissionSummaryResponse
{
    public Guid Id { get; set; }
    public string TokenPurpose { get; set; } = string.Empty;
    public decimal AiComplianceScore { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? AssetType { get; set; }
    public string? AssetVerificationStatus { get; set; }
}

public class TokenSubmissionDetailResponse
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public string TokenPurpose { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
    public decimal AiComplianceScore { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewComments { get; set; }
    public string? AssetType { get; set; }
    public string? AssetVerificationId { get; set; }
    public string? AssetVerificationStatus { get; set; }
    public DateTime? AssetVerificationDate { get; set; }
    public Guid? TokenId { get; set; }
}

public class ComplianceAnalysisResponse
{
    public Guid SubmissionId { get; set; }
    public decimal ComplianceScore { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public List<string> RedFlags { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}
