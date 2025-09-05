using TokenService.Models.Requests;
using TokenService.Models.Responses;

namespace TokenService.Services.Interfaces;

public interface IAiComplianceService
{
    /// <summary>
    /// Calculates comprehensive compliance score for a token submission
    /// </summary>
    Task<ComplianceScore> CalculateComplianceScoreAsync(TokenSubmissionRequest request);
    
    /// <summary>
    /// Generates AI-powered recommendations for improving token compliance
    /// </summary>
    Task<List<string>> GetRecommendationsAsync(TokenSubmissionRequest request);
    
    /// <summary>
    /// Detects potential red flags in token submission
    /// </summary>
    Task<List<string>> DetectRedFlagsAsync(TokenSubmissionRequest request);
    
    /// <summary>
    /// Analyzes community benefit potential
    /// </summary>
    Task<decimal> AnalyzeCommunityBenefitAsync(string purpose, string useCase);
    
    /// <summary>
    /// Checks regulatory compliance based on SEC guidelines
    /// </summary>
    Task<decimal> CheckRegulatoryComplianceAsync(TokenConfiguration config, string? assetType);
    
    /// <summary>
    /// Assesses fraud risk using anti-rug pull and anti-FTX detection
    /// </summary>
    Task<decimal> AssessFraudRiskAsync(TokenSubmissionRequest request);
    
    /// <summary>
    /// Validates token economics and sustainability
    /// </summary>
    Task<bool> ValidateTokenEconomicsAsync(TokenConfiguration config);
}
