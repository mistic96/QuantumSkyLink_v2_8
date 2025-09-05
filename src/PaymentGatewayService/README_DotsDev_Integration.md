# Dots.dev Integration for PaymentGatewayService

## Overview
This document describes the Dots.dev global payout integration in the PaymentGatewayService, enabling payouts to 190+ countries with automatic payment method selection.

## Supported Regions and Payment Methods

### United States 🇺🇸
- **Venmo** - Popular P2P payment app
- **PayPal** - Global payment platform
- **Cash App** - Square's P2P payment service
- **ACH Bank Transfer** - Direct bank deposits
- **Wire Transfer** - For larger amounts

### Europe 🇪🇺
- **SEPA Transfer** - Single Euro Payments Area (EU countries)
- **Local Bank Transfers** - Country-specific banking
- **PayPal** - Available in most EU countries
- **Wire Transfers** - International transfers

### India 🇮🇳
- **UPI** - Unified Payments Interface
- **IMPS** - Immediate Payment Service
- **NEFT/RTGS** - National Electronic Funds Transfer
- **Local Bank Transfers** - All major Indian banks

### Philippines 🇵🇭
- **GCash** - Leading mobile wallet
- **PayMaya** - Digital payment platform
- **Bank Transfers** - Local bank networks
- **Remittance Centers** - Cash pickup locations

### Other Supported Regions
- **Brazil** 🇧🇷 - PIX, Bank transfers
- **Mexico** 🇲🇽 - SPEI, Bank transfers
- **Canada** 🇨🇦 - Interac, Bank transfers
- **Australia** 🇦🇺 - BPAY, Bank transfers
- **Japan** 🇯🇵 - Bank transfers
- **190+ Countries** - With local payment methods

## API Endpoints

### DotsDevController (`/api/dotsdev`)

#### Create International Payout
```http
POST /api/dotsdev/payout
Authorization: Bearer {token}
```

Request:
```json
{
  "amount": 1000.50,
  "currency": "USD",
  "recipientName": "John Doe",
  "recipientEmail": "john@example.com",
  "recipientCountry": "US",
  "recipientPhone": "+1234567890",
  "preferredPaymentMethod": "ACH",
  "description": "Monthly payout",
  "paymentDetails": {
    "accountNumber": "****1234",
    "routingNumber": "****5678"
  },
  "complianceData": {
    "taxId": "XXX-XX-1234",
    "dateOfBirth": "1990-01-01",
    "address": {
      "line1": "123 Main St",
      "city": "New York",
      "state": "NY",
      "postalCode": "10001",
      "country": "US"
    }
  }
}
```

Response:
```json
{
  "id": "payout_abc123",
  "status": "processing",
  "amountInCents": 100050,
  "currency": "USD",
  "paymentMethod": "ACH",
  "recipient": {
    "name": "John Doe",
    "email": "john@example.com",
    "country": "US"
  },
  "estimatedDelivery": "2024-01-22T15:00:00Z",
  "fees": {
    "platformFeeInCents": 250,
    "paymentMethodFeeInCents": 100,
    "totalFeeInCents": 350,
    "netAmountInCents": 99700
  },
  "createdAt": "2024-01-20T10:00:00Z"
}
```

#### Create Onboarding Flow
```http
POST /api/dotsdev/flow
Authorization: Bearer {token}
```

Request:
```json
{
  "flowType": "payout_onboarding",
  "userData": {
    "name": "John Doe",
    "email": "john@example.com",
    "country": "US"
  },
  "redirectUrl": "https://myapp.com/onboarding/complete",
  "theme": {
    "primaryColor": "#007bff",
    "logoUrl": "https://myapp.com/logo.png",
    "companyName": "QuantumSkyLink"
  }
}
```

Response:
```json
{
  "flowId": "flow_xyz789",
  "flowUrl": "https://app.dots.dev/flow/xyz789",
  "expiresAt": "2024-01-20T12:00:00Z",
  "status": "pending"
}
```

#### Get Flow Status
```http
GET /api/dotsdev/flow/{flowId}
Authorization: Bearer {token}
```

#### Get Supported Countries
```http
GET /api/dotsdev/countries
```

Response:
```json
[
  {
    "countryCode": "US",
    "countryName": "United States",
    "currencies": ["USD"],
    "paymentMethods": [
      {
        "method": "ACH",
        "displayName": "ACH Bank Transfer",
        "minAmountInCents": 100,
        "maxAmountInCents": 10000000,
        "fees": {
          "fixedFeeInCents": 100,
          "percentageFee": 0.5
        }
      },
      {
        "method": "VENMO",
        "displayName": "Venmo",
        "minAmountInCents": 100,
        "maxAmountInCents": 300000
      }
    ],
    "requiredFields": ["name", "email", "taxId", "address"],
    "deliveryEstimates": {
      "ACH": "1-3 business days",
      "VENMO": "Instant"
    }
  }
]
```

