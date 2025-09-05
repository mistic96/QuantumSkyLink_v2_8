using System;
using System.Collections.Generic;
using PaymentGatewayService.Data.Entities;

namespace PaymentGatewayService.Models.Square;

public class SquarePaymentRequest
{
    public required string SourceId { get; set; } // Token from Square Web/Apple/Google Pay
    public long AmountMoney { get; set; }         // Minor units (e.g., cents)
    public string Currency { get; set; } = "USD";
    public required string IdempotencyKey { get; set; } // Typically payment.Id
    public string? LocationId { get; set; }
    public string? OrderId { get; set; }
    public SquareBuyerDetails? Buyer { get; set; }
    public string? Note { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SquareBuyerDetails
{
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public SquareAddress? Address { get; set; }
}

public class SquareAddress
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? Locality { get; set; } // City
    public string? AdministrativeDistrictLevel1 { get; set; } // State/Province
    public string? PostalCode { get; set; }
    public string? Country { get; set; } // ISO 3166-1 alpha-2
}

public static class MoneyConverter
{
    // Common currency decimal places. Defaults to 2 if unknown.
    private static readonly Dictionary<string, int> CurrencyMinorUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BHD"] = 3, ["JOD"] = 3, ["KWD"] = 3, ["OMR"] = 3, ["TND"] = 3,
        ["JPY"] = 0, ["KRW"] = 0, ["VND"] = 0
    };

    public static long ToMinorUnits(decimal amount, string currency)
    {
        var decimals = CurrencyMinorUnits.TryGetValue(currency, out var d) ? d : 2;
        var factor = (decimal)Math.Pow(10, decimals);
        return (long)Math.Round(amount * factor, MidpointRounding.AwayFromZero);
    }

    public static decimal FromMinorUnits(long minor, string currency)
    {
        var decimals = CurrencyMinorUnits.TryGetValue(currency, out var d) ? d : 2;
        var factor = (decimal)Math.Pow(10, decimals);
        return minor / factor;
    }
}

public static class SquareStatusMapper
{
    public static Data.Entities.PaymentStatus MapPaymentStatus(string? squareStatus)
    {
        if (string.IsNullOrWhiteSpace(squareStatus))
            return Data.Entities.PaymentStatus.Processing;

        switch (squareStatus.Trim().ToUpperInvariant())
        {
            case "COMPLETED":
                return Data.Entities.PaymentStatus.Completed;
            case "APPROVED":
                // Authorized but not captured yet; treat as processing in our domain
                return Data.Entities.PaymentStatus.Processing;
            case "PENDING":
                return Data.Entities.PaymentStatus.Pending;
            case "CANCELED":
            case "CANCELLED":
                return Data.Entities.PaymentStatus.Cancelled;
            case "FAILED":
                return Data.Entities.PaymentStatus.Failed;
            default:
                return Data.Entities.PaymentStatus.Processing;
        }
    }
}
