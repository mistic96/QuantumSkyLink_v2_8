using System.Threading.Tasks;

namespace QuantumLedger.Models.Blockchain
{
    /// <summary>
    /// Defines the contract for handling blockchain events.
    /// </summary>
    public interface IBlockchainEventHandler
    {
        /// <summary>
        /// Handles a transaction submitted event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        Task HandleTransactionSubmittedAsync(TransactionSubmittedEvent evt);

        /// <summary>
        /// Handles a transaction status changed event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        Task HandleTransactionStatusChangedAsync(TransactionStatusChangedEvent evt);

        /// <summary>
        /// Handles a block created event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        Task HandleBlockCreatedAsync(BlockCreatedEvent evt);

        /// <summary>
        /// Handles an account state changed event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        Task HandleAccountStateChangedAsync(AccountStateChangedEvent evt);

        /// <summary>
        /// Handles a blockchain error event.
        /// </summary>
        /// <param name="evt">The event to handle.</param>
        Task HandleBlockchainErrorAsync(BlockchainErrorEvent evt);
    }
}
