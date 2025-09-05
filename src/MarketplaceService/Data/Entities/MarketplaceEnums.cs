namespace MarketplaceService.Data.Entities;

/// <summary>
/// Represents the type of marketplace listing
/// </summary>
public enum MarketType
{
    /// <summary>
    /// Primary market - new token sales directly from issuers
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Secondary market - peer-to-peer trading of existing tokens
    /// </summary>
    Secondary = 2
}

/// <summary>
/// Represents the status of a marketplace listing
/// </summary>
public enum ListingStatus
{
    /// <summary>
    /// Listing is being created and validated
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Listing is active and available for purchase
    /// </summary>
    Active = 2,

    /// <summary>
    /// Listing is temporarily paused by the seller
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Listing has been sold out completely
    /// </summary>
    SoldOut = 4,

    /// <summary>
    /// Listing has expired based on expiration date
    /// </summary>
    Expired = 5,

    /// <summary>
    /// Listing has been cancelled by the seller
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Listing has been suspended by administrators
    /// </summary>
    Suspended = 7
}

/// <summary>
/// Represents the pricing strategy for a marketplace listing
/// </summary>
public enum PricingStrategy
{
    /// <summary>
    /// Fixed price per token
    /// </summary>
    Fixed = 1,

    /// <summary>
    /// Bulk pricing - all-or-nothing purchase
    /// </summary>
    Bulk = 2,

    /// <summary>
    /// Margin-based pricing with fixed dollar margin
    /// </summary>
    MarginFixed = 3,

    /// <summary>
    /// Margin-based pricing with percentage margin
    /// </summary>
    MarginPercentage = 4,

    /// <summary>
    /// Tiered pricing based on quantity ranges
    /// </summary>
    Tiered = 5,

    /// <summary>
    /// Dynamic pricing based on market conditions
    /// </summary>
    Dynamic = 6,

    /// <summary>
    /// Standard unit pricing per token
    /// </summary>
    Unit = 7
}

/// <summary>
/// Represents the status of a marketplace order
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order has been created but not yet processed
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Order is being processed and validated
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Order has been confirmed and escrow created
    /// </summary>
    Confirmed = 3,

    /// <summary>
    /// Payment has been received and verified
    /// </summary>
    Paid = 4,

    /// <summary>
    /// Order has been completed successfully
    /// </summary>
    Completed = 5,

    /// <summary>
    /// Order has been cancelled by buyer or seller
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Order has failed due to payment or other issues
    /// </summary>
    Failed = 7,

    /// <summary>
    /// Order has been refunded
    /// </summary>
    Refunded = 8,

    /// <summary>
    /// Order is in dispute resolution
    /// </summary>
    Disputed = 9
}

/// <summary>
/// Represents the status of an escrow account
/// </summary>
public enum EscrowStatus
{
    /// <summary>
    /// Escrow account has been created
    /// </summary>
    Created = 1,

    /// <summary>
    /// Assets have been locked in escrow
    /// </summary>
    Locked = 2,

    /// <summary>
    /// Payment has been received and verified
    /// </summary>
    Funded = 3,

    /// <summary>
    /// All conditions met, ready for release
    /// </summary>
    ReadyForRelease = 4,

    /// <summary>
    /// Assets and payment have been released
    /// </summary>
    Released = 5,

    /// <summary>
    /// Escrow has been cancelled and assets returned
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Escrow is in dispute resolution
    /// </summary>
    Disputed = 7,

    /// <summary>
    /// Escrow has expired due to timeout
    /// </summary>
    Expired = 8
}

/// <summary>
/// Represents the type of asset being traded
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Platform-native token created through TokenService
    /// </summary>
    PlatformToken = 1,

    /// <summary>
    /// Bitcoin (BTC)
    /// </summary>
    Bitcoin = 2,

    /// <summary>
    /// Ethereum (ETH)
    /// </summary>
    Ethereum = 3,

    /// <summary>
    /// Solana (SOL)
    /// </summary>
    Solana = 4,

    /// <summary>
    /// Other supported cryptocurrency
    /// </summary>
    OtherCrypto = 5
}

/// <summary>
/// Represents the type of marketplace transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Purchase transaction - buying tokens
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Sale transaction - selling tokens
    /// </summary>
    Sale = 2,

    /// <summary>
    /// Escrow creation transaction
    /// </summary>
    EscrowCreation = 3,

    /// <summary>
    /// Escrow release transaction
    /// </summary>
    EscrowRelease = 4,

    /// <summary>
    /// Fee payment transaction
    /// </summary>
    FeePayment = 5,

    /// <summary>
    /// Refund transaction
    /// </summary>
    Refund = 6
}
