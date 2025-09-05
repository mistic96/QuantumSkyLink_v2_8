using System;
using QuantumLedger.Blockchain.Models;

namespace QuantumLedger.Blockchain.Events
{
    /// <summary>
    /// Base class for all blockchain events.
    /// </summary>
    public abstract class BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the event ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the event type.
        /// </summary>
        public string EventType { get; set; }
    }

    /// <summary>
    /// Event raised when a transaction is submitted.
    /// </summary>
    public class TransactionSubmittedEvent : BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the submitted transaction.
        /// </summary>
        public Transaction Transaction { get; set; }

        public TransactionSubmittedEvent()
        {
            EventType = "TransactionSubmitted";
        }
    }

    /// <summary>
    /// Event raised when a transaction status changes.
    /// </summary>
    public class TransactionStatusChangedEvent : BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the previous status.
        /// </summary>
        public string PreviousStatus { get; set; }

        /// <summary>
        /// Gets or sets the new status.
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// Gets or sets the block number if the transaction was included in a block.
        /// </summary>
        public long? BlockNumber { get; set; }

        public TransactionStatusChangedEvent()
        {
            EventType = "TransactionStatusChanged";
        }
    }

    /// <summary>
    /// Event raised when a new block is created.
    /// </summary>
    public class BlockCreatedEvent : BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the created block.
        /// </summary>
        public BlockInfo Block { get; set; }

        public BlockCreatedEvent()
        {
            EventType = "BlockCreated";
        }
    }

    /// <summary>
    /// Event raised when an account state changes.
    /// </summary>
    public class AccountStateChangedEvent : BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the account address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the previous balance.
        /// </summary>
        public decimal PreviousBalance { get; set; }

        /// <summary>
        /// Gets or sets the new balance.
        /// </summary>
        public decimal NewBalance { get; set; }

        /// <summary>
        /// Gets or sets the transaction ID that caused the change.
        /// </summary>
        public string TransactionId { get; set; }

        public AccountStateChangedEvent()
        {
            EventType = "AccountStateChanged";
        }
    }

    /// <summary>
    /// Event raised when an error occurs in the blockchain.
    /// </summary>
    public class BlockchainErrorEvent : BlockchainEvent
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the related transaction ID if applicable.
        /// </summary>
        public string RelatedTransactionId { get; set; }

        public BlockchainErrorEvent()
        {
            EventType = "BlockchainError";
        }
    }
}
