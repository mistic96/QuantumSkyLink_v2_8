using System;
using System.ComponentModel.DataAnnotations;
using QuantumLedger.Models.Interfaces;

namespace QuantumLedger.Models
{
    /// <summary>
    /// Represents an account balance in the Quantum Ledger system.
    /// </summary>
    public class AccountBalance : ISurrealEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for this entity (same as Address)
        /// </summary>
        public string Id { get; set; }

  

        /// <summary>
        /// Gets the table name for SurrealDB
        /// </summary>
        public string TableName => "balances";

        /// <summary>
        /// Gets the namespace for SurrealDB
        /// </summary>
        public string Namespace => "ledger";

        /// <summary>
        /// Gets or sets the blockchain address of the account.
        /// </summary>
        [Required]
        public string Address
        {
            get => Id;
            set => Id = value;
        }

        /// <summary>
        /// Gets or sets the current balance of the account.
        /// The balance is stored with decimal precision to handle financial transactions accurately.
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp of the balance.
        /// </summary>
        [Required]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Validates that the account balance has all required fields and valid data.
        /// </summary>
        /// <returns>True if the account balance is valid; otherwise, false.</returns>
        public bool SolidState { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Address)) return false;
            if (Balance < 0) return false;
            if (LastUpdated == default) return false;
            
            return true;
        }

        /// <summary>
        /// Updates the balance by adding the specified amount.
        /// </summary>
        /// <param name="amount">The amount to add to the balance. Can be negative for deductions.</param>
        /// <returns>True if the update was successful; false if it would result in a negative balance.</returns>
        public bool UpdateBalance(decimal amount)
        {
            var newBalance = Balance + amount;
            if (newBalance < 0) return false;

            Balance = newBalance;
            LastUpdated = DateTime.UtcNow;
            return true;
        }
    }
}
