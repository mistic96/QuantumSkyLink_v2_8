using System.Text.Json;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ComplianceService.Data;
using ComplianceService.Data.Entities;
using ComplianceService.Models.Requests;
using ComplianceService.Models.Responses;
using ComplianceService.Models.ComplyCube;
using ComplianceService.Services.Interfaces;

namespace ComplianceService.Services;

public class ComplianceService : IComplianceService
{
    private readonly ComplianceDbContext _context;
    private readonly IComplyCubeService _complyCubeService;
    private readonly IUserServiceClient _userService;
    private readonly IAccountServiceClient _accountService;
    private readonly ISecurityServiceClient _securityService;
    private readonly INotificationServiceClient _notificationService;
    private readonly ILogger<ComplianceService> _logger;

    public ComplianceService(
        ComplianceDbContext context,
        IComplyCubeService complyCubeService,
        IUserServiceClient userService,
        IAccountServiceClient accountService,
        ISecurityServiceClient securityService,
        INotificationServiceClient notificationService,
        ILogger<ComplianceService> logger)
    {
        _context = context;
        _complyCubeService = complyCubeService;
        _userService = userService;
        _accountService = accountService;
        _securityService = securityService;
        _notificationService = notificationService;
        _logger = logger;
    }

    #region KYC Management

    public async Task<KycStatusResponse> InitiateKycAsync(Guid userId, InitiateKycRequest request)
    {
        _logger.LogInformation("Initiating KYC verification for user {UserId} with correlation {CorrelationId}", userId, request.CorrelationId);

        try
        {
            // Validate user exists
            var user = await _userService.GetUserAsync(userId);
            if (user == null)
            {
                throw new ArgumentException($"User {userId} not found");
            }

            // Check if there's already an active KYC verification
            var existingKyc = await _context.KycVerifications
                .FirstOrDefaultAsync(k => k.UserId == userId && k.IsActive && 
                    (k.Status == "Initiated" || k.Status == "EmailSent" || k.Status == "DocumentsSubmitted"));

            if (existingKyc != null)
            {
                return existingKyc.Adapt<KycStatusResponse>();
            }

            // Create ComplyCube client
            var clientRequest = new ComplyCubeClientCreateRequest
            {
                Type = "individual",
                Email = request.Email,
                PersonDetails = new PersonDetails
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    Nationality = request.Nationality,
                    Address = !string.IsNullOrEmpty(request.Address) ? new Address
                    {
                        Line1 = request.Address,
                        City = request.City ?? "",
                        PostalCode = request.PostalCode ?? "",
                        Country = request.Country ?? ""
                    } : null
                },
                ReferenceId = userId.ToString()
            };

            var complyCubeClient = await _complyCubeService.CreateClientAsync(clientRequest);

            // Create KYC verification record
            var kycVerification = new KycVerification
            {
                UserId = userId,
                ComplyCubeClientId = complyCubeClient.Id,
                Status = "EmailSent",
                KycLevel = request.KycLevel,
                TriggerReason = request.TriggerReason,
                Comments = request.Comments,
                CorrelationId = request.CorrelationId,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 days to complete
                CreatedBy = "System"
            };

            _context.KycVerifications.Add(kycVerification);
            await _context.SaveChangesAsync();

            // Log compliance event
            await LogComplianceEventAsync(userId, "KycInitiated", "Medium", 
                $"KYC verification initiated for level {request.KycLevel}", 
                "ComplianceService", "InitiateKyc", "Success", 
                new { KycLevel = request.KycLevel, TriggerReason = request.TriggerReason }, 
                request.CorrelationId, null, null, kycVerification.Id);

            // Send notification email
            try
            {
                await _notificationService.SendEmailAsync(new SendEmailRequest
                {
                    To = request.Email,
                    Subject = "Identity Verification Required",
                    Body = $"Please complete your identity verification. You will receive an email from ComplyCube with instructions.",
                    TemplateId = "kyc_initiated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send KYC initiation email to {Email}", request.Email);
            }

            var response = kycVerification.Adapt<KycStatusResponse>();
            response.Message = "KYC verification initiated. You will receive an email with instructions to complete the verification process.";

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating KYC verification for user {UserId}", userId);
            
            await LogComplianceEventAsync(userId, "KycInitiationFailed", "High", 
                $"Failed to initiate KYC verification: {ex.Message}", 
                "ComplianceService", "InitiateKyc", "Failure", 
                new { Error = ex.Message }, request.CorrelationId);
            
            throw;
        }
    }

