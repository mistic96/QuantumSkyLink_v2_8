using FeeService.Models.Requests;
using FeeService.Models.Responses;

namespace FeeService.Services.Interfaces;

public interface IFeeCollectionService
{
    /// <summary>
    /// Collect fee from a user
    /// </summary>
    Task<FeeTransactionResponse> CollectFeeAsync(CollectFeeRequest request);

    /// <summary>
    /// Process refund for a fee transaction
    /// </summary>
    Task<FeeTransactionResponse> ProcessRefundAsync(ProcessRefundRequest request);

    /// <summary>
    /// Get fee transaction details
    /// </summary>
    Task<FeeTransactionResponse> GetTransactionAsync(Guid transactionId);

    /// <summary>
    /// Get fee transaction history for a user
    /// </summary>
    Task<IEnumerable<FeeTransactionResponse>> GetTransactionHistoryAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Update transaction status
    /// </summary>
    Task<FeeTransactionResponse> UpdateTransactionStatusAsync(Guid transactionId, string status, string? reason = null);

    /// <summary>
    /// Generate receipt for a transaction
    /// </summary>
    Task<ReceiptResponse> GenerateReceiptAsync(Guid transactionId);

    /// <summary>
    /// Validate payment method
    /// </summary>
    Task<bool> ValidatePaymentMethodAsync(string paymentMethod, Guid userId);

    /// <summary>
    /// Get pending transactions
    /// </summary>
    Task<IEnumerable<FeeTransactionResponse>> GetPendingTransactionsAsync(int page = 1, int pageSize = 20);
}
