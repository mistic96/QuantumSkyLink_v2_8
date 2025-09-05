# MoonPay Integration for PaymentGatewayService

## Overview

MoonPay is a fiat-to-crypto and crypto-to-fiat payment gateway integrated into the QuantumSkyLink PaymentGatewayService. This integration enables users to buy and sell cryptocurrencies using traditional payment methods, providing a seamless on-ramp and off-ramp solution for cryptocurrency transactions.

## Table of Contents

- [Features](#features)
- [Supported Currencies](#supported-currencies)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Implementation Guide](#implementation-guide)
- [Webhook Integration](#webhook-integration)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

## Features

### Core Capabilities
- **Fiat-to-Crypto (On-ramp)**: Buy cryptocurrency with credit cards or bank transfers
- **Crypto-to-Fiat (Off-ramp)**: Sell cryptocurrency for fiat currency
- **Embeddable Widget**: Seamless UI integration for web and mobile
- **KYC/AML Compliance**: Built-in identity verification and screening
- **Real-time Quotes**: Live exchange rates and fee calculations
- **Multi-currency Support**: 7+ cryptocurrencies and 5+ fiat currencies
- **Transaction Tracking**: Real-time status updates via webhooks

### Key Benefits
- **Global Coverage**: Support for users in 150+ countries
- **Fast Processing**: Instant card payments, same-day bank transfers
- **Regulatory Compliance**: Licensed in US, UK, EU, and other jurisdictions
- **High Conversion Rates**: Optimized checkout flow
- **Flexible Integration**: API, SDK, and widget options

## Supported Currencies

### Cryptocurrencies
| Currency | Symbol | Network |
|----------|--------|---------|
| Bitcoin | BTC | Bitcoin |
| Ethereum | ETH | Ethereum |
| Tether | USDT | Ethereum/Tron |
| USD Coin | USDC | Ethereum/Solana |
| Solana | SOL | Solana |
| Cardano | ADA | Cardano |
| Polygon | MATIC | Polygon |

### Fiat Currencies
| Currency | Code | Payment Methods |
|----------|------|-----------------|
| US Dollar | USD | Cards, ACH, Wire |
| Euro | EUR | Cards, SEPA |
| British Pound | GBP | Cards, FPS |
| Canadian Dollar | CAD | Cards, Interac |
| Australian Dollar | AUD | Cards, PayID |

## API Endpoints

All endpoints require JWT authentication unless specified otherwise.

### 1. Buy Cryptocurrency
Create a fiat-to-crypto transaction.

**Endpoint:** `POST /api/moonpay/buy`

**Request:**
```json
{
  "cryptoCurrency": "BTC",
  "amount": 100.00,
  "fiatCurrency": "USD",
  "walletAddress": "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa",
  "email": "user@example.com",
  "paymentMethod": "credit_debit_card",
  "includeFees": false,
  "returnUrl": "https://yourapp.com/payment/success",
  "theme": {
    "primaryColor": "#0052FF",
    "backgroundColor": "#FFFFFF",
    "borderRadius": "8px"
  }
}
```

**Response:**
```json
{
  "id": "txn_123456789",
  "status": "pending",
  "cryptoAmount": 0.0025,
  "fiatAmount": 100.00,
  "exchangeRate": 40000,
  "feeAmount": 4.99,
  "widgetUrl": "https://buy.moonpay.com/?txn=txn_123456789",
  "createdAt": "2024-01-20T10:30:00Z"
}
```

### 2. Sell Cryptocurrency
Create a crypto-to-fiat transaction.

**Endpoint:** `POST /api/moonpay/sell`

**Request:**
```json
{
  "cryptoCurrency": "BTC",
  "cryptoAmount": 0.01,
  "fiatCurrency": "USD",
  "email": "user@example.com",
  "bankAccount": {
    "accountNumber": "12345678",
    "accountHolderName": "John Doe",
    "bankName": "Chase Bank",
    "sortCode": "123456",
    "iban": "US12345678901234567890"
  }
}
```

### 3. Get Transaction Status
Monitor transaction progress.

**Endpoint:** `GET /api/moonpay/transaction/{transactionId}`

**Response:**
```json
{
  "id": "txn_123456789",
  "status": "completed",
  "statusDescription": "Transaction completed successfully",
  "stages": [
    {
      "stage": "payment_completed",
      "status": "completed"
    },
    {
      "stage": "crypto_purchased",
      "status": "completed"
    },
    {
      "stage": "crypto_sent",
      "status": "completed"
    }
  ],
  "cryptoTransactionId": "0xabc123...",
  "estimatedArrivalTime": "2024-01-20T10:35:00Z"
}
```

### 4. Get Supported Currencies
List available cryptocurrencies.

**Endpoint:** `GET /api/moonpay/currencies?fiatCurrency=USD`  
**Authentication:** Not required

### 5. Get Exchange Quote
Get current exchange rates.

**Endpoint:** `GET /api/moonpay/quote?cryptoCurrency=BTC&fiatCurrency=USD`  
**Authentication:** Not required

### 6. Create Widget Session
Generate embeddable widget URL.

**Endpoint:** `POST /api/moonpay/widget`

**Request:**
```json
{
  "flow": "buy",
  "defaultCryptoCurrency": "BTC",
  "defaultAmount": 100,
  "walletAddress": "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa",
  "email": "user@example.com",
  "language": "en",
  "colorMode": "light",
  "redirectUrl": "https://yourapp.com/payment/complete"
}
```

### 7. KYC/AML Screening
Verify user compliance.

**Endpoint:** `POST /api/moonpay/screen`

### 8. Get Transaction Limits
Check min/max amounts.

**Endpoint:** `GET /api/moonpay/limits?cryptoCurrency=BTC&fiatCurrency=USD&paymentMethod=credit_debit_card`  
**Authentication:** Not required

## Configuration

### Application Settings

Add to `appsettings.json`:

```json
{
  "MoonPay": {
    "ApiKey": "pk_test_123456789",
    "SecretKey": "sk_test_987654321",
    "BaseUrl": "https://api.moonpay.com/v3/",
    "WebhookSecret": "whsec_abcdef123456",
    "Environment": "sandbox",
    "DefaultCurrency": "USD",
    "SupportedCryptoCurrencies": ["BTC", "ETH", "USDT", "USDC", "SOL", "ADA", "MATIC"],
    "SupportedFiatCurrencies": ["USD", "EUR", "GBP", "CAD", "AUD"],
    "SupportedPaymentMethods": ["credit_debit_card", "bank_transfer", "sepa_bank_transfer"],
    "WidgetTheme": {
      "primaryColor": "#0052FF",
      "backgroundColor": "#FFFFFF",
      "borderRadius": "8px"
    },
    "Limits": {
      "MinBuyAmount": 20,
      "MaxBuyAmount": 20000,
      "MinSellAmount": 20,
      "MaxSellAmount": 10000
    }
  }
}
```

### Environment Variables

For production deployment:

```bash
MoonPay__ApiKey="pk_live_123456789"
MoonPay__SecretKey="sk_live_987654321"
MoonPay__WebhookSecret="whsec_live_abcdef123456"
MoonPay__Environment="production"
```

## Implementation Guide

### Service Registration

The MoonPay service is automatically registered in `Program.cs`:

```csharp
// HTTP client configuration
builder.Services.AddHttpClient<IMoonPayService, MoonPayService>(client =>
{
    var moonPayConfig = builder.Configuration.GetSection("MoonPay");
    var baseUrl = moonPayConfig["BaseUrl"] ?? "https://api.moonpay.com/v3/";
    
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "PaymentGatewayService/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service registration
builder.Services.AddScoped<IMoonPayService, MoonPayService>();
```

### Gateway Integration

MoonPay is integrated into the payment processing pipeline:

```csharp
// In GatewayIntegrationService
PaymentGatewayType.MoonPay => await ExecuteMoonPayPaymentAsync(payment)
```

### Transaction Processing

#### Buy Flow
1. User selects cryptocurrency and amount
2. System creates payment record
3. MoonPay API generates transaction
4. User redirected to MoonPay widget
5. User completes payment
6. Webhook confirms transaction
7. Crypto delivered to wallet

#### Sell Flow
1. User selects crypto amount to sell
2. System creates withdrawal record
3. MoonPay provides deposit address
4. User sends crypto
5. MoonPay processes fiat payout
6. Webhook confirms completion

## Webhook Integration

### Endpoint Configuration

Configure webhook URL in MoonPay dashboard:
```
https://yourapi.com/api/webhook/moonpay
```

### Webhook Events

| Event | Description |
|-------|-------------|
| `transaction_created` | New transaction initiated |
| `transaction_updated` | Status change (pending → completed) |
| `transaction_failed` | Transaction failed |
| `customer_created` | New user registered |
| `customer_updated` | KYC status updated |

### Signature Verification

All webhooks are signed using HMAC-SHA256:

```csharp
// Signature included in X-MoonPay-Signature header
var signature = Request.Headers["X-MoonPay-Signature"];
var isValid = await _moonPayService.ProcessWebhookAsync(payload, signature);
```

### Webhook Processing

The `WebhookController` automatically:
1. Validates webhook signature
2. Stores webhook in database
3. Updates payment status
4. Creates payment attempt record
5. Returns acknowledgment

## Testing

### Sandbox Environment

Use sandbox credentials for testing:

**Test Credit Cards:**
- Success: `4242 4242 4242 4242` (any expiry/CVV)
- 3D Secure: `4000 0025 0000 3155`
- Decline: `4000 0000 0000 9995`

**Test Bank Accounts:**
- US: Routing `110000000`, Account `000123456789`
- UK: Sort Code `108800`, Account `00012345`

### Test Scenarios

1. **Successful Purchase**
   - Use test success card
   - Complete KYC with test data
   - Transaction completes in ~30 seconds

2. **KYC Rejection**
   - Use email `rejected@moonpay.com`
   - Transaction fails with KYC error

3. **Transaction Limits**
   - Try amount below $20 (minimum)
   - Try amount above $20,000 (maximum)

4. **Webhook Testing**
   - Use webhook test tool in dashboard
   - Verify signature validation
   - Check payment status updates

## Troubleshooting

### Common Issues

#### 1. API Authentication Errors
**Error:** `401 Unauthorized`
**Solution:**
- Verify API key format (`pk_test_` for sandbox)
- Check environment configuration
- Ensure headers are properly set

#### 2. Invalid Wallet Address
**Error:** `Invalid wallet address format`
**Solution:**
- Validate address checksum
- Ensure correct network selected
- Use proper address format for currency

#### 3. Transaction Failures
**Error:** `Transaction failed`
**Possible Causes:**
- Card declined by issuer
- KYC/AML rejection
- Transaction limits exceeded
- Unsupported country/region

#### 4. Webhook Signature Mismatch
**Error:** `Invalid webhook signature`
**Solution:**
- Verify webhook secret configuration
- Check for trailing whitespace
- Ensure raw body is used for verification

### Debug Logging

Enable detailed logging for troubleshooting:

```json
{
  "Logging": {
    "LogLevel": {
      "PaymentGatewayService.Services.Integrations.MoonPayService": "Debug"
    }
  }
}
```

### Health Monitoring

Monitor MoonPay integration health:
- API response times
- Transaction success rates
- Webhook delivery rates
- Currency availability

## Security Best Practices

1. **API Key Security**
   - Store keys in secure configuration
   - Use environment variables in production
   - Rotate keys regularly

2. **Webhook Security**
   - Always verify signatures
   - Use HTTPS endpoints only
   - Implement replay protection

3. **Data Protection**
   - Never log sensitive data
   - Encrypt stored credentials
   - Follow PCI compliance

4. **Rate Limiting**
   - Implement request throttling
   - Handle 429 responses gracefully
   - Use exponential backoff

## Support Resources

- **Documentation**: https://docs.moonpay.com
- **API Reference**: https://docs.moonpay.com/api
- **Status Page**: https://status.moonpay.com
- **Support Email**: support@moonpay.com
- **Integration Help**: integrations@moonpay.com

## License

This integration is part of the QuantumSkyLink PaymentGatewayService.  
Copyright © 2024 QuantumSkyLink. All rights reserved.