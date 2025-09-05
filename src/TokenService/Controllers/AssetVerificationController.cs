using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Controllers;

[Authorize]
[ApiController]
[Route("api/asset-verification")]
public class AssetVerificationController : ControllerBase
{
    private readonly IAssetVerificationService _assetVerificationService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AssetVerificationController> _logger;

    public AssetVerificationController(
        IAssetVerificationService assetVerificationService,
        ITokenService tokenService,
        ILogger<AssetVerificationController> logger)
    {
        _assetVerificationService = assetVerificationService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate asset verification for a token submission
    /// </summary>
    /// <param name="request">Asset verification request with asset details and documents</param>
    /// <returns>Asset verification response with verification ID and status</returns>
    [HttpPost("initiate")]
    public async Task<ActionResult<AssetVerificationResponse>> InitiateVerification([FromBody] AssetVerificationRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var verificationId = await _assetVerificationService.InitiateVerificationAsync(request);
            
            var response = new AssetVerificationResponse
            {
                VerificationId = verificationId,
                Status = "Initiated",
                AssetType = request.AssetType,
                AssetId = request.AssetId,
                InitiatedAt = DateTime.UtcNow,
                EstimatedCompletionTime = DateTime.UtcNow.AddHours(24) // Default 24 hour estimate
            };
            
            _logger.LogInformation("Asset verification initiated by user {UserId} with verification ID {VerificationId}", 
                currentUserId, verificationId);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid asset verification request: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating asset verification for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { error = "Internal server error occurred while initiating asset verification" });
        }
    }

    /// <summary>
    /// Get the status of an asset verification
    /// </summary>
    /// <param name="verificationId">The unique identifier of the asset verification</param>
    /// <returns>Current verification status and details</returns>
    [HttpGet("{verificationId}/status")]
    public async Task<ActionResult<AssetVerificationResult>> GetVerificationStatus(string verificationId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var status = await _assetVerificationService.GetVerificationStatusAsync(verificationId);
            
            if (status == null)
            {
                return NotFound(new { error = "Asset verification not found" });
            }
            
            // Note: In a real implementation, we would check if the user owns this verification
            // For now, we'll allow access to all authenticated users
            
            return Ok(status);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status for verification {VerificationId}", verificationId);
            return StatusCode(500, new { error = "Internal server error occurred while retrieving verification status" });
        }
    }

    /// <summary>
    /// Verify asset ownership for a specific user
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset</param>
    /// <param name="userId">The user ID to verify ownership for (optional, defaults to current user)</param>
    /// <returns>Ownership verification result</returns>
    [HttpPost("verify-ownership")]
    public async Task<ActionResult<OwnershipVerificationResult>> VerifyOwnership([FromBody] VerifyOwnershipRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var userIdToVerify = request.UserId ?? currentUserId;
            
            // Only allow users to verify their own ownership unless they have admin privileges
            if (userIdToVerify != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid("You can only verify your own asset ownership");
            }
            
            var result = await _assetVerificationService.VerifyOwnershipAsync(request.AssetId, userIdToVerify);
            
            _logger.LogInformation("Ownership verification completed for asset {AssetId} and user {UserId}. Result: {IsOwner}", 
                request.AssetId, userIdToVerify, result.IsOwner);
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying ownership for asset {AssetId}", request.AssetId);
            return StatusCode(500, new { error = "Internal server error occurred while verifying ownership" });
        }
    }

    /// <summary>
    /// Validate asset documents for authenticity
    /// </summary>
    /// <param name="documents">List of documents to validate</param>
    /// <returns>Document validation result</returns>
    [HttpPost("validate-documents")]
    public async Task<ActionResult<DocumentValidationResponse>> ValidateDocuments([FromBody] List<AssetDocument> documents)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var isValid = await _assetVerificationService.ValidateDocumentsAsync(documents);
            
            var response = new DocumentValidationResponse
            {
                IsValid = isValid,
                ValidatedAt = DateTime.UtcNow,
                DocumentCount = documents.Count,
                ValidationMethod = "Automated Document Analysis"
            };
            
            _logger.LogInformation("Document validation completed by user {UserId}. {Count} documents validated. Result: {IsValid}", 
                currentUserId, documents.Count, isValid);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating documents for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { error = "Internal server error occurred while validating documents" });
        }
    }

    /// <summary>
    /// Get supported asset types for verification
    /// </summary>
    /// <returns>List of supported asset types</returns>
    [HttpGet("supported-assets")]
    public async Task<ActionResult<SupportedAssetsResponse>> GetSupportedAssets()
    {
        try
        {
            var supportedAssetTypes = await _assetVerificationService.GetSupportedAssetTypesAsync();
            
            var response = new SupportedAssetsResponse
            {
                AssetTypes = supportedAssetTypes.Select(assetType => new SupportedAssetType
                {
                    AssetType = assetType,
                    DisplayName = FormatAssetTypeName(assetType),
                    Description = GetAssetTypeDescription(assetType),
                    RequiredDocuments = GetRequiredDocuments(assetType),
                    OptionalDocuments = GetOptionalDocuments(assetType),
                    EstimatedProcessingTime = GetEstimatedProcessingTime(assetType),
                    IsActive = true
                }).ToList()
            };
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported asset types");
            return StatusCode(500, new { error = "Internal server error occurred while retrieving supported asset types" });
        }
    }

    /// <summary>
    /// Get estimated verification time for a specific asset type
    /// </summary>
    /// <param name="assetType">The asset type to get estimation for</param>
    /// <returns>Estimated verification time</returns>
    [HttpGet("estimate-time/{assetType}")]
    public async Task<ActionResult<VerificationTimeEstimateResponse>> GetVerificationTimeEstimate(string assetType)
    {
        try
        {
            var estimatedTime = await _assetVerificationService.EstimateVerificationTimeAsync(assetType);
            
            var response = new VerificationTimeEstimateResponse
            {
                AssetType = assetType,
                EstimatedTime = estimatedTime,
                EstimatedAt = DateTime.UtcNow
            };
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification time estimate for asset type {AssetType}", assetType);
            return StatusCode(500, new { error = "Internal server error occurred while getting time estimate" });
        }
    }

    /// <summary>
    /// Webhook endpoint for receiving verification status updates from external providers
    /// </summary>
    /// <param name="verificationId">The unique identifier of the asset verification</param>
    /// <param name="callbackData">Verification callback data from external provider</param>
    /// <returns>Success confirmation</returns>
    [AllowAnonymous] // Webhooks typically don't use user authentication
    [HttpPost("{verificationId}/webhook")]
    public async Task<ActionResult> VerificationWebhook(string verificationId, [FromBody] Dictionary<string, object> callbackData)
    {
        try
        {
            // Validate webhook signature/authentication here in a real implementation
            // For now, we'll process the callback directly
            
            await _assetVerificationService.HandleVerificationCallbackAsync(verificationId, callbackData);
            
            _logger.LogInformation("Webhook callback processed for verification {VerificationId}", verificationId);
            
            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid webhook callback for verification {VerificationId}: {Error}", verificationId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for verification {VerificationId}", verificationId);
            return StatusCode(500, new { error = "Internal server error occurred while processing webhook" });
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

    private string FormatAssetTypeName(string assetType)
    {
        return assetType switch
        {
            "RealEstate" => "Real Estate",
            "Commodity" => "Commodity",
            "Security" => "Security",
            "DigitalAsset" => "Digital Asset",
            "Artwork" => "Artwork",
            "Collectible" => "Collectible",
            "IntellectualProperty" => "Intellectual Property",
            _ => assetType
        };
    }

    private string GetAssetTypeDescription(string assetType)
    {
        return assetType switch
        {
            "RealEstate" => "Physical real estate properties including residential, commercial, and industrial properties",
            "Commodity" => "Physical commodities such as gold, silver, oil, agricultural products",
            "Security" => "Financial securities including stocks, bonds, and other investment instruments",
            "DigitalAsset" => "Digital assets including cryptocurrencies, NFTs, and digital collectibles",
            "Artwork" => "Physical and digital artwork, paintings, sculptures, and other artistic creations",
            "Collectible" => "Collectible items including rare coins, stamps, trading cards, and memorabilia",
            "IntellectualProperty" => "Intellectual property including patents, trademarks, copyrights, and trade secrets",
            _ => "Asset type requiring verification for tokenization"
        };
    }

    private List<string> GetRequiredDocuments(string assetType)
    {
        return assetType switch
        {
            "RealEstate" => new List<string> { "Property Deed", "Title Certificate", "Property Valuation" },
            "Commodity" => new List<string> { "Certificate of Authenticity", "Storage Receipt", "Quality Certificate" },
            "Security" => new List<string> { "Security Certificate", "Ownership Proof", "Regulatory Filing" },
            "DigitalAsset" => new List<string> { "Ownership Proof", "Transaction History", "Authenticity Certificate" },
            "Artwork" => new List<string> { "Certificate of Authenticity", "Provenance Documentation", "Appraisal Report" },
            "Collectible" => new List<string> { "Certificate of Authenticity", "Condition Report", "Provenance Documentation" },
            "IntellectualProperty" => new List<string> { "Patent/Trademark Certificate", "Registration Documents", "Ownership Proof" },
            _ => new List<string> { "Ownership Proof", "Certificate of Authenticity" }
        };
    }

    private List<string> GetOptionalDocuments(string assetType)
    {
        return assetType switch
        {
            "RealEstate" => new List<string> { "Insurance Documentation", "Property Photos", "Survey Report" },
            "Commodity" => new List<string> { "Insurance Certificate", "Transportation Documents", "Origin Certificate" },
            "Security" => new List<string> { "Dividend History", "Performance Reports", "Analyst Reports" },
            "DigitalAsset" => new List<string> { "Metadata Documentation", "Creation History", "Previous Sales Records" },
            "Artwork" => new List<string> { "Exhibition History", "Conservation Reports", "Insurance Valuation" },
            "Collectible" => new List<string> { "Grading Certificate", "Historical Documentation", "Insurance Valuation" },
            "IntellectualProperty" => new List<string> { "Usage History", "Licensing Agreements", "Valuation Reports" },
            _ => new List<string> { "Additional Documentation", "Supporting Evidence" }
        };
    }

    private TimeSpan GetEstimatedProcessingTime(string assetType)
    {
        return assetType switch
        {
            "RealEstate" => TimeSpan.FromDays(7),
            "Commodity" => TimeSpan.FromDays(3),
            "Security" => TimeSpan.FromDays(5),
            "DigitalAsset" => TimeSpan.FromDays(1),
            "Artwork" => TimeSpan.FromDays(10),
            "Collectible" => TimeSpan.FromDays(5),
            "IntellectualProperty" => TimeSpan.FromDays(14),
            _ => TimeSpan.FromDays(5)
        };
    }

    #endregion
}

// Additional request/response models for asset verification
public class AssetVerificationResponse
{
    public string VerificationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public DateTime InitiatedAt { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
}

public class VerifyOwnershipRequest
{
    public string AssetId { get; set; } = string.Empty;
    public Guid? UserId { get; set; } // Optional, defaults to current user
}

public class DocumentValidationResponse
{
    public bool IsValid { get; set; }
    public DateTime ValidatedAt { get; set; }
    public int DocumentCount { get; set; }
    public string ValidationMethod { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
}

public class SupportedAssetsResponse
{
    public List<SupportedAssetType> AssetTypes { get; set; } = new();
}

public class SupportedAssetType
{
    public string AssetType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RequiredDocuments { get; set; } = new();
    public List<string> OptionalDocuments { get; set; } = new();
    public TimeSpan EstimatedProcessingTime { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VerificationTimeEstimateResponse
{
    public string AssetType { get; set; } = string.Empty;
    public TimeSpan EstimatedTime { get; set; }
    public DateTime EstimatedAt { get; set; }
}