#### Get Country Payment Methods
```http
GET /api/dotsdev/methods/{country}
```

#### Validate Recipient
```http
POST /api/dotsdev/validate
Authorization: Bearer {token}
```

Request:
```json
{
  "country": "US",
  "recipientData": {
    "name": "John Doe",
    "email": "john@example.com",
    "taxId": "123-45-6789"
  }
}
```

## Webhook Integration

Dots.dev sends webhooks to notify about payout status changes:

### Webhook Endpoint
```
POST https://your-domain.com/api/webhook/dotsdev
```

### Webhook Events
- `payout.completed` - Payout successfully delivered
- `payout.processing` - Payout is being processed
- `payout.failed` - Payout failed
- `flow.completed` - Onboarding flow completed
- `flow.abandoned` - User abandoned the flow

### Webhook Payload Example
```json
{
  "event": "payout.completed",
  "eventId": "evt_123456",
  "timestamp": "2024-01-20T11:00:00Z",
  "data": {
    "payout_id": "payout_abc123",
    "status": "completed",
    "amount": 100050,
    "currency": "USD",
    "recipient": {
      "name": "John Doe",
      "country": "US"
    }
  }
}
```

## Configuration

Add the following to your `appsettings.json`:

```json
{
  "DotsDev": {
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "BaseUrl": "https://api.dots.dev/v2/",
    "WebhookSecret": "your-webhook-secret",
    "Environment": "sandbox",
    "DefaultCurrency": "USD",
    "SupportedRegions": ["US", "EU", "IN", "PH", "GLOBAL"],
    "ComplianceLevel": "standard"
  }
}
```

## Regional Payout Examples

### US Payout (ACH)
```json
{
  "amount": 500.00,
  "currency": "USD",
  "recipientCountry": "US",
  "paymentDetails": {
    "accountNumber": "123456789",
    "routingNumber": "021000021",
    "accountType": "checking"
  }
}
```

### Europe Payout (SEPA)
```json
{
  "amount": 500.00,
  "currency": "EUR",
  "recipientCountry": "DE",
  "paymentDetails": {
    "iban": "DE89370400440532013000",
    "bic": "COBADEFFXXX"
  }
}
```

### India Payout (UPI)
```json
{
  "amount": 40000.00,
  "currency": "INR",
  "recipientCountry": "IN",
  "paymentDetails": {
    "upiId": "john.doe@paytm",
    "panNumber": "ABCDE1234F"
  }
}
```

### Philippines Payout (GCash)
```json
{
  "amount": 25000.00,
  "currency": "PHP",
  "recipientCountry": "PH",
  "paymentDetails": {
    "gcashNumber": "+639123456789"
  }
}
```

## Compliance Requirements

### United States
- **Required**: SSN or EIN (Tax ID)
- **Optional**: Address verification
- **Limits**: $10,000 per transaction for ACH

### Europe (SEPA)
- **Required**: IBAN, Full name
- **Optional**: BIC/SWIFT for some banks
- **Limits**: €15,000 per transaction

### India
- **Required**: PAN number, Full name
- **Optional**: Aadhaar for higher limits
- **Limits**: ₹200,000 per transaction

### Philippines
- **Required**: Full name, Valid ID
- **Optional**: TIN for business payouts
- **Limits**: ₱50,000 per transaction

## Error Handling

Common error scenarios:
- `INVALID_RECIPIENT` - Recipient validation failed
- `UNSUPPORTED_COUNTRY` - Country not supported
- `COMPLIANCE_REQUIRED` - Additional KYC needed
- `LIMIT_EXCEEDED` - Transaction limit exceeded
- `INVALID_PAYMENT_METHOD` - Method not available for country

## Testing

### Sandbox Testing
Use sandbox environment for testing:
- Test API keys provided by Dots.dev
- Simulated payouts with instant status updates
- Test recipient data validation

### Test Data
- US Test SSN: 123-45-6789
- EU Test IBAN: DE89370400440532013000
- India Test PAN: ABCDE1234F
- Philippines Test Number: +639123456789

## Monitoring

Monitor Dots.dev payouts through:
- Payment status in `/api/payment/{id}`
- Webhook status in `/api/webhook/status/{paymentId}`
- Gateway health checks in `/api/paymentgateway/{id}/statistics`

## Best Practices

1. **Validate Recipients** - Always validate before creating payouts
2. **Use Idempotency Keys** - Prevent duplicate payouts
3. **Handle Webhooks** - Don't rely only on synchronous responses
4. **Store Compliance Data** - Keep records for regulatory requirements
5. **Monitor Fees** - Track platform and method-specific fees
6. **Test Thoroughly** - Test each region/method in sandbox

## Support

For Dots.dev specific issues:
- Documentation: [docs.dots.dev](https://docs.dots.dev)
- Support: support@dots.dev
- Status: [status.dots.dev](https://status.dots.dev)