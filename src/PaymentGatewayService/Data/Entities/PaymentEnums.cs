namespace PaymentGatewayService.Data.Entities;

/// <summary>
/// Represents the current status of a payment transaction
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment has been created but not yet processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment is currently being processed by the gateway
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Payment has been successfully completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Payment processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Payment was cancelled before completion
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Payment has been fully refunded
    /// </summary>
    Refunded = 5,

    /// <summary>
    /// Payment has been partially refunded
    /// </summary>
    PartiallyRefunded = 6,

    /// <summary>
    /// Payment is being held for review
    /// </summary>
    Held = 7,

    /// <summary>
    /// Payment has been rejected
    /// </summary>
    Rejected = 8
}

/// <summary>
/// Represents the type of payment transaction
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Money being added to the platform (user deposit)
    /// </summary>
    Deposit = 0,

    /// <summary>
    /// Money being withdrawn from the platform
    /// </summary>
    Withdrawal = 1,

    /// <summary>
    /// Transfer between accounts within the platform
    /// </summary>
    Transfer = 2,

    /// <summary>
    /// Fee payment for platform services
    /// </summary>
    Fee = 3,

    /// <summary>
    /// Refund payment back to user
    /// </summary>
    Refund = 4,

    /// <summary>
    /// Cryptocurrency payment
    /// </summary>
    Crypto = 5
}

/// <summary>
/// Represents the type of payment gateway
/// </summary>
public enum PaymentGatewayType
{
    /// <summary>
    /// Stripe payment gateway (Secondary - Global coverage)
    /// </summary>
    Stripe = 0,

    /// <summary>
    /// Square payment gateway (Primary - Fintech optimized)
    /// </summary>
    Square = 1,

    /// <summary>
    /// Bank transfer (ACH, Wire)
    /// </summary>
    BankTransfer = 2,

    /// <summary>
    /// Cryptocurrency wallet
    /// </summary>
    CryptoWallet = 3,

    /// <summary>
    /// Apple Pay
    /// </summary>
    ApplePay = 4,

    /// <summary>
    /// Google Pay
    /// </summary>
    GooglePay = 5,

    /// <summary>
    /// PIX Brazil instant payment system
    /// </summary>
    PIXBrazil = 6,

    /// <summary>
    /// Dots.dev global payout platform (190+ countries)
    /// </summary>
    DotsDev = 7,

    /// <summary>
    /// MoonPay fiat-to-crypto and crypto-to-fiat platform
    /// </summary>
    MoonPay = 8,

    /// <summary>
    /// Coinbase Advanced Trade API for cryptocurrency trading
    /// </summary>
    Coinbase = 9
}

/// <summary>
/// Represents the type of payment provider for routing and processing
/// </summary>
public enum PaymentProviderType
{
    /// <summary>
    /// Internal platform payment processing
    /// </summary>
    Internal = 0,

    /// <summary>
    /// Square payment provider (Primary - Fintech optimized)
    /// </summary>
    Square = 1,

    /// <summary>
    /// PIX Brazil instant payment system
    /// </summary>
    PIXBrazil = 2,

    /// <summary>
    /// MoonPay fiat-to-crypto and crypto-to-fiat platform
    /// </summary>
    MoonPay = 3,

    /// <summary>
    /// Stripe payment provider (Secondary - Global coverage)
    /// </summary>
    Stripe = 4,

    /// <summary>
    /// Dots.dev global payout platform (190+ countries)
    /// </summary>
    DotsDev = 5,

    /// <summary>
    /// Coinbase Advanced Trade API for cryptocurrency trading
    /// </summary>
    Coinbase = 6
}

/// <summary>
/// Represents the type of payment method
/// </summary>
public enum PaymentMethodType
{
    /// <summary>
    /// Credit card payment method
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Debit card payment method
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// Bank account (checking/savings)
    /// </summary>
    BankAccount = 2,

    /// <summary>
    /// Digital wallet (PayPal, Apple Pay, etc.)
    /// </summary>
    DigitalWallet = 3,

    /// <summary>
    /// Cryptocurrency wallet
    /// </summary>
    Cryptocurrency = 4
}

/// <summary>
/// Represents the status of a payment attempt
/// </summary>
public enum PaymentAttemptStatus
{
    /// <summary>
    /// Attempt is pending processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Attempt is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Attempt was successful
    /// </summary>
    Succeeded = 2,

    /// <summary>
    /// Attempt failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Attempt was cancelled
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Attempt requires additional authentication
    /// </summary>
    RequiresAction = 5
}

/// <summary>
/// Represents the status of a refund
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// Refund request is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Refund is being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Refund has been completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Refund processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Refund was cancelled
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Represents the status of a webhook event
/// </summary>
public enum WebhookStatus
{
    /// <summary>
    /// Webhook received but not yet processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Webhook is being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Webhook processed successfully
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Webhook processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Webhook was ignored (duplicate or irrelevant)
    /// </summary>
    Ignored = 4
}

/// <summary>
/// Represents the status of a deposit code
/// </summary>
public enum DepositCodeStatus
{
    /// <summary>
    /// Deposit code is active and can be used
    /// </summary>
    Active = 0,

    /// <summary>
    /// Deposit code has been used for a deposit
    /// </summary>
    Used = 1,

    /// <summary>
    /// Deposit code has expired
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Deposit code has been revoked/cancelled
    /// </summary>
    Revoked = 3,

    /// <summary>
    /// Deposit code is under review
    /// </summary>
    UnderReview = 4,

    /// <summary>
    /// Deposit code has been rejected
    /// </summary>
    Rejected = 5
}
