using PaymentGatewayService.Data.Entities;
using Model = PaymentGatewayService.Models;

namespace PaymentGatewayService.Utils
{
    /// <summary>
    /// Centralized mapping between model enums and data/entity enums.
    /// Keeps mapping logic in one place to avoid ambiguous references.
    /// </summary>
    public static class StatusMapper
    {
        // PaymentStatus mappings
        public static PaymentStatus ToEntity(Model.PaymentStatus status)
        {
            return status switch
            {
                Model.PaymentStatus.Pending => PaymentStatus.Pending,
                Model.PaymentStatus.Processing => PaymentStatus.Processing,
                Model.PaymentStatus.Completed => PaymentStatus.Completed,
                Model.PaymentStatus.Failed => PaymentStatus.Failed,
                Model.PaymentStatus.Cancelled => PaymentStatus.Cancelled,
                _ => PaymentStatus.Processing
            };
        }

        public static Model.PaymentStatus ToModel(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => Model.PaymentStatus.Pending,
                PaymentStatus.Processing => Model.PaymentStatus.Processing,
                PaymentStatus.Completed => Model.PaymentStatus.Completed,
                PaymentStatus.Failed => Model.PaymentStatus.Failed,
                PaymentStatus.Cancelled => Model.PaymentStatus.Cancelled,
                PaymentStatus.Refunded => Model.PaymentStatus.Completed, // map refunded -> completed for model simplicity
                PaymentStatus.PartiallyRefunded => Model.PaymentStatus.Processing,
                _ => Model.PaymentStatus.Processing
            };
        }

        // PaymentType mappings
        public static PaymentType ToEntity(Model.PaymentType type)
        {
            return type switch
            {
                Model.PaymentType.Deposit => PaymentType.Deposit,
                Model.PaymentType.Withdrawal => PaymentType.Withdrawal,
                Model.PaymentType.Transfer => PaymentType.Transfer,
                Model.PaymentType.Fee => PaymentType.Fee,
                Model.PaymentType.Refund => PaymentType.Refund,
                Model.PaymentType.Crypto => PaymentType.Crypto,
                _ => PaymentType.Deposit
            };
        }

        public static Model.PaymentType ToModel(PaymentType type)
        {
            return type switch
            {
                PaymentType.Deposit => Model.PaymentType.Deposit,
                PaymentType.Withdrawal => Model.PaymentType.Withdrawal,
                PaymentType.Transfer => Model.PaymentType.Transfer,
                PaymentType.Fee => Model.PaymentType.Fee,
                PaymentType.Refund => Model.PaymentType.Refund,
                PaymentType.Crypto => Model.PaymentType.Crypto,
                _ => Model.PaymentType.Deposit
            };
        }
    }

    /// <summary>
    /// Extension helpers to support usage like status.ToEntity() and status.ToModel()
    /// placed next to StatusMapper for cohesion.
    /// </summary>
    public static class StatusMapperExtensions
    {
        public static PaymentStatus ToEntity(this Model.PaymentStatus status) => StatusMapper.ToEntity(status);

        public static Model.PaymentStatus ToModel(this PaymentStatus status) => StatusMapper.ToModel(status);

        public static PaymentType ToEntity(this Model.PaymentType type) => StatusMapper.ToEntity(type);

        public static Model.PaymentType ToModel(this PaymentType type) => StatusMapper.ToModel(type);

        /// <summary>
        /// Map a gateway-provided status string to the canonical entity PaymentStatus.
        /// This helps callers that receive strings from external gateways.
        /// </summary>
        public static PaymentStatus ToEntity(this string? gatewayStatus)
        {
            if (string.IsNullOrWhiteSpace(gatewayStatus))
                return PaymentStatus.Processing;

            switch (gatewayStatus.Trim().ToUpperInvariant())
            {
                case "COMPLETED":
                case "SUCCEEDED":
                case "SUCCESS":
                case "REFUNDED":
                    return PaymentStatus.Completed;
                case "REFUND":
                case "REFUNDED_PARTIAL":
                case "PARTIALLY_REFUNDED":
                case "PARTIAL_REFUND":
                    return PaymentStatus.PartiallyRefunded;
                case "FAILED":
                case "FAILURE":
                case "DECLINED":
                case "REJECTED":
                    return PaymentStatus.Failed;
                case "CANCELLED":
                case "CANCELED":
                case "VOIDED":
                    return PaymentStatus.Cancelled;
                case "PENDING":
                case "AUTHORIZED":
                case "APPROVED":
                    return PaymentStatus.Pending;
                case "PROCESSING":
                case "IN_PROGRESS":
                    return PaymentStatus.Processing;
                default:
                    return PaymentStatus.Processing;
            }
        }

        /// <summary>
        /// Convenience: convert a Model.PaymentStatus (if present as nullable) to entity.
        /// </summary>
        public static PaymentStatus ToEntity(this Model.PaymentStatus? status) => status.HasValue ? StatusMapper.ToEntity(status.Value) : PaymentStatus.Processing;
    }
}