    public async Task<KycStatusResponse> GetKycStatusAsync(Guid userId, Guid? verificationId = null)
    {
        KycVerification? kycVerification;

        if (verificationId.HasValue)
        {
            kycVerification = await _context.KycVerifications
                .FirstOrDefaultAsync(k => k.Id == verificationId.Value && k.UserId == userId);
        }
        else
        {
            kycVerification = await _context.KycVerifications
                .Where(k => k.UserId == userId && k.IsActive)
                .OrderByDescending(k => k.CreatedAt)
                .FirstOrDefaultAsync();
        }

        if (kycVerification == null)
        {
            throw new ArgumentException("KYC verification not found");
        }

        return kycVerification.Adapt<KycStatusResponse>();
    }

    public async Task<KycListResponse> GetUserKycHistoryAsync(Guid userId, int page = 1, int pageSize = 50)
    {
        var query = _context.KycVerifications
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt);

        var totalCount = await query.CountAsync();
        var verifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new KycListResponse
        {
            Verifications = verifications.Adapt<List<KycStatusResponse>>(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsUserKycCompliantAsync(Guid userId, string? requiredLevel = null)
    {
        var latestKyc = await _context.KycVerifications
            .Where(k => k.UserId == userId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestKyc == null || latestKyc.Status != "Approved")
        {
            return false;
        }

        if (!string.IsNullOrEmpty(requiredLevel))
        {
            var levelHierarchy = new Dictionary<string, int>
            {
                ["Basic"] = 1,
                ["Enhanced"] = 2,
                ["Premium"] = 3
            };

            var currentLevel = levelHierarchy.GetValueOrDefault(latestKyc.KycLevel, 0);
            var requiredLevelValue = levelHierarchy.GetValueOrDefault(requiredLevel, 0);

            return currentLevel >= requiredLevelValue;
        }

        return true;
    }

    public async Task UpdateKycStatusAsync(Guid verificationId, string status, string? failureReason = null, decimal? riskScore = null)
    {
        var kycVerification = await _context.KycVerifications
            .FirstOrDefaultAsync(k => k.Id == verificationId);

        if (kycVerification == null)
        {
            throw new ArgumentException($"KYC verification {verificationId} not found");
        }

        kycVerification.Status = status;
        kycVerification.FailureReason = failureReason;
        kycVerification.RiskScore = riskScore;
        kycVerification.UpdatedAt = DateTime.UtcNow;
        kycVerification.UpdatedBy = "System";

        if (status == "Approved" || status == "Rejected")
        {
            kycVerification.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Update user KYC status in UserService
        try
        {
            await _userService.UpdateUserKycStatusAsync(kycVerification.UserId, new UpdateKycStatusRequest
            {
                Status = status,
                KycLevel = kycVerification.KycLevel,
                RiskScore = riskScore,
                CompletedAt = kycVerification.CompletedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update user KYC status in UserService for user {UserId}", kycVerification.UserId);
        }

        // Log compliance event
        await LogComplianceEventAsync(kycVerification.UserId, "KycStatusUpdated", "Medium", 
            $"KYC verification status updated to {status}", 
            "ComplianceService", "UpdateKycStatus", "Success", 
            new { Status = status, FailureReason = failureReason, RiskScore = riskScore }, 
            Guid.NewGuid().ToString(), null, null, verificationId);
    }

    #endregion

    #region Case Management

    public async Task<CaseResponse> CreateCaseAsync(Guid userId, CreateCaseRequest request)
    {
        _logger.LogInformation("Creating compliance case for user {UserId}", userId);

        try
        {
            var complianceCase = new ComplianceCase
            {
                UserId = userId,
                KycVerificationId = request.KycVerificationId,
                CaseType = request.CaseType,
                Priority = request.Priority,
                Title = request.Title,
                Description = request.Description,
                AssignedTo = request.AssignedTo,
                CorrelationId = request.CorrelationId,
                CreatedBy = "System",
                LastActivityAt = DateTime.UtcNow
            };

            _context.ComplianceCases.Add(complianceCase);
            await _context.SaveChangesAsync();

            // Log compliance event
            await LogComplianceEventAsync(userId, "CaseCreated", "Medium", 
                $"Compliance case created: {request.Title}", 
                "ComplianceService", "CreateCase", "Success", 
                new { CaseType = request.CaseType, Priority = request.Priority }, 
                request.CorrelationId ?? Guid.NewGuid().ToString(), null, null, 
                request.KycVerificationId, complianceCase.Id);

            return complianceCase.Adapt<CaseResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance case for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CaseResponse> GetCaseAsync(Guid caseId, Guid userId)
    {
        var complianceCase = await _context.ComplianceCases
            .Include(c => c.CaseDocuments)
            .Include(c => c.CaseReviews)
            .FirstOrDefaultAsync(c => c.Id == caseId && c.UserId == userId);

        if (complianceCase == null)
        {
            throw new ArgumentException($"Case {caseId} not found for user {userId}");
        }

        var response = complianceCase.Adapt<CaseResponse>();
        response.Documents = complianceCase.CaseDocuments.Adapt<List<CaseDocumentResponse>>();
        response.Reviews = complianceCase.CaseReviews.Adapt<List<CaseReviewResponse>>();

        return response;
    }

    public async Task<CaseListResponse> GetUserCasesAsync(Guid userId, string? status = null, int page = 1, int pageSize = 50)
    {
        var query = _context.ComplianceCases
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        var totalCount = await query.CountAsync();
        var cases = await query
            .Include(c => c.CaseDocuments)
            .Include(c => c.CaseReviews)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var caseResponses = cases.Select(c =>
        {
            var response = c.Adapt<CaseResponse>();
            response.Documents = c.CaseDocuments.Adapt<List<CaseDocumentResponse>>();
            response.Reviews = c.CaseReviews.Adapt<List<CaseReviewResponse>>();
            return response;
        }).ToList();

        return new CaseListResponse
        {
            Cases = caseResponses,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<CaseResponse> SubmitCaseDocumentAsync(Guid caseId, Guid userId, SubmitCaseDocumentRequest request)
    {
        var complianceCase = await _context.ComplianceCases
            .FirstOrDefaultAsync(c => c.Id == caseId && c.UserId == userId);

        if (complianceCase == null)
        {
            throw new ArgumentException($"Case {caseId} not found for user {userId}");
        }

        var caseDocument = new CaseDocument
        {
            CaseId = caseId,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileData.Length,
            FileData = request.FileData,
            Comments = request.Comments,
            UploadedBy = userId.ToString(),
            CorrelationId = request.CorrelationId
        };

        _context.CaseDocuments.Add(caseDocument);
        
        complianceCase.Status = "DocumentsSubmitted";
        complianceCase.LastActivityAt = DateTime.UtcNow;
        complianceCase.UpdatedAt = DateTime.UtcNow;
        complianceCase.UpdatedBy = userId.ToString();

        await _context.SaveChangesAsync();

        // Log compliance event
        await LogComplianceEventAsync(userId, "DocumentSubmitted", "Low", 
            $"Document submitted for case {complianceCase.CaseNumber}", 
            "ComplianceService", "SubmitDocument", "Success", 
            new { DocumentType = request.DocumentType, FileName = request.FileName }, 
            request.CorrelationId ?? Guid.NewGuid().ToString(), null, null, 
            complianceCase.KycVerificationId, caseId);

        return await GetCaseAsync(caseId, userId);
    }

    public async Task<CaseResponse> ReviewCaseAsync(Guid caseId, Guid reviewerId, ReviewCaseRequest request)
    {
        var complianceCase = await _context.ComplianceCases
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (complianceCase == null)
        {
            throw new ArgumentException($"Case {caseId} not found");
        }

        var caseReview = new CaseReview
        {
            CaseId = caseId,
            ReviewType = request.ReviewType,
            ReviewResult = request.ReviewResult,
            ReviewNotes = request.ReviewNotes,
            DetailedAnalysis = request.DetailedAnalysis,
            ConfidenceScore = request.ConfidenceScore,
            RecommendedAction = request.RecommendedAction,
            ReviewedBy = reviewerId.ToString(),
            NextReviewBy = request.NextReviewBy,
            NextReviewDate = request.NextReviewDate,
            CorrelationId = request.CorrelationId
        };

        _context.CaseReviews.Add(caseReview);

        // Update case status based on review result
        switch (request.ReviewResult)
        {
            case "Approved":
                complianceCase.Status = "Resolved";
                complianceCase.ResolvedAt = DateTime.UtcNow;
                break;
            case "Rejected":
                complianceCase.Status = "Closed";
                complianceCase.ClosedAt = DateTime.UtcNow;
                break;
            case "NeedsMoreInfo":
                complianceCase.Status = "PendingDocuments";
                break;
            case "Escalated":
                complianceCase.Status = "Escalated";
                complianceCase.Priority = "High";
                break;
            case "Resubmit":
                complianceCase.Status = "InProgress";
                break;
        }

        complianceCase.LastActivityAt = DateTime.UtcNow;
        complianceCase.UpdatedAt = DateTime.UtcNow;
        complianceCase.UpdatedBy = reviewerId.ToString();

        await _context.SaveChangesAsync();

        // Log compliance event
        await LogComplianceEventAsync(complianceCase.UserId, "CaseReviewed", "Medium", 
            $"Case {complianceCase.CaseNumber} reviewed with result: {request.ReviewResult}", 
            "ComplianceService", "ReviewCase", "Success", 
            new { ReviewType = request.ReviewType, ReviewResult = request.ReviewResult }, 
            request.CorrelationId ?? Guid.NewGuid().ToString(), null, null, 
            complianceCase.KycVerificationId, caseId);

        return await GetCaseAsync(caseId, complianceCase.UserId);
    }

    public async Task<CaseResponse> UpdateCaseStatusAsync(Guid caseId, string status, string? resolution = null)
    {
        var complianceCase = await _context.ComplianceCases
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (complianceCase == null)
        {
            throw new ArgumentException($"Case {caseId} not found");
        }

        complianceCase.Status = status;
        complianceCase.Resolution = resolution;
        complianceCase.LastActivityAt = DateTime.UtcNow;
        complianceCase.UpdatedAt = DateTime.UtcNow;
        complianceCase.UpdatedBy = "System";

        if (status == "Resolved" || status == "Closed")
        {
            complianceCase.ResolvedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await GetCaseAsync(caseId, complianceCase.UserId);
    }

    #endregion

    #region Compliance Status

    public async Task<ComplianceStatusResponse> GetComplianceStatusAsync(Guid userId)
    {
        var latestKyc = await _context.KycVerifications
            .Where(k => k.UserId == userId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync();

        var openCases = await _context.ComplianceCases
            .CountAsync(c => c.UserId == userId && c.Status != "Resolved" && c.Status != "Closed");

        var pendingReviews = await _context.ComplianceCases
            .CountAsync(c => c.UserId == userId && (c.Status == "UnderReview" || c.Status == "PendingDocuments"));

        var requiredActions = new List<string>();

        if (latestKyc == null || latestKyc.Status != "Approved")
        {
            requiredActions.Add("Complete KYC verification");
        }

        if (openCases > 0)
        {
            requiredActions.Add($"Resolve {openCases} open compliance case(s)");
        }

        return new ComplianceStatusResponse
        {
            UserId = userId,
            IsKycCompliant = latestKyc?.Status == "Approved",
            CurrentKycStatus = latestKyc?.Status,
            KycLevel = latestKyc?.KycLevel,
            OpenCases = openCases,
            PendingReviews = pendingReviews,
            LastKycUpdate = latestKyc?.UpdatedAt ?? latestKyc?.CreatedAt,
            RequiredActions = requiredActions,
            AdditionalInfo = new Dictionary<string, object>
            {
                ["riskScore"] = latestKyc?.RiskScore ?? (object?)null,
                ["kycExpiresAt"] = latestKyc?.ExpiresAt ?? (object?)null,
                ["lastActivity"] = DateTime.UtcNow
            }
        };
    }

    public async Task<bool> ValidateComplianceRequirementsAsync(Guid userId, string operationType, decimal? amount = null)
    {
        // Check KYC compliance
        var isKycCompliant = await IsUserKycCompliantAsync(userId);
        if (!isKycCompliant)
        {
            return false;
        }

        // Check for open compliance cases
        var openCases = await _context.ComplianceCases
            .AnyAsync(c => c.UserId == userId && c.Status != "Resolved" && c.Status != "Closed");

        if (openCases)
        {
            return false;
        }

        // Operation-specific validation
        switch (operationType.ToLower())
        {
            case "high_value_transaction":
                if (amount.HasValue && amount > 10000)
                {
                    return await IsUserKycCompliantAsync(userId, "Enhanced");
                }
                break;
            case "role_upgrade":
                return await IsUserKycCompliantAsync(userId, "Enhanced");
            case "liquidity_provision":
                return await IsUserKycCompliantAsync(userId, "Premium");
        }

        return true;
    }

    #endregion

    #region Event Logging

    public async Task LogComplianceEventAsync(Guid userId, string eventType, string severity, string description, 
        string source, string action, string result, object? eventData, string correlationId, 
        string? ipAddress = null, string? userAgent = null, Guid? kycVerificationId = null, Guid? caseId = null)
    {
        try
        {
            var complianceEvent = new ComplianceEvent
            {
                UserId = userId,
                KycVerificationId = kycVerificationId,
                CaseId = caseId,
                EventType = eventType,
                Severity = severity,
                Description = description,
                Source = source,
                Action = action,
                Result = result,
                EventData = eventData != null ? JsonSerializer.Serialize(eventData) : null,
                CorrelationId = correlationId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                RequiresInvestigation = severity == "Critical" || severity == "High"
            };

            _context.ComplianceEvents.Add(complianceEvent);
            await _context.SaveChangesAsync();

            // Also log to SecurityService for centralized security event tracking
            try
            {
                await _securityService.LogSecurityEventAsync(new LogSecurityEventRequest
                {
                    UserId = userId,
                    EventType = eventType,
                    Severity = severity,
                    Description = description,
                    Source = source,
                    Action = action,
                    Result = result,
                    EventData = eventData,
                    CorrelationId = correlationId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log compliance event to SecurityService");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log compliance event for user {UserId}", userId);
        }
    }

    #endregion

    #region Webhook Processing

    public async Task ProcessComplyCubeWebhookAsync(string webhookPayload, string signature)
    {
        try
        {
            // Verify webhook signature (implementation would depend on ComplyCube's signature method)
            // For now, we'll skip signature verification in development

            var webhookData = JsonSerializer.Deserialize<ComplyCubeWebhookPayload>(webhookPayload);
            if (webhookData == null)
            {
                throw new ArgumentException("Invalid webhook payload");
            }

            _logger.LogInformation("Processing ComplyCube webhook: {Type} for client {ClientId}", 
                webhookData.Type, webhookData.Data.ClientId);

            // Find the KYC verification by ComplyCube client ID
            var kycVerification = await _context.KycVerifications
                .FirstOrDefaultAsync(k => k.ComplyCubeClientId == webhookData.Data.ClientId);

            if (kycVerification == null)
            {
                _logger.LogWarning("No KYC verification found for ComplyCube client {ClientId}", webhookData.Data.ClientId);
                return;
            }

            switch (webhookData.Type)
            {
                case "check.completed":
                    await HandleCheckCompletedAsync(kycVerification, webhookData.Data);
                    break;
                case "check.failed":
                    await HandleCheckFailedAsync(kycVerification, webhookData.Data);
                    break;
                case "review.updated":
                    await HandleReviewUpdatedAsync(kycVerification, webhookData.Data);
                    break;
                default:
                    _logger.LogInformation("Unhandled webhook type: {Type}", webhookData.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ComplyCube webhook");
            throw;
        }
    }

    private async Task HandleCheckCompletedAsync(KycVerification kycVerification, WebhookData data)
    {
        var status = data.Result switch
        {
            "clear" => "Approved",
            "consider" => "NeedsReview",
            "unresolved" => "NeedsReview",
            _ => "NeedsReview"
        };

        kycVerification.Status = status;
        kycVerification.ComplyCubeCheckId = data.Id;
        kycVerification.ComplyCubeResult = JsonSerializer.Serialize(data);
        kycVerification.UpdatedAt = DateTime.UtcNow;
        kycVerification.UpdatedBy = "ComplyCube";

        if (status == "Approved")
        {
            kycVerification.CompletedAt = DateTime.UtcNow;
        }
        else if (status == "NeedsReview")
        {
            // Create a case for manual review
            var caseRequest = new CreateCaseRequest
            {
                KycVerificationId = kycVerification.Id,
                CaseType = "KycFailure",
                Priority = "Medium",
                Title = $"KYC Verification Needs Review - {data.Result}",
                Description = $"ComplyCube verification result: {data.Result}. Manual review required.",
                CorrelationId = Guid.NewGuid().ToString()
            };

            await CreateCaseAsync(kycVerification.UserId, caseRequest);
        }

        await _context.SaveChangesAsync();

        // Log compliance event
        await LogComplianceEventAsync(kycVerification.UserId, "KycCheckCompleted", "Medium", 
            $"ComplyCube check completed with result: {data.Result}", 
            "ComplyCube", "CheckCompleted", "Success", 
            new { Result = data.Result, CheckId = data.Id }, 
            Guid.NewGuid().ToString(), null, null, kycVerification.Id);
    }

    private async Task HandleCheckFailedAsync(KycVerification kycVerification, WebhookData data)
    {
        kycVerification.Status = "Rejected";
        kycVerification.ComplyCubeCheckId = data.Id;
        kycVerification.ComplyCubeResult = JsonSerializer.Serialize(data);
        kycVerification.FailureReason = "ComplyCube check failed";
        kycVerification.CompletedAt = DateTime.UtcNow;
        kycVerification.UpdatedAt = DateTime.UtcNow;
        kycVerification.UpdatedBy = "ComplyCube";

        // Create a case for investigation
        var caseRequest = new CreateCaseRequest
        {
            KycVerificationId = kycVerification.Id,
            CaseType = "Investigation",
            Priority = "High",
            Title = "KYC Verification Failed",
            Description = "ComplyCube verification check failed. Investigation required.",
            CorrelationId = Guid.NewGuid().ToString()
        };

        await CreateCaseAsync(kycVerification.UserId, caseRequest);
        await _context.SaveChangesAsync();

        // Log compliance event
        await LogComplianceEventAsync(kycVerification.UserId, "KycCheckFailed", "High", 
            "ComplyCube check failed", 
            "ComplyCube", "CheckFailed", "Failure", 
            new { CheckId = data.Id }, 
            Guid.NewGuid().ToString(), null, null, kycVerification.Id);
    }

    private async Task HandleReviewUpdatedAsync(KycVerification kycVerification, WebhookData data)
    {
        kycVerification.ComplyCubeResult = JsonSerializer.Serialize(data);
        kycVerification.UpdatedAt = DateTime.UtcNow;
        kycVerification.UpdatedBy = "ComplyCube";

        await _context.SaveChangesAsync();

        // Log compliance event
        await LogComplianceEventAsync(kycVerification.UserId, "KycReviewUpdated", "Low", 
            "ComplyCube review updated", 
            "ComplyCube", "ReviewUpdated", "Info", 
            new { CheckId = data.Id }, 
            Guid.NewGuid().ToString(), null, null, kycVerification.Id);
    }

    #endregion

    #region Administrative Functions

    public async Task<CaseListResponse> GetCasesForReviewAsync(string reviewType, string? priority = null, int page = 1, int pageSize = 50)
    {
        var query = _context.ComplianceCases
            .Where(c => c.Status == "UnderReview" || c.Status == "PendingDocuments");

        if (reviewType == "ComplianceOfficer")
        {
            query = query.Where(c => c.RequiresComplianceOfficerReview);
        }
        else if (reviewType == "AI")
        {
            query = query.Where(c => c.RequiresAIReview);
        }

        if (!string.IsNullOrEmpty(priority))
        {
            query = query.Where(c => c.Priority == priority);
        }

        var totalCount = await query.CountAsync();
        var cases = await query
            .Include(c => c.CaseDocuments)
            .Include(c => c.CaseReviews)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var caseResponses = cases.Select(c =>
        {
            var response = c.Adapt<CaseResponse>();
            response.Documents = c.CaseDocuments.Adapt<List<CaseDocumentResponse>>();
            response.Reviews = c.CaseReviews.Adapt<List<CaseReviewResponse>>();
            return response;
        }).ToList();

        return new CaseListResponse
        {
            Cases = caseResponses,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }

    public async Task<List<ComplianceEvent>> GetComplianceEventsAsync(Guid? userId = null, string? eventType = null, 
        string? severity = null, DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        var query = _context.ComplianceEvents.AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(e => e.UserId == userId.Value);
        }

        if (!string.IsNullOrEmpty(eventType))
        {
            query = query.Where(e => e.EventType == eventType);
        }

        if (!string.IsNullOrEmpty(severity))
        {
            query = query.Where(e => e.Severity == severity);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.Timestamp <= toDate.Value);
        }

        return await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    #endregion
}
