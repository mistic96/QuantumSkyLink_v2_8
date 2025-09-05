using Refit;
using TokenService.Models.Requests;
using TokenService.Models.Responses;

namespace TokenService.Services.Interfaces;

[Headers("Accept: application/json", "X-API-Version: 1.0")]
public interface IQuantumLedgerClient
{
    /// <summary>
    /// Creates a new account in QuantumLedger with substitution keys
    /// </summary>
    [Post("/api/accounts/create")]
    Task<QuantumLedgerAccountCreationResponse> CreateAccountAsync(
        [Body] CreateQuantumLedgerAccountRequest request,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);

    /// <summary>
    /// Gets account information by external owner ID
    /// </summary>
    [Get("/api/accounts/external/{externalOwnerId}")]
    Task<QuantumLedgerAccount> GetAccountByExternalOwnerIdAsync(
        string externalOwnerId,
        [Query] string? vendorSystem = null,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);

    /// <summary>
    /// Gets account information by account ID
    /// </summary>
    [Get("/api/accounts/{accountId}")]
    Task<QuantumLedgerAccount> GetAccountAsync(
        Guid accountId,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);

    /// <summary>
    /// Creates a transaction in QuantumLedger
    /// </summary>
    [Post("/api/transactions/create")]
    Task<QuantumLedgerTransactionResponse> CreateTransactionAsync(
        [Body] CreateQuantumLedgerTransactionRequest request,
        [Header("X-Substitution-Signature")] string signature,
        [Header("X-Substitution-Key-Id")] string keyId);

    /// <summary>
    /// Gets transaction details
    /// </summary>
    [Get("/api/transactions/{transactionId}")]
    Task<QuantumLedgerTransactionResponse> GetTransactionAsync(
        Guid transactionId,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);

    /// <summary>
    /// Gets account balance
    /// </summary>
    [Get("/api/balances/{accountId}")]
    Task<QuantumLedgerBalanceResponse> GetAccountBalanceAsync(
        Guid accountId,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);

    /// <summary>
    /// Gets transaction history for an account
    /// </summary>
    [Get("/api/balances/{accountId}/transactions")]
    Task<List<QuantumLedgerTransactionResponse>> GetTransactionHistoryAsync(
        Guid accountId,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        [Header("X-Substitution-Signature")] string? signature = null,
        [Header("X-Substitution-Key-Id")] string? keyId = null);
}
