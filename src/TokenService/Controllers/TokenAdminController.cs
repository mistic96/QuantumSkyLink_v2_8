using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Controllers;

[Authorize(Roles = "TokenReviewer,Admin")]
[ApiController]
[Route("api/admin/tokens")]
public class TokenAdminController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<TokenAdminController> _logger;

    public TokenAdminController(
        ITokenService tokenService,
        ILogger<TokenAdminController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Get all pending token submissions for review
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of pending token submissions</returns>
    [HttpGet("pending")]
    public async Task<ActionResult<TokenSubmissionAdminListResponse>> GetPendingSubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            
            var currentUserId = GetCurrentUserId();
            
            var submissions = await _tokenService.GetPendingSubmissionsAsync();
            
            // Apply pagination
            var totalCount = submissions.Count;
            var paginatedSubmissions = submissions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var response = new TokenSubmissionAdminListResponse
            {
                Submissions = paginatedSubmissions.Select(s => new TokenSubmissionAdminResponse
                {
                    Id = s.Id,
                    CreatorId = s.CreatorId,
                    TokenPurpose = s.TokenPurpose,
                    UseCase = s.UseCase,
                    AiComplianceScore = s.AiComplianceScore,
                    ApprovalStatus = s.ApprovalStatus,
                    SubmissionDate = s.SubmissionDate,
                    AssetType = s.AssetType,
                    AssetVerificationStatus = s.AssetVerificationStatus,
                    AssetVerificationDate = s.AssetVerificationDate,
                    ReviewComments = s.ReviewComments
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            _logger.LogInformation("Admin {UserId} retrieved {Count} pending submissions", currentUserId, paginatedSubmissions.Count);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending token submissions for admin review");
            return StatusCode(500, new { error = "Internal server error occurred while retrieving pending submissions" });
        }
    }

    /// <summary>
    /// Approve a token submission
    /// </summary>
    /// <param name="submissionId">The unique identifier of the token submission to approve</param>
    /// <param name="request">Approval request with comments</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPost("{submissionId}/approve")]
    public async Task<ActionResult> ApproveSubmission(Guid submissionId, [FromBody] TokenApprovalRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            
            // Set the reviewer information
            request.AdminId = currentUserId;
            request.Decision = "Approved";
            
            var result = await _tokenService.ApproveTokenSubmissionAsync(request);
            
            if (result)
            {
                _logger.LogInformation("Token submission {SubmissionId} approved by admin {UserId} ({UserName})", 
                    submissionId, currentUserId, currentUserName);
                return Ok(new { message = "Token submission approved successfully" });
            }
            
            return BadRequest(new { error = "Failed to approve token submission" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid approval request for submission {SubmissionId}: {Error}", submissionId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving token submission {SubmissionId}", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while approving submission" });
        }
    }

    /// <summary>
    /// Reject a token submission
    /// </summary>
    /// <param name="submissionId">The unique identifier of the token submission to reject</param>
    /// <param name="request">Rejection request with reason and comments</param>
    /// <returns>Success confirmation or error message</returns>
    [HttpPost("{submissionId}/reject")]
    public async Task<ActionResult> RejectSubmission(Guid submissionId, [FromBody] TokenApprovalRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            
            // Set the reviewer information
            request.AdminId = currentUserId;
            request.Decision = "Rejected";
            
            var result = await _tokenService.RejectTokenSubmissionAsync(request);
            
            if (result)
            {
                _logger.LogInformation("Token submission {SubmissionId} rejected by admin {UserId} ({UserName}). Reason: {Reason}", 
                    submissionId, currentUserId, currentUserName, request.Comments);
                return Ok(new { message = "Token submission rejected successfully" });
            }
            
            return BadRequest(new { error = "Failed to reject token submission" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid rejection request for submission {SubmissionId}: {Error}", submissionId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting token submission {SubmissionId}", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while rejecting submission" });
        }
    }

    /// <summary>
    /// Get tokens that require administrative review
    /// </summary>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of tokens requiring review</returns>
    [HttpGet("review-required")]
    public async Task<ActionResult<TokenAdminListResponse>> GetTokensRequiringReview(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;
            
            var currentUserId = GetCurrentUserId();
            
            var tokens = await _tokenService.GetTokensRequiringReviewAsync();
            
            // Apply pagination
            var totalCount = tokens.Count;
            var paginatedTokens = tokens
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            var response = new TokenAdminListResponse
            {
                Tokens = paginatedTokens.Select(t => new TokenAdminResponse
                {
                    Id = t.Id,
                    Name = t.Name,
                    Symbol = t.Symbol,
                    TotalSupply = t.TotalSupply,
                    TokenType = t.TokenType,
                    CreatorId = t.CreatorId,
                    Status = t.Status,
                    ApprovalStatus = t.ApprovalStatus,
                    AssetType = t.AssetType,
                    CrossChainEnabled = t.CrossChainEnabled,
                    Network = t.Network,
                    CreatedAt = t.CreatedAt,
                    ApprovedAt = t.ApprovedAt,
                    ApprovedBy = t.ApprovedBy
                }).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            
            _logger.LogInformation("Admin {UserId} retrieved {Count} tokens requiring review", currentUserId, paginatedTokens.Count);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tokens requiring review for admin");
            return StatusCode(500, new { error = "Internal server error occurred while retrieving tokens requiring review" });
        }
    }

    /// <summary>
    /// Get comprehensive admin statistics for token operations
    /// </summary>
    /// <returns>Admin dashboard statistics</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<TokenAdminStatisticsResponse>> GetAdminStatistics()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Get all submissions and tokens for statistics
            var pendingSubmissions = await _tokenService.GetPendingSubmissionsAsync();
            var tokensRequiringReview = await _tokenService.GetTokensRequiringReviewAsync();
            
            // Calculate statistics
            var response = new TokenAdminStatisticsResponse
            {
                PendingSubmissions = pendingSubmissions.Count,
                TokensRequiringReview = tokensRequiringReview.Count,
                HighComplianceSubmissions = pendingSubmissions.Count(s => s.AiComplianceScore >= 80),
                LowComplianceSubmissions = pendingSubmissions.Count(s => s.AiComplianceScore < 50),
                AssetVerificationPending = pendingSubmissions.Count(s => s.AssetVerificationStatus == "Pending"),
                AssetVerificationFailed = pendingSubmissions.Count(s => s.AssetVerificationStatus == "Failed"),
                SubmissionsToday = pendingSubmissions.Count(s => s.SubmissionDate.Date == DateTime.UtcNow.Date),
                SubmissionsThisWeek = pendingSubmissions.Count(s => s.SubmissionDate >= DateTime.UtcNow.AddDays(-7)),
                SubmissionsThisMonth = pendingSubmissions.Count(s => s.SubmissionDate >= DateTime.UtcNow.AddDays(-30)),
                GeneratedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Admin statistics generated for user {UserId}", currentUserId);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin statistics");
            return StatusCode(500, new { error = "Internal server error occurred while generating statistics" });
        }
    }

    /// <summary>
    /// Get detailed submission information for admin review
    /// </summary>
    /// <param name="submissionId">The unique identifier of the token submission</param>
    /// <returns>Detailed submission information including AI analysis</returns>
    [HttpGet("submissions/{submissionId}")]
    public async Task<ActionResult<TokenSubmissionAdminDetailResponse>> GetSubmissionForReview(Guid submissionId)
    {
        try
        {
            var submission = await _tokenService.GetSubmissionAsync(submissionId);
            
            if (submission == null)
            {
                return NotFound(new { error = "Token submission not found" });
            }
            
            var currentUserId = GetCurrentUserId();
            
            // Convert to detailed admin response
            var response = new TokenSubmissionAdminDetailResponse
            {
                Id = submission.Id,
                CreatorId = submission.CreatorId,
                TokenPurpose = submission.TokenPurpose,
                UseCase = submission.UseCase,
                ConfigurationJson = submission.ConfigurationJson,
                AiComplianceScore = submission.AiComplianceScore,
                ApprovalStatus = submission.ApprovalStatus,
                SubmissionDate = submission.SubmissionDate,
                ReviewedAt = submission.ReviewedAt,
                ReviewedBy = submission.ReviewedBy,
                ReviewComments = submission.ReviewComments,
                AiRecommendations = !string.IsNullOrEmpty(submission.AiRecommendations) 
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(submission.AiRecommendations) ?? new List<string>()
                    : new List<string>(),
                AiRedFlags = !string.IsNullOrEmpty(submission.AiRedFlags) 
                    ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(submission.AiRedFlags) ?? new List<string>()
                    : new List<string>(),
                AssetType = submission.AssetType,
                AssetVerificationId = submission.AssetVerificationId,
                AssetVerificationStatus = submission.AssetVerificationStatus,
                AssetVerificationDate = submission.AssetVerificationDate,
                TokenId = submission.TokenId
            };
            
            _logger.LogInformation("Admin {UserId} retrieved detailed submission {SubmissionId} for review", currentUserId, submissionId);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission {SubmissionId} for admin review", submissionId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving submission details" });
        }
    }

    /// <summary>
    /// Bulk approve multiple token submissions
    /// </summary>
    /// <param name="request">Bulk approval request with submission IDs and comments</param>
    /// <returns>Bulk approval results</returns>
    [HttpPost("bulk-approve")]
    public async Task<ActionResult<BulkApprovalResponse>> BulkApproveSubmissions([FromBody] BulkApprovalRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();
            
            var results = new List<BulkApprovalResult>();
            
            foreach (var submissionId in request.SubmissionIds)
            {
                try
                {
                    var approvalRequest = new TokenApprovalRequest
                    {
                        SubmissionId = submissionId,
                        AdminId = currentUserId,
                        Decision = "Approved",
                        Comments = request.Comments
                    };
                    
                    var success = await _tokenService.ApproveTokenSubmissionAsync(approvalRequest);
                    
                    results.Add(new BulkApprovalResult
                    {
                        SubmissionId = submissionId,
                        Success = success,
                        ErrorMessage = success ? null : "Failed to approve submission"
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new BulkApprovalResult
                    {
                        SubmissionId = submissionId,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
            
            var successCount = results.Count(r => r.Success);
            var failureCount = results.Count(r => !r.Success);
            
            _logger.LogInformation("Bulk approval completed by admin {UserId}: {SuccessCount} successful, {FailureCount} failed", 
                currentUserId, successCount, failureCount);
            
            var response = new BulkApprovalResponse
            {
                Results = results,
                SuccessCount = successCount,
                FailureCount = failureCount,
                TotalProcessed = results.Count
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk approval operation");
            return StatusCode(500, new { error = "Internal server error occurred during bulk approval" });
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

    private string? GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value 
               ?? User.FindFirst("name")?.Value 
               ?? User.FindFirst("preferred_username")?.Value;
    }

    #endregion
}

// Additional request/response models for admin operations
public class TokenSubmissionAdminListResponse
{
    public List<TokenSubmissionAdminResponse> Submissions { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class TokenSubmissionAdminResponse
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public string TokenPurpose { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
    public decimal AiComplianceScore { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public string? AssetType { get; set; }
    public string? AssetVerificationStatus { get; set; }
    public DateTime? AssetVerificationDate { get; set; }
    public string? ReviewComments { get; set; }
}

public class TokenSubmissionAdminDetailResponse
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public string TokenPurpose { get; set; } = string.Empty;
    public string UseCase { get; set; } = string.Empty;
    public string ConfigurationJson { get; set; } = string.Empty;
    public decimal AiComplianceScore { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public DateTime SubmissionDate { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewComments { get; set; }
    public List<string> AiRecommendations { get; set; } = new();
    public List<string> AiRedFlags { get; set; } = new();
    public string? AssetType { get; set; }
    public string? AssetVerificationId { get; set; }
    public string? AssetVerificationStatus { get; set; }
    public DateTime? AssetVerificationDate { get; set; }
    public Guid? TokenId { get; set; }
}

public class TokenAdminListResponse
{
    public List<TokenAdminResponse> Tokens { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class TokenAdminResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal TotalSupply { get; set; }
    public string TokenType { get; set; } = string.Empty;
    public Guid CreatorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public bool CrossChainEnabled { get; set; }
    public string Network { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}

public class TokenAdminStatisticsResponse
{
    public int PendingSubmissions { get; set; }
    public int TokensRequiringReview { get; set; }
    public int HighComplianceSubmissions { get; set; }
    public int LowComplianceSubmissions { get; set; }
    public int AssetVerificationPending { get; set; }
    public int AssetVerificationFailed { get; set; }
    public int SubmissionsToday { get; set; }
    public int SubmissionsThisWeek { get; set; }
    public int SubmissionsThisMonth { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class BulkApprovalRequest
{
    public List<Guid> SubmissionIds { get; set; } = new();
    public string Comments { get; set; } = string.Empty;
}

public class BulkApprovalResponse
{
    public List<BulkApprovalResult> Results { get; set; } = new();
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int TotalProcessed { get; set; }
}

public class BulkApprovalResult
{
    public Guid SubmissionId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
