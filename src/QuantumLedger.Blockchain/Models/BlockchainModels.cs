using System;
using System.Collections.Generic;

namespace QuantumLedger.Blockchain.Models
{
    /// <summary>
    /// Represents a transaction in the blockchain.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Gets or sets the unique identifier for the transaction.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the sender's address.
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient's address.
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Gets or sets the transaction amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the transaction.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the transaction.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Represents the status of a transaction.
    /// </summary>
    public class TransactionStatus
    {
        /// <summary>
        /// Gets or sets the transaction ID.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the current status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the block number where the transaction was included.
        /// </summary>
        public long? BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the last status update.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets any error message if the transaction failed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Represents information about a block in the blockchain.
    /// </summary>
    public class BlockInfo
    {
        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        public long Number { get; set; }

        /// <summary>
        /// Gets or sets the block's hash.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the previous block's hash.
        /// </summary>
        public string PreviousHash { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the block was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the list of transaction IDs in this block.
        /// </summary>
        public List<string> TransactionIds { get; set; }
    }

    /// <summary>
    /// Represents the current state of an account.
    /// </summary>
    public class AccountState
    {
        /// <summary>
        /// Gets or sets the account's address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the current balance.
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the account's nonce (transaction count).
        /// </summary>
        public uint Nonce { get; set; }

        /// <summary>
        /// Gets or sets additional metadata for the account.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Represents common status values for transactions.
    /// </summary>
    public static class TransactionStatuses
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Failed = "Failed";
        public const string Rejected = "Rejected";
    }
}
