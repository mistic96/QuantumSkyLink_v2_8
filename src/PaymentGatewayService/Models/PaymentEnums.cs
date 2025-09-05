namespace PaymentGatewayService.Models
{
    /// <summary>
    /// Represents the type of payment transaction (models facade).
    /// This mirrors the canonical definitions in Data.Entities to satisfy references that use the Models namespace.
    /// Keep in sync with PaymentGatewayService.Data.Entities.PaymentType.
    /// </summary>
    public enum PaymentType
    {
        Deposit = 0,
        Withdrawal = 1,
        Transfer = 2,
        Fee = 3,
        Refund = 4,
        Crypto = 5
    }

    /// <summary>
    /// Lightweight stats holder used by some response models.
    /// </summary>
    public class PaymentTypeStatistics
    {
        public string Currency { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Volume { get; set; }
    }
}
