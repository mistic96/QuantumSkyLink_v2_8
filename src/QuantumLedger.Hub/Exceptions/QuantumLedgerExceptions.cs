using System.Runtime.Serialization;

namespace QuantumLedger.Hub.Exceptions;

/// <summary>
/// Base exception for all QuantumLedger-specific exceptions
/// </summary>
[Serializable]
public abstract class QuantumLedgerException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets additional context information about the error
    /// </summary>
    public Dictionary<string, object> Context { get; }

    protected QuantumLedgerException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    protected QuantumLedgerException(string errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }

    protected QuantumLedgerException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN";
        Context = new Dictionary<string, object>();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }

    /// <summary>
    /// Adds context information to the exception
    /// </summary>
    /// <param name="key">The context key</param>
    /// <param name="value">The context value</param>
    /// <returns>This exception instance for method chaining</returns>
    public QuantumLedgerException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
}

/// <summary>
/// Exception thrown when input validation fails
/// </summary>
[Serializable]
public class ValidationException : QuantumLedgerException
{
    public ValidationException(string message) : base("VALIDATION_ERROR", message)
    {
    }

    public ValidationException(string message, Exception innerException) : base("VALIDATION_ERROR", message, innerException)
    {
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

/// <summary>
/// Exception thrown when a blockchain operation fails
/// </summary>
[Serializable]
public class BlockchainOperationException : QuantumLedgerException
{
    /// <summary>
    /// Gets the blockchain operation that failed
    /// </summary>
    public string Operation { get; }

    public BlockchainOperationException(string operation, string message) : base("BLOCKCHAIN_OPERATION_ERROR", message)
    {
        Operation = operation;
    }

    public BlockchainOperationException(string operation, string message, Exception innerException) : base("BLOCKCHAIN_OPERATION_ERROR", message, innerException)
    {
        Operation = operation;
    }

    protected BlockchainOperationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Operation = info.GetString(nameof(Operation)) ?? "UNKNOWN";
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Operation), Operation);
    }
}

/// <summary>
/// Exception thrown when a transaction operation fails
/// </summary>
[Serializable]
public class TransactionException : QuantumLedgerException
{
    /// <summary>
    /// Gets the transaction ID associated with the error
    /// </summary>
    public string? TransactionId { get; }

    public TransactionException(string message, string? transactionId = null) : base("TRANSACTION_ERROR", message)
    {
        TransactionId = transactionId;
    }

    public TransactionException(string message, Exception innerException, string? transactionId = null) : base("TRANSACTION_ERROR", message, innerException)
    {
        TransactionId = transactionId;
    }

    protected TransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        TransactionId = info.GetString(nameof(TransactionId));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(TransactionId), TransactionId);
    }
}

/// <summary>
/// Exception thrown when an account operation fails
/// </summary>
[Serializable]
public class AccountException : QuantumLedgerException
{
    /// <summary>
    /// Gets the account address associated with the error
    /// </summary>
    public string? AccountAddress { get; }

    public AccountException(string message, string? accountAddress = null) : base("ACCOUNT_ERROR", message)
    {
        AccountAddress = accountAddress;
    }

    public AccountException(string message, Exception innerException, string? accountAddress = null) : base("ACCOUNT_ERROR", message, innerException)
    {
        AccountAddress = accountAddress;
    }

    protected AccountException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        AccountAddress = info.GetString(nameof(AccountAddress));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(AccountAddress), AccountAddress);
    }
}

/// <summary>
/// Exception thrown when configuration is invalid or missing
/// </summary>
[Serializable]
public class ConfigurationException : QuantumLedgerException
{
    /// <summary>
    /// Gets the configuration key that caused the error
    /// </summary>
    public string? ConfigurationKey { get; }

    public ConfigurationException(string message, string? configurationKey = null) : base("CONFIGURATION_ERROR", message)
    {
        ConfigurationKey = configurationKey;
    }

    public ConfigurationException(string message, Exception innerException, string? configurationKey = null) : base("CONFIGURATION_ERROR", message, innerException)
    {
        ConfigurationKey = configurationKey;
    }

    protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ConfigurationKey = info.GetString(nameof(ConfigurationKey));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ConfigurationKey), ConfigurationKey);
    }
}

/// <summary>
/// Exception thrown when a caching operation fails
/// </summary>
[Serializable]
public class CacheException : QuantumLedgerException
{
    /// <summary>
    /// Gets the cache key associated with the error
    /// </summary>
    public string? CacheKey { get; }

    public CacheException(string message, string? cacheKey = null) : base("CACHE_ERROR", message)
    {
        CacheKey = cacheKey;
    }

    public CacheException(string message, Exception innerException, string? cacheKey = null) : base("CACHE_ERROR", message, innerException)
    {
        CacheKey = cacheKey;
    }

    protected CacheException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        CacheKey = info.GetString(nameof(CacheKey));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(CacheKey), CacheKey);
    }
}
