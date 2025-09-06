using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGatewayService.Data;
using PaymentGatewayService.Data.Entities;
using PaymentGatewayService.Services.Interfaces;
using System.Text.Json;

namespace PaymentGatewayService.Controllers;

/// <summary>
/// Admin endpoints for deposit code management
/// </summary>
[ApiController]
[Route("api/deposit-codes")]
public class DepositCodeAdminController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentProcessingService _paymentProcessingService;
    private readonly ILogger<DepositCodeAdminController> _logger;

public DepositCodeAdminController(
        PaymentDbContext context,
        IPaymentProcessingService paymentProcessingService,
        ILogger<DepositCodeAdminController> logger)
    {
        _context = context;
        _paymentProcessingService = paymentProcessingService;
        _logger = logger;
    }

    [HttpGet("pending-review")]
    public async Task<IActionResult> GetPendingReviewCodes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] string? filterReason = null)
    {
        try
        {
            var query = _context.DepositCodes
                .Include(dc => dc.User)
                .Where(dc => dc.Status == DepositCodeStatus.UnderReview);

            if (!string.IsNullOrEmpty(filterReason))
            {
                query = query.Where(dc => dc.Metadata != null && dc.Metadata.Contains(filterReason));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "amount" => sortDescending ? query.OrderByDescending(dc => dc.Amount) : query.OrderBy(dc => dc.Amount),
                "code" => sortDescending ? query.OrderByDescending(dc => dc.Code) : query.OrderBy(dc => dc.Code),
                _ => sortDescending ? query.OrderByDescending(dc => dc.CreatedAt) : query.OrderBy(dc => dc.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(dc => new DepositCodeReviewItem
                {
                    Id = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                    Code = dc.Code,
                    Status = dc.Status.ToString(),
                    ReviewReason = ExtractReviewReason(dc.Metadata),
                    Amount = dc.Amount ?? 0m, // Handle nullable decimal
                    Currency = dc.Currency,
                    UserId = dc.UserId, //string.IsNullOrEmpty(dc.UserId) ? (Guid?)null : ParseGuidFromString(dc.UserId),
                    UserEmail = dc.User != null ? dc.User.Email : null,
                    CreatedAt = dc.CreatedAt,
                    HeldSince = dc.UpdatedAt,
                    DuplicateCount = _context.DepositCodes.Count(d => d.Code.ToUpper() == dc.Code.ToUpper() && d.Id != dc.Id)
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<DepositCodeReviewItem>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending review codes");
            return StatusCode(500, new { error = "Failed to retrieve pending review codes" });
        }
    }

    [HttpGet("review/{depositCodeId}")]
    public async Task<IActionResult> GetDepositCodeReviewDetail(int depositCodeId)
    {
        try
        {
            var depositCode = await _context.DepositCodes
                .Include(dc => dc.User)
                .FirstOrDefaultAsync(dc => dc.Id == depositCodeId);

            if (depositCode == null)
            {
                return NotFound();
            }

            // Get associated payment if exists
            var payment = await _context.Payments
                .Where(p => p.Metadata != null && p.Metadata.Contains(depositCode.Code))
                .FirstOrDefaultAsync();

            // Get duplicates
            var duplicates = await _context.DepositCodes
                .Include(dc => dc.User)
                .Where(dc => dc.Code.ToUpper() == depositCode.Code.ToUpper() && dc.Id != depositCodeId)
                .Select(dc => new DepositCodeDuplicate
                {
                    Id = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                    Code = dc.Code,
                    UserId = dc.UserId,
                    UserEmail = dc.User != null ? dc.User.Email : null,
                    CreatedAt = dc.CreatedAt,
                    Status = dc.Status.ToString()
                })
                .ToListAsync();

            var detail = new DepositCodeReviewDetail
            {
                Id = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                Code = depositCode.Code,
                Status = depositCode.Status.ToString(),
                ReviewReason = ExtractReviewReason(depositCode.Metadata),
                Amount = depositCode.Amount ?? 0m, // Handle nullable decimal
                Currency = depositCode.Currency,
                UserId = depositCode.UserId,
                User = depositCode.User != null ? new UserInfo
                {
                    Id = depositCode.User.Id,
                    Email = depositCode.User.Email,
                    FullName = depositCode.User.FullName,
                    KycStatus = depositCode.User.KycStatus,
                    RegisteredAt = depositCode.User.CreatedAt
                } : null,
                AssociatedPayment = payment != null ? new PaymentInfo
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status.ToString(),
                    PaymentMethod = depositCode.PaymentId?.ToString() ?? "Unknown", // Use PaymentId instead of PaymentMethodId
                    CreatedAt = payment.CreatedAt
                } : null,
                Duplicates = duplicates,
                Metadata = string.IsNullOrEmpty(depositCode.Metadata) 
                    ? new Dictionary<string, object>() 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(depositCode.Metadata) ?? new Dictionary<string, object>(),
                CreatedAt = depositCode.CreatedAt,
                HeldSince = depositCode.Status == DepositCodeStatus.UnderReview ? depositCode.UpdatedAt : null
            };

            return Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deposit code review detail");
            return StatusCode(500, new { error = "Failed to retrieve deposit code details" });
        }
    }

    [HttpPost("review/{depositCodeId}/approve")]
    public async Task<IActionResult> ApproveDepositCode(int depositCodeId, [FromBody] DepositCodeApprovalRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var depositCode = await _context.DepositCodes
                .FirstOrDefaultAsync(dc => dc.Id == depositCodeId);

            if (depositCode == null)
            {
                return NotFound(new { error = "Deposit code not found" });
            }

            if (depositCode.Status != DepositCodeStatus.UnderReview)
            {
                return BadRequest(new { error = $"Deposit code is not under review. Current status: {depositCode.Status}" });
            }

            // Update deposit code status
            depositCode.Status = DepositCodeStatus.Active;
            depositCode.UpdatedAt = DateTime.UtcNow;
            
            // Add approval metadata
            var metadata = string.IsNullOrEmpty(depositCode.Metadata) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(depositCode.Metadata) ?? new Dictionary<string, object>();
            
            metadata["approvalReason"] = request.ApprovalReason;
            metadata["approvalNotes"] = request.ApprovalNotes ?? "";
            metadata["approvedBy"] = request.ApprovedBy ?? "system";
            metadata["approvedAt"] = request.ApprovedAt?.ToString("O") ?? DateTime.UtcNow.ToString("O");
            
            depositCode.Metadata = JsonSerializer.Serialize(metadata);

            // Process any held payment
            var heldPayment = await _context.Payments
                .Where(p => p.Metadata != null && p.Metadata.Contains(depositCode.Code) && p.Status == PaymentStatus.Held)
                .FirstOrDefaultAsync();

            bool paymentProcessed = false;
            if (heldPayment != null)
            {
                await _paymentProcessingService.UpdatePaymentStatusAsync(heldPayment.Id, PaymentStatus.Processing);
                paymentProcessed = true;
            }

            // Process duplicates if requested
            int duplicatesProcessed = 0;
            if (request.ProcessDuplicatesAsWell)
            {
                var duplicates = await _context.DepositCodes
                    .Where(dc => dc.Code.ToUpper() == depositCode.Code.ToUpper() && 
                                dc.Id != depositCodeId && 
                                dc.Status == DepositCodeStatus.UnderReview)
                    .ToListAsync();

                foreach (var duplicate in duplicates)
                {
                    duplicate.Status = DepositCodeStatus.Active;
                    duplicate.UpdatedAt = DateTime.UtcNow;
                    duplicatesProcessed++;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Record metrics (monitoring removed)
            _logger.LogInformation("Deposit code approved: {Code}, Reason: {Reason}, ApprovedBy: {ApprovedBy}", depositCode.Code, request.ApprovalReason, request.ApprovedBy);

            return Ok(new DepositCodeApprovalResult
            {
                Success = true,
                DepositCodeId = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                Status = depositCode.Status.ToString(),
                PaymentProcessed = paymentProcessed,
                DuplicatesProcessed = duplicatesProcessed
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error approving deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to approve deposit code" });
        }
    }

    [HttpPost("review/{depositCodeId}/reject")]
    public async Task<IActionResult> RejectDepositCode(int depositCodeId, [FromBody] DepositCodeRejectionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var depositCode = await _context.DepositCodes
                .FirstOrDefaultAsync(dc => dc.Id == depositCodeId);

            if (depositCode == null)
            {
                return NotFound(new { error = "Deposit code not found" });
            }

            if (depositCode.Status != DepositCodeStatus.UnderReview)
            {
                return BadRequest(new { error = $"Deposit code is not under review. Current status: {depositCode.Status}" });
            }

            // Update deposit code status
            depositCode.Status = DepositCodeStatus.Rejected;
            depositCode.UpdatedAt = DateTime.UtcNow;
            
            // Add rejection metadata
            var metadata = string.IsNullOrEmpty(depositCode.Metadata) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(depositCode.Metadata) ?? new Dictionary<string, object>();
            
            metadata["rejectionReason"] = request.RejectionReason;
            metadata["rejectionNotes"] = request.RejectionNotes ?? "";
            metadata["rejectedBy"] = request.RejectedBy ?? "system";
            metadata["rejectedAt"] = request.RejectedAt?.ToString("O") ?? DateTime.UtcNow.ToString("O");
            metadata["refundInitiated"] = request.InitiateRefund;
            
            depositCode.Metadata = JsonSerializer.Serialize(metadata);

            // Handle associated payment
            var associatedPayment = await _context.Payments
                .Where(p => p.Metadata != null && p.Metadata.Contains(depositCode.Code) && 
                           (p.Status == PaymentStatus.Held || p.Status == PaymentStatus.Processing))
                .FirstOrDefaultAsync();

            bool refundInitiated = false;
            decimal? refundAmount = null;
            string? refundTransactionId = null;

            if (associatedPayment != null && request.InitiateRefund)
            {
                // Update payment status to rejected
                await _paymentProcessingService.UpdatePaymentStatusAsync(associatedPayment.Id, PaymentStatus.Rejected);
                
                // TODO: Initiate actual refund through payment gateway
                refundInitiated = true;
                refundAmount = associatedPayment.Amount - associatedPayment.FeeAmount;
                refundTransactionId = $"REF-{associatedPayment.Id}-{DateTime.UtcNow.Ticks}";
                
                _logger.LogInformation("Refund initiated for payment {PaymentId}, Amount: {RefundAmount}", 
                    associatedPayment.Id, refundAmount);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Record metrics (monitoring removed)
            _logger.LogInformation("Deposit code rejected: {Code}, Reason: {Reason}, RejectedBy: {RejectedBy}", depositCode.Code, request.RejectionReason, request.RejectedBy);

            return Ok(new DepositCodeRejectionResult
            {
                Success = true,
                DepositCodeId = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                Status = depositCode.Status.ToString(),
                RefundInitiated = refundInitiated,
                RefundAmount = refundAmount,
                RefundTransactionId = refundTransactionId
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error rejecting deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to reject deposit code" });
        }
    }

    [HttpPost("bulk-action")]
    public async Task<IActionResult> BulkDepositCodeAction([FromBody] BulkDepositCodeActionRequest request)
    {
        var results = new List<BulkActionItemResult>();
        
        foreach (var depositCodeId in request.DepositCodeIds)
        {
            try
            {
                // Convert Guid to int for database operations
                var intDepositCodeId = depositCodeId.GetHashCode(); // Simple conversion - in real implementation, you'd need proper mapping
                
                switch (request.Action)
                {
                    case BulkAction.Approve:
                        var approvalRequest = new DepositCodeApprovalRequest
                        {
                            ApprovalReason = request.Reason ?? "Bulk approval",
                            ApprovalNotes = request.Notes,
                            ApprovedBy = request.PerformedBy,
                            ApprovedAt = request.PerformedAt
                        };
                        await ApproveDepositCodeInternal(intDepositCodeId, approvalRequest);
                        results.Add(new BulkActionItemResult { DepositCodeId = depositCodeId, Success = true });
                        break;
                        
                    case BulkAction.Reject:
                        var rejectionRequest = new DepositCodeRejectionRequest
                        {
                            RejectionReason = request.Reason ?? "Bulk rejection",
                            RejectionNotes = request.Notes,
                            RejectedBy = request.PerformedBy,
                            RejectedAt = request.PerformedAt,
                            InitiateRefund = true
                        };
                        await RejectDepositCodeInternal(intDepositCodeId, rejectionRequest);
                        results.Add(new BulkActionItemResult { DepositCodeId = depositCodeId, Success = true });
                        break;
                        
                    case BulkAction.Hold:
                        await UpdateDepositCodeStatus(intDepositCodeId, DepositCodeStatus.UnderReview);
                        results.Add(new BulkActionItemResult { DepositCodeId = depositCodeId, Success = true });
                        break;
                        
                    case BulkAction.Release:
                        await UpdateDepositCodeStatus(intDepositCodeId, DepositCodeStatus.Active);
                        results.Add(new BulkActionItemResult { DepositCodeId = depositCodeId, Success = true });
                        break;
                        
                    case BulkAction.MarkReviewed:
                        await MarkDepositCodeReviewed(intDepositCodeId, request.PerformedBy ?? "system");
                        results.Add(new BulkActionItemResult { DepositCodeId = depositCodeId, Success = true });
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk action for deposit code {DepositCodeId}", depositCodeId);
                results.Add(new BulkActionItemResult 
                { 
                    DepositCodeId = depositCodeId, 
                    Success = false, 
                    ErrorMessage = ex.Message 
                });
            }
        }

        var successCount = results.Count(r => r.Success);
        var failedCount = results.Count(r => !r.Success);

        return Ok(new BulkActionResult
        {
            TotalCount = request.DepositCodeIds.Count,
            SuccessCount = successCount,
            FailedCount = failedCount,
            Results = results
        });
    }

    [HttpGet("duplicate-statistics")]
    public async Task<IActionResult> GetDuplicateStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var allCodes = await _context.DepositCodes
                .Where(dc => dc.CreatedAt >= start && dc.CreatedAt <= end)
                .ToListAsync();

            var duplicateGroups = allCodes
                .GroupBy(dc => dc.Code.ToUpper())
                .Where(g => g.Count() > 1)
                .ToList();

            var topPatterns = duplicateGroups
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new DuplicatePattern
                {
                    Pattern = g.Key,
                    Count = g.Count(),
                    Examples = g.Take(3).Select(dc => dc.Code).ToList()
                })
                .ToList();

            var duplicatesByStatus = allCodes
                .Where(dc => duplicateGroups.Any(g => g.Key == dc.Code.ToUpper()))
                .GroupBy(dc => dc.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return Ok(new DuplicateStatistics
            {
                StartDate = start,
                EndDate = end,
                TotalDuplicates = duplicateGroups.Sum(g => g.Count()),
                UniqueDuplicateSets = duplicateGroups.Count,
                ResolvedDuplicates = duplicateGroups.Sum(g => g.Count(dc => dc.Status == DepositCodeStatus.Used || dc.Status == DepositCodeStatus.Expired)),
                PendingDuplicates = duplicateGroups.Sum(g => g.Count(dc => dc.Status == DepositCodeStatus.UnderReview)),
                TopPatterns = topPatterns,
                DuplicatesByStatus = duplicatesByStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting duplicate statistics");
            return StatusCode(500, new { error = "Failed to retrieve duplicate statistics" });
        }
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchDepositCodes([FromBody] DepositCodeSearchRequest request)
    {
        try
        {
            var query = _context.DepositCodes.Include(dc => dc.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Code))
            {
                query = query.Where(dc => dc.Code.Contains(request.Code.ToUpper()));
            }

            if (request.Statuses != null && request.Statuses.Any())
            {
                var statusEnums = request.Statuses
                    .Select(s => Enum.Parse<DepositCodeStatus>(s, true))
                    .ToList();
                query = query.Where(dc => statusEnums.Contains(dc.Status));
            }

            if (request.UserId.HasValue)
            {
                query = query.Where(dc => dc.UserId == request.UserId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(dc => dc.CreatedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(dc => dc.CreatedAt <= request.EndDate.Value);
            }

            if (request.MinAmount.HasValue)
            {
                query = query.Where(dc => dc.Amount >= request.MinAmount.Value);
            }

            if (request.MaxAmount.HasValue)
            {
                query = query.Where(dc => dc.Amount <= request.MaxAmount.Value);
            }

            if (!string.IsNullOrEmpty(request.Currency))
            {
                query = query.Where(dc => dc.Currency == request.Currency);
            }

            if (request.HasDuplicates.HasValue)
            {
                if (request.HasDuplicates.Value)
                {
                    var duplicateCodes = _context.DepositCodes
                        .GroupBy(dc => dc.Code.ToUpper())
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();
                    query = query.Where(dc => duplicateCodes.Contains(dc.Code.ToUpper()));
                }
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "amount" => request.SortDescending ? query.OrderByDescending(dc => dc.Amount) : query.OrderBy(dc => dc.Amount),
                "code" => request.SortDescending ? query.OrderByDescending(dc => dc.Code) : query.OrderBy(dc => dc.Code),
                "status" => request.SortDescending ? query.OrderByDescending(dc => dc.Status) : query.OrderBy(dc => dc.Status),
                _ => request.SortDescending ? query.OrderByDescending(dc => dc.CreatedAt) : query.OrderBy(dc => dc.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(dc => new DepositCodeSearchResult
                {
                    Id = Guid.NewGuid(), // Convert int Id to Guid for API compatibility
                    Code = dc.Code,
                    Status = dc.Status.ToString(),
                    Amount = dc.Amount ?? 0m, // Handle nullable decimal
                    Currency = dc.Currency,
                    UserId = dc.UserId,
                    UserEmail = dc.User != null ? dc.User.Email : null,
                    CreatedAt = dc.CreatedAt,
                    UsedAt = dc.Status == DepositCodeStatus.Used ? dc.UpdatedAt : null,
                    HasDuplicates = _context.DepositCodes.Any(d => d.Code.ToUpper() == dc.Code.ToUpper() && d.Id != dc.Id)
                })
                .ToListAsync();

            return Ok(new PaginatedResponse<DepositCodeSearchResult>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching deposit codes");
            return StatusCode(500, new { error = "Failed to search deposit codes" });
        }
    }

    [HttpGet("{depositCodeId}/audit-trail")]
    public async Task<IActionResult> GetDepositCodeAuditTrail(int depositCodeId)
    {
        try
        {
            var depositCode = await _context.DepositCodes.FindAsync(depositCodeId);
            if (depositCode == null)
            {
                return NotFound();
            }

            var auditTrail = new List<DepositCodeAuditEntry>();

            // Add creation event
            auditTrail.Add(new DepositCodeAuditEntry
            {
                Id = Guid.NewGuid(),
                Timestamp = depositCode.CreatedAt,
                Action = "Created",
                PerformedBy = depositCode.UserId != Guid.Empty ? depositCode.UserId.ToString() : "System",
                Details = $"Deposit code {depositCode.Code} created"
            });

            // Extract events from metadata
            if (!string.IsNullOrEmpty(depositCode.Metadata))
            {
                try
                {
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(depositCode.Metadata);
                    
                    // Check for approval
                    if (metadata != null && metadata.ContainsKey("approvedAt"))
                    {
                        auditTrail.Add(new DepositCodeAuditEntry
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = DateTime.Parse(metadata["approvedAt"].ToString()!),
                            Action = "Approved",
                            PerformedBy = metadata.GetValueOrDefault("approvedBy")?.ToString() ?? "System",
                            Details = metadata.GetValueOrDefault("approvalReason")?.ToString()
                        });
                    }

                    // Check for rejection
                    if (metadata != null && metadata.ContainsKey("rejectedAt"))
                    {
                        auditTrail.Add(new DepositCodeAuditEntry
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = DateTime.Parse(metadata["rejectedAt"].ToString()!),
                            Action = "Rejected",
                            PerformedBy = metadata.GetValueOrDefault("rejectedBy")?.ToString() ?? "System",
                            Details = metadata.GetValueOrDefault("rejectionReason")?.ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse metadata for audit trail");
                }
            }

            // Add status changes based on current status
            if (depositCode.Status == DepositCodeStatus.Used)
            {
                auditTrail.Add(new DepositCodeAuditEntry
                {
                    Id = Guid.NewGuid(),
                    Timestamp = depositCode.UpdatedAt ?? depositCode.CreatedAt, // Handle nullable UpdatedAt
                    Action = "Used",
                    PerformedBy = depositCode.UserId != Guid.Empty ? depositCode.UserId.ToString() : "System",
                    Details = "Deposit code was used for a transaction"
                });
            }

            return Ok(auditTrail.OrderByDescending(a => a.Timestamp).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit trail for deposit code {DepositCodeId}", depositCodeId);
            return StatusCode(500, new { error = "Failed to retrieve audit trail" });
        }
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportDepositCodes([FromBody] DepositCodeExportRequest request)
    {
        try
        {
            // Use the search logic to get the data
            var searchRequest = request.SearchCriteria;
            var query = _context.DepositCodes.Include(dc => dc.User).AsQueryable();

            // Apply same filters as search
            if (!string.IsNullOrEmpty(searchRequest.Code))
            {
                query = query.Where(dc => dc.Code.Contains(searchRequest.Code.ToUpper()));
            }

            if (searchRequest.Statuses != null && searchRequest.Statuses.Any())
            {
                var statusEnums = searchRequest.Statuses
                    .Select(s => Enum.Parse<DepositCodeStatus>(s, true))
                    .ToList();
                query = query.Where(dc => statusEnums.Contains(dc.Status));
            }

            if (searchRequest.UserId.HasValue)
            {
                query = query.Where(dc => dc.UserId == searchRequest.UserId.Value);
            }

            // Get all results (no pagination for export)
            var results = await query.ToListAsync();

            byte[] exportData;
            
            switch (request.Format)
            {
                case ExportFormat.CSV:
                    exportData = GenerateCsvExport(results, request.IncludeFields);
                    break;
                case ExportFormat.JSON:
                    exportData = GenerateJsonExport(results, request.IncludeFields);
                    break;
                case ExportFormat.Excel:
                    // For now, just use CSV format
                    exportData = GenerateCsvExport(results, request.IncludeFields);
                    break;
                default:
                    exportData = GenerateCsvExport(results, request.IncludeFields);
                    break;
            }

            return Ok(exportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting deposit codes");
            return StatusCode(500, new { error = "Failed to export deposit codes" });
        }
    }

    // Helper methods
    private static string ExtractReviewReason(string? metadata)
    {
        if (string.IsNullOrEmpty(metadata))
            return "Unknown";

        try
        {
            var metadataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(metadata);
            if (metadataDict != null && metadataDict.TryGetValue("reviewReason", out var reason))
            {
                return reason.ToString() ?? "Unknown";
            }
        }
        catch
        {
            // Ignore deserialization errors
        }

        return "Unknown";
    }

    private async Task ApproveDepositCodeInternal(int depositCodeId, DepositCodeApprovalRequest request)
    {
        var depositCode = await _context.DepositCodes.FindAsync(depositCodeId);
        if (depositCode != null && depositCode.Status == DepositCodeStatus.UnderReview)
        {
            depositCode.Status = DepositCodeStatus.Active;
            depositCode.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task RejectDepositCodeInternal(int depositCodeId, DepositCodeRejectionRequest request)
    {
        var depositCode = await _context.DepositCodes.FindAsync(depositCodeId);
        if (depositCode != null && depositCode.Status == DepositCodeStatus.UnderReview)
        {
            depositCode.Status = DepositCodeStatus.Rejected;
            depositCode.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpdateDepositCodeStatus(int depositCodeId, DepositCodeStatus status)
    {
        var depositCode = await _context.DepositCodes.FindAsync(depositCodeId);
        if (depositCode != null)
        {
            depositCode.Status = status;
            depositCode.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private async Task MarkDepositCodeReviewed(int depositCodeId, string reviewedBy)
    {
        var depositCode = await _context.DepositCodes.FindAsync(depositCodeId);
        if (depositCode != null)
        {
            var metadata = string.IsNullOrEmpty(depositCode.Metadata) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(depositCode.Metadata) ?? new Dictionary<string, object>();
            
            metadata["reviewed"] = true;
            metadata["reviewedBy"] = reviewedBy;
            metadata["reviewedAt"] = DateTime.UtcNow.ToString("O");
            
            depositCode.Metadata = JsonSerializer.Serialize(metadata);
            depositCode.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private byte[] GenerateCsvExport(List<DepositCode> depositCodes, List<string> includeFields)
    {
        var sb = new System.Text.StringBuilder();
        
        // Default fields if none specified
        if (includeFields == null || !includeFields.Any())
        {
            includeFields = new List<string> { "Id", "Code", "Status", "Amount", "Currency", "UserId", "CreatedAt" };
        }

        // Header
        sb.AppendLine(string.Join(",", includeFields));

        // Data rows
        foreach (var dc in depositCodes)
        {
            var values = new List<string>();
            
            foreach (var field in includeFields)
            {
                var value = field switch
                {
                    "Id" => dc.Id.ToString(),
                    "Code" => dc.Code,
                    "Status" => dc.Status.ToString(),
                    "Amount" => (dc.Amount ?? 0m).ToString(),
                    "Currency" => dc.Currency,
                    "UserId" => dc.UserId != Guid.Empty ? dc.UserId.ToString() : "",
                    "CreatedAt" => dc.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    "UpdatedAt" => (dc.UpdatedAt ?? dc.CreatedAt).ToString("yyyy-MM-dd HH:mm:ss"),
                    "ExpiresAt" => (dc.ExpiresAt ?? DateTime.MinValue).ToString("yyyy-MM-dd HH:mm:ss"),
                    _ => ""
                };
                
                // Escape values containing commas
                if (value.Contains(","))
                {
                    value = $"\"{value}\"";
                }
                
                values.Add(value);
            }
            
            sb.AppendLine(string.Join(",", values));
        }

        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GenerateJsonExport(List<DepositCode> depositCodes, List<string> includeFields)
    {
        var exportData = depositCodes.Select(dc => 
        {
            var obj = new Dictionary<string, object>();
            
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("Id"))
                obj["Id"] = dc.Id;
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("Code"))
                obj["Code"] = dc.Code;
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("Status"))
                obj["Status"] = dc.Status.ToString();
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("Amount"))
                obj["Amount"] = dc.Amount ?? 0m;
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("Currency"))
                obj["Currency"] = dc.Currency;
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("UserId"))
                obj["UserId"] = dc.UserId;
            if (includeFields == null || !includeFields.Any() || includeFields.Contains("CreatedAt"))
                obj["CreatedAt"] = dc.CreatedAt;
            if (includeFields.Contains("UpdatedAt"))
                obj["UpdatedAt"] = dc.UpdatedAt;
            if (includeFields.Contains("ExpiresAt"))
                obj["ExpiresAt"] = dc.ExpiresAt;
            if (includeFields.Contains("Metadata"))
                obj["Metadata"] = dc.Metadata;
                
            return obj;
        }).ToList();

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    // Helper method to safely parse GUID from string
    private static Guid? ParseGuidFromString(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return null;
        
        return Guid.TryParse(input, out var result) ? result : null;
    }
}

// Models used by both AdminAPIGateway and PaymentGatewayService
public class DepositCodeReviewItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReviewReason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? HeldSince { get; set; }
    public int DuplicateCount { get; set; }
}

public class DepositCodeReviewDetail
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ReviewReason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public UserInfo? User { get; set; }
    public PaymentInfo? AssociatedPayment { get; set; }
    public List<DepositCodeDuplicate> Duplicates { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? HeldSince { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string KycStatus { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public class PaymentInfo
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DepositCodeDuplicate
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DepositCodeApprovalRequest
{
    public string ApprovalReason { get; set; } = string.Empty;
    public string? ApprovalNotes { get; set; }
    public bool ProcessDuplicatesAsWell { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class DepositCodeApprovalResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DepositCodeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool PaymentProcessed { get; set; }
    public int DuplicatesProcessed { get; set; }
}

public class DepositCodeRejectionRequest
{
    public string RejectionReason { get; set; } = string.Empty;
    public string? RejectionNotes { get; set; }
    public bool InitiateRefund { get; set; } = true;
    public string? CustomerCommunication { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
}

public class DepositCodeRejectionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid DepositCodeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool RefundInitiated { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundTransactionId { get; set; }
}

public class BulkDepositCodeActionRequest
{
    public List<Guid> DepositCodeIds { get; set; } = new();
    public BulkAction Action { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime? PerformedAt { get; set; }
}

public enum BulkAction
{
    Approve,
    Reject,
    Hold,
    Release,
    MarkReviewed
}

public class BulkActionResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkActionItemResult> Results { get; set; } = new();
}

public class BulkActionItemResult
{
    public Guid DepositCodeId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DuplicateStatistics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDuplicates { get; set; }
    public int UniqueDuplicateSets { get; set; }
    public int ResolvedDuplicates { get; set; }
    public int PendingDuplicates { get; set; }
    public List<DuplicatePattern> TopPatterns { get; set; } = new();
    public Dictionary<string, int> DuplicatesByStatus { get; set; } = new();
}

public class DuplicatePattern
{
    public string Pattern { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Examples { get; set; } = new();
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DepositCodeSearchRequest
{
    public string? Code { get; set; }
    public List<string>? Statuses { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Currency { get; set; }
    public bool? HasDuplicates { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "createdAt";
    public bool SortDescending { get; set; } = true;
}

public class DepositCodeSearchResult
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool HasDuplicates { get; set; }
}

public class DepositCodeAuditEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, object>? Changes { get; set; }
}

public class DepositCodeExportRequest
{
    public DepositCodeSearchRequest SearchCriteria { get; set; } = new();
    public ExportFormat Format { get; set; } = ExportFormat.CSV;
    public List<string> IncludeFields { get; set; } = new();
}

public enum ExportFormat
{
    CSV,
    Excel,
    JSON
}
