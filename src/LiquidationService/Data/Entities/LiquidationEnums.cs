namespace LiquidationService.Data.Entities;

/// <summary>
/// Status of a liquidation request
/// </summary>
public enum LiquidationRequestStatus
{
    /// <summary>
    /// Request has been submitted and is pending initial validation
    /// </summary>
    Pending = 0,

    /// <summary>
    /// KYC/AML verification is in progress
    /// </summary>
    KycVerificationInProgress = 1,

    /// <summary>
    /// Asset verification and eligibility check in progress
    /// </summary>
    AssetVerificationInProgress = 2,

    /// <summary>
    /// Compliance and AML checks in progress
    /// </summary>
    ComplianceCheckInProgress = 3,

    /// <summary>
    /// Waiting for liquidity provider matching
    /// </summary>
    AwaitingLiquidityProvider = 4,

    /// <summary>
    /// Liquidation is being executed
    /// </summary>
    Executing = 5,

    /// <summary>
    /// Fund transfer in progress
    /// </summary>
    TransferInProgress = 6,

    /// <summary>
    /// Liquidation completed successfully
    /// </summary>
    Completed = 7,

    /// <summary>
    /// Liquidation was cancelled by user
    /// </summary>
    Cancelled = 8,

    /// <summary>
    /// Liquidation failed due to error
    /// </summary>
    Failed = 9,

    /// <summary>
    /// Liquidation rejected due to compliance issues
    /// </summary>
    Rejected = 10
}

/// <summary>
/// Type of output desired from liquidation
/// </summary>
public enum LiquidationOutputType
{
    /// <summary>
    /// Convert to fiat currency
    /// </summary>
    Fiat = 0,

    /// <summary>
    /// Convert to stablecoin
    /// </summary>
    Stablecoin = 1,

    /// <summary>
    /// Convert to another cryptocurrency
    /// </summary>
    Cryptocurrency = 2
}

/// <summary>
/// Type of destination for liquidated funds
/// </summary>
public enum DestinationType
{
    /// <summary>
    /// Bank account for fiat transfers
    /// </summary>
    BankAccount = 0,

    /// <summary>
    /// Cryptocurrency wallet address
    /// </summary>
    WalletAddress = 1,

    /// <summary>
    /// Internal platform account
    /// </summary>
    InternalAccount = 2
}

/// <summary>
/// Status of liquidity provider
/// </summary>
public enum LiquidityProviderStatus
{
    /// <summary>
    /// Provider is pending verification
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Provider is active and available
    /// </summary>
    Active = 1,

    /// <summary>
    /// Provider is temporarily inactive
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Provider is suspended
    /// </summary>
    Suspended = 3,

    /// <summary>
    /// Provider registration was rejected
    /// </summary>
    Rejected = 4
}

/// <summary>
/// Status of liquidation transaction execution
/// </summary>
public enum LiquidationTransactionStatus
{
    /// <summary>
    /// Transaction is pending execution
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Transaction is being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Transaction completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Transaction failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Transaction was cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Type of compliance check
/// </summary>
public enum ComplianceCheckType
{
    /// <summary>
    /// KYC verification check
    /// </summary>
    KycVerification = 0,

    /// <summary>
    /// AML screening check
    /// </summary>
    AmlScreening = 1,

    /// <summary>
    /// Sanctions list screening
    /// </summary>
    SanctionsScreening = 2,

    /// <summary>
    /// PEP (Politically Exposed Person) screening
    /// </summary>
    PepScreening = 3,

    /// <summary>
    /// Illicit address screening
    /// </summary>
    IllicitAddressScreening = 4,

    /// <summary>
    /// Risk assessment
    /// </summary>
    RiskAssessment = 5
}

/// <summary>
/// Result of compliance check
/// </summary>
public enum ComplianceCheckResult
{
    /// <summary>
    /// Check is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Check passed successfully
    /// </summary>
    Passed = 1,

    /// <summary>
    /// Check failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Check requires manual review
    /// </summary>
    RequiresReview = 3,

    /// <summary>
    /// Check was skipped
    /// </summary>
    Skipped = 4
}

/// <summary>
/// Asset eligibility status
/// </summary>
public enum AssetEligibilityStatus
{
    /// <summary>
    /// Asset is eligible for liquidation
    /// </summary>
    Eligible = 0,

    /// <summary>
    /// Asset is not eligible for liquidation
    /// </summary>
    NotEligible = 1,

    /// <summary>
    /// Asset has restrictions (e.g., lock-up period)
    /// </summary>
    Restricted = 2,

    /// <summary>
    /// Asset eligibility is under review
    /// </summary>
    UnderReview = 3
}

/// <summary>
/// Risk level assessment
/// </summary>
public enum RiskLevel
{
    /// <summary>
    /// Low risk
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium risk
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High risk
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical risk
    /// </summary>
    Critical = 3
}
