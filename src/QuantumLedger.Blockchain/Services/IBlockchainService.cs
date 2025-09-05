using System.Threading.Tasks;
using QuantumLedger.Blockchain.Models;

namespace QuantumLedger.Blockchain.Services
{
    /// <summary>
    /// Represents the result of a blockchain operation.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets or sets whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the result data if the operation was successful.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful result with the specified data.
        /// </summary>
        public static Result<T> Success(T data) => new Result<T> { IsSuccess = true, Data = data };

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        public static Result<T> Failure(string errorMessage) => new Result<T> { IsSuccess = false, ErrorMessage = errorMessage };
    }

    /// <summary>
    /// Defines the contract for blockchain service operations.
    /// </summary>
    public interface IBlockchainService
    {
        /// <summary>
        /// Submits a transaction to the blockchain.
        /// </summary>
        /// <param name="transaction">The transaction to submit.</param>
        /// <returns>A result containing the transaction ID if successful.</returns>
        Task<Result<string>> SubmitTransactionAsync(Transaction transaction);

        /// <summary>
        /// Gets the status of a transaction.
        /// </summary>
        /// <param name="txId">The transaction ID to check.</param>
        /// <returns>A result containing the transaction status if found.</returns>
        Task<Result<TransactionStatus>> GetTransactionStatusAsync(string txId);

        /// <summary>
        /// Gets information about a specific block.
        /// </summary>
        /// <param name="blockId">The block ID or number to query.</param>
        /// <returns>A result containing the block information if found.</returns>
        Task<Result<BlockInfo>> GetBlockInfoAsync(string blockId);

        /// <summary>
        /// Gets the current state of an account.
        /// </summary>
        /// <param name="address">The account address to query.</param>
        /// <returns>A result containing the account state if found.</returns>
        Task<Result<AccountState>> GetAccountStateAsync(string address);
    }
}
