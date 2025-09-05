using TokenService.Models.Requests;
using TokenService.Models.Responses;
using TokenService.Services.Interfaces;

namespace TokenService.Services;

public class AiComplianceService : IAiComplianceService
{
    private readonly ILogger<AiComplianceService> _logger;
    private readonly IConfiguration _configuration;

    public AiComplianceService(ILogger<AiComplianceService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ComplianceScore> CalculateComplianceScoreAsync(TokenSubmissionRequest request)
    {
        _logger.LogInformation("Calculating AI compliance score for token submission");

        // Mock AI compliance scoring - in production this would call actual AI services
        await Task.Delay(100); // Simulate AI processing time

        var baseScore = _configuration.GetValue<decimal>("AiCompliance:BaseComplianceScore", 75.0m);
        var communityWeight = _configuration.GetValue<decimal>("AiCompliance:CommunityBenefitWeight", 0.3m);
        var regulatoryWeight = _configuration.GetValue<decimal>("AiCompliance:RegulatoryComplianceWeight", 0.4m);
        var fraudWeight = _configuration.GetValue<decimal>("AiCompliance:FraudRiskWeight", 0.3m);

        // Calculate component scores based on submission data
        var communityBenefitScore = await AnalyzeCommunityBenefitAsync(request.TokenPurpose, request.UseCase);
        var regulatoryComplianceScore = await CheckRegulatoryComplianceAsync(request.Configuration, request.AssetDetails?.AssetType);
        var fraudRiskScore = await AssessFraudRiskAsync(request);

        // Calculate weighted overall score
        var overallScore = Math.Round(
            (communityBenefitScore * communityWeight) +
            (regulatoryComplianceScore * regulatoryWeight) +
            (fraudRiskScore * fraudWeight), 2);

        // Ensure score is within valid range
        overallScore = Math.Max(0, Math.Min(100, overallScore));

        var complianceScore = new ComplianceScore
        {
            OverallScore = overallScore,
            CommunityBenefitScore = communityBenefitScore,
            RegulatoryComplianceScore = regulatoryComplianceScore,
            FraudRiskScore = fraudRiskScore,
            CalculatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("AI compliance score calculated: {Score}", overallScore);
        return complianceScore;
    }

    public async Task<List<string>> GetRecommendationsAsync(TokenSubmissionRequest request)
    {
        _logger.LogInformation("Generating AI recommendations for token submission");

        await Task.Delay(50); // Simulate AI processing time

        var recommendations = new List<string>();

        // Analyze token purpose and provide recommendations
        if (string.IsNullOrWhiteSpace(request.TokenPurpose))
        {
            recommendations.Add("Consider providing a more detailed token purpose to improve compliance score");
        }

        if (string.IsNullOrWhiteSpace(request.UseCase))
        {
            recommendations.Add("Add specific use cases to demonstrate token utility and compliance");
        }

        // Asset-specific recommendations
        if (request.AssetDetails != null)
        {
            switch (request.AssetDetails.AssetType?.ToLower())
            {
                case "realestate":
                    recommendations.Add("Ensure property ownership documentation is complete and verified");
                    recommendations.Add("Consider obtaining professional property valuation");
                    break;
                case "security":
                    recommendations.Add("Verify SEC compliance for security token offerings");
                    recommendations.Add("Ensure proper investor accreditation processes");
                    break;
                case "commodity":
                    recommendations.Add("Provide commodity storage and custody documentation");
                    recommendations.Add("Consider third-party commodity verification");
                    break;
            }
        }

        // Token configuration recommendations
        if (request.Configuration.TotalSupply > 1_000_000_000)
        {
            recommendations.Add("Consider justification for high token supply to prevent inflation concerns");
        }

        if (request.Configuration.Decimals > 18)
        {
            recommendations.Add("Standard token decimals are typically 18 or fewer");
        }

        // Compliance-specific recommendations
        if (!request.Configuration.Name.Contains("Token") && !request.Configuration.Name.Contains("Coin"))
        {
            recommendations.Add("Consider including 'Token' in the name for clarity");
        }

        if (request.Configuration.Symbol.Length > 5)
        {
            recommendations.Add("Token symbols are typically 3-5 characters for better recognition");
        }

        // Add general best practices if no specific issues found
        if (recommendations.Count == 0)
        {
            recommendations.Add("Token submission meets basic compliance requirements");
            recommendations.Add("Consider implementing governance mechanisms for token holders");
            recommendations.Add("Ensure transparent communication about token utility and roadmap");
        }

        _logger.LogInformation("Generated {Count} AI recommendations", recommendations.Count);
        return recommendations;
    }

    public async Task<List<string>> DetectRedFlagsAsync(TokenSubmissionRequest request)
    {
        _logger.LogInformation("Detecting red flags for token submission");

        await Task.Delay(75); // Simulate AI processing time

        var redFlags = new List<string>();
        var redFlagThreshold = _configuration.GetValue<decimal>("AiCompliance:RedFlagThreshold", 50.0m);

        // Check for common fraud patterns
        if (request.TokenPurpose?.ToLower().Contains("guaranteed returns") == true)
        {
            redFlags.Add("CRITICAL: Mentions of guaranteed returns may indicate investment fraud");
        }

        if (request.TokenPurpose?.ToLower().Contains("get rich quick") == true)
        {
            redFlags.Add("CRITICAL: Get-rich-quick language indicates potential scam");
        }

        if (request.UseCase?.ToLower().Contains("ponzi") == true || 
            request.UseCase?.ToLower().Contains("pyramid") == true)
        {
            redFlags.Add("CRITICAL: Potential Ponzi or pyramid scheme indicators");
        }

        // Check for unrealistic promises
        if (request.Configuration.TotalSupply > 1_000_000_000_000) // 1 trillion
        {
            redFlags.Add("WARNING: Extremely high token supply may indicate inflationary tokenomics");
        }

        // Check for suspicious naming patterns
        var suspiciousNames = new[] { "moon", "rocket", "lambo", "diamond", "ape" };
        if (suspiciousNames.Any(name => 
            request.Configuration.Name.ToLower().Contains(name) || 
            request.Configuration.Symbol.ToLower().Contains(name)))
        {
            redFlags.Add("WARNING: Token name/symbol contains meme-related terms that may indicate speculative nature");
        }

        // Check for asset verification issues
        if (request.AssetDetails != null && string.IsNullOrWhiteSpace(request.AssetDetails.AssetId))
        {
            redFlags.Add("WARNING: Asset tokenization without proper asset identification");
        }

        // Check for regulatory compliance issues
        if (request.AssetDetails?.AssetType?.ToLower() == "security" && 
            !request.TokenPurpose.ToLower().Contains("sec"))
        {
            redFlags.Add("CRITICAL: Security token without SEC compliance mention");
        }

        // Check for insufficient documentation
        if (string.IsNullOrWhiteSpace(request.TokenPurpose) || request.TokenPurpose.Length < 50)
        {
            redFlags.Add("WARNING: Insufficient token purpose documentation");
        }

        if (string.IsNullOrWhiteSpace(request.UseCase) || request.UseCase.Length < 50)
        {
            redFlags.Add("WARNING: Insufficient use case documentation");
        }

        // Check for potential rug pull indicators
        if (request.Configuration.Name.ToLower().Contains("safe") && 
            request.Configuration.Symbol.ToLower().Contains("safe"))
        {
            redFlags.Add("WARNING: Overuse of 'safe' terminology may indicate false security claims");
        }

        _logger.LogInformation("Detected {Count} red flags", redFlags.Count);
        return redFlags;
    }

    public async Task<decimal> AnalyzeCommunityBenefitAsync(string purpose, string useCase)
    {
        _logger.LogInformation("Analyzing community benefit potential");
        await Task.Delay(25); // Simulate AI processing time

        decimal score = 50; // Base score

        // Analyze token purpose for community benefit indicators
        var purposeLower = purpose?.ToLower() ?? "";
        var useCaseLower = useCase?.ToLower() ?? "";

        if (purposeLower.Contains("charity") || purposeLower.Contains("donation") || purposeLower.Contains("nonprofit"))
            score += 20;

        if (purposeLower.Contains("community") || purposeLower.Contains("social") || purposeLower.Contains("public"))
            score += 15;

        if (purposeLower.Contains("environment") || purposeLower.Contains("sustainability") || purposeLower.Contains("green"))
            score += 15;

        if (useCaseLower.Contains("education") || useCaseLower.Contains("healthcare") || useCaseLower.Contains("research"))
            score += 10;

        if (useCaseLower.Contains("governance") || useCaseLower.Contains("voting") || useCaseLower.Contains("dao"))
            score += 10;

        // Penalize purely speculative purposes
        if (purposeLower.Contains("trading") || purposeLower.Contains("speculation") || purposeLower.Contains("profit"))
            score -= 10;

        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<decimal> CheckRegulatoryComplianceAsync(TokenConfiguration config, string? assetType)
    {
        _logger.LogInformation("Checking regulatory compliance");
        await Task.Delay(25); // Simulate AI processing time

        decimal score = 60; // Base score

        // Check for regulatory awareness in token name/symbol
        var nameLower = config.Name?.ToLower() ?? "";
        var symbolLower = config.Symbol?.ToLower() ?? "";

        if (nameLower.Contains("sec") || nameLower.Contains("regulatory") || nameLower.Contains("compliance"))
            score += 20;

        if (nameLower.Contains("kyc") || nameLower.Contains("aml"))
            score += 15;

        // Asset-specific compliance
        if (!string.IsNullOrWhiteSpace(assetType))
        {
            switch (assetType.ToLower())
            {
                case "security":
                    score += nameLower.Contains("sec") ? 15 : -20; // Securities require SEC compliance
                    break;
                case "realestate":
                    score += 10; // Generally well-regulated
                    break;
                case "commodity":
                    score += 5;
                    break;
            }
        }

        // Check token configuration for compliance indicators
        if (config.TotalSupply <= 1_000_000_000) // Reasonable supply
            score += 10;

        if (config.Decimals <= 18) // Standard decimals
            score += 5;

        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<decimal> AssessFraudRiskAsync(TokenSubmissionRequest request)
    {
        _logger.LogInformation("Assessing fraud risk");
        await Task.Delay(25); // Simulate AI processing time

        decimal score = 80; // Start with high score (low risk)

        var purpose = request.TokenPurpose?.ToLower() ?? "";
        var useCase = request.UseCase?.ToLower() ?? "";
        var tokenName = request.Configuration.Name?.ToLower() ?? "";
        var tokenSymbol = request.Configuration.Symbol?.ToLower() ?? "";

        // High-risk indicators (reduce score)
        if (purpose.Contains("guaranteed") || purpose.Contains("risk-free"))
            score -= 30;

        if (purpose.Contains("moon") || purpose.Contains("rocket") || purpose.Contains("lambo"))
            score -= 20;

        if (tokenName.Contains("safe") && tokenSymbol.Contains("safe"))
            score -= 15;

        if (request.Configuration.TotalSupply > 1_000_000_000_000) // 1 trillion
            score -= 15;

        // Positive indicators (increase score)
        if (request.AssetDetails != null && !string.IsNullOrWhiteSpace(request.AssetDetails.AssetId))
            score += 10;

        if (purpose.Contains("utility") || purpose.Contains("governance"))
            score += 10;

        if (!string.IsNullOrWhiteSpace(request.TokenPurpose) && request.TokenPurpose.Length > 200)
            score += 5;

        return Math.Max(0, Math.Min(100, score));
    }

    public async Task<bool> ValidateTokenEconomicsAsync(TokenConfiguration config)
    {
        _logger.LogInformation("Validating token economics");
        await Task.Delay(25); // Simulate AI processing time

        var issues = new List<string>();

        // Check total supply
        if (config.TotalSupply <= 0)
            issues.Add("Total supply must be greater than zero");

        if (config.TotalSupply > 1_000_000_000_000_000) // 1 quadrillion
            issues.Add("Total supply is extremely high and may cause economic issues");

        // Check decimals
        if (config.Decimals < 0 || config.Decimals > 18)
            issues.Add("Decimals should be between 0 and 18");

        // Check name and symbol
        if (string.IsNullOrWhiteSpace(config.Name) || config.Name.Length < 3)
            issues.Add("Token name should be at least 3 characters");

        if (string.IsNullOrWhiteSpace(config.Symbol) || config.Symbol.Length < 2 || config.Symbol.Length > 10)
            issues.Add("Token symbol should be between 2 and 10 characters");

        // Check for suspicious patterns
        if (config.Name.ToLower().Contains("scam") || config.Symbol.ToLower().Contains("scam"))
            issues.Add("Token name/symbol contains suspicious terms");

        return issues.Count == 0;
    }
}
