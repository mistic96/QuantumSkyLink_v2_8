# MoonPay Integration Documentation

## Overview

MoonPay is a fiat-to-crypto and crypto-to-fiat payment gateway that enables users to buy and sell cryptocurrencies using traditional payment methods. This integration provides a seamless on-ramp and off-ramp solution for cryptocurrency transactions.

## Features

### Supported Operations
- **Buy Crypto (On-ramp)**: Convert fiat currency to cryptocurrency
- **Sell Crypto (Off-ramp)**: Convert cryptocurrency to fiat currency
- **Widget Integration**: Embedded UI for seamless user experience
- **KYC/AML Screening**: Compliance checks for users
- **Real-time Exchange Rates**: Current market prices and quotes
- **Transaction Status Tracking**: Monitor payment progress
- **Webhook Notifications**: Real-time updates on transaction status

### Supported Currencies

#### Cryptocurrencies
- Bitcoin (BTC)
- Ethereum (ETH)
- Tether (USDT)
- USD Coin (USDC)
- Solana (SOL)
- Cardano (ADA)
- Polygon (MATIC)

#### Fiat Currencies
- US Dollar (USD)
- Euro (EUR)
- British Pound (GBP)
- Canadian Dollar (CAD)
- Australian Dollar (AUD)

### Payment Methods
- Credit/Debit Cards
- Bank Transfers
- SEPA Bank Transfers (EU)
- GBP Bank Transfers (UK)

## API Endpoints

### Buy Cryptocurrency
```http
POST /api/moonpay/buy
Authorization: Bearer {token}
Content-Type: application/json

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

### Sell Cryptocurrency
```http
POST /api/moonpay/sell
Authorization: Bearer {token}
Content-Type: application/json

{
  "cryptoCurrency": "BTC",
  "cryptoAmount": 0.01,
  "fiatCurrency": "USD",
  "email": "user@example.com",
  "bankAccount": {
    "accountNumber": "12345678",
    "accountHolderName": "John Doe",
    "bankName": "Example Bank",
    "sortCode": "123456"
  }
}
```

### Get Transaction Status
```http
GET /api/moonpay/transaction/{transactionId}
Authorization: Bearer {token}
```

### Get Supported Currencies
```http
GET /api/moonpay/currencies?fiatCurrency=USD
```

### Get Exchange Quote
```http
GET /api/moonpay/quote?cryptoCurrency=BTC&fiatCurrency=USD
```

### Create Widget Session
```http
POST /api/moonpay/widget
Authorization: Bearer {token}
Content-Type: application/json

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

### Screen Recipient (KYC/AML)
```http
POST /api/moonpay/screen
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": "user@example.com",
  "name": "John Doe",
  "dateOfBirth": "1990-01-01",
  "country": "US",
  "state": "CA",
  "ipAddress": "192.168.1.1"
}
```

### Get Transaction Limits
```http
GET /api/moonpay/limits?cryptoCurrency=BTC&fiatCurrency=USD&paymentMethod=credit_debit_card
```

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "MoonPay": {
    "ApiKey": "your-api-key",
    "SecretKey": "your-secret-key",
    "BaseUrl": "https://api.moonpay.com/v3/",
    "WebhookSecret": "your-webhook-secret",
    "Environment": "sandbox",
    "DefaultCurrency": "USD",
    "SupportedCryptoCurrencies": ["BTC", "ETH", "USDT", "USDC"],
    "SupportedFiatCurrencies": ["USD", "EUR", "GBP"],
    "SupportedPaymentMethods": ["credit_debit_card", "bank_transfer"],
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

## Webhook Integration

MoonPay sends webhook notifications to update transaction status. Configure your webhook endpoint:

```
https://yourapi.com/api/webhook/moonpay
```

### Webhook Events
- `transaction_created`: New transaction initiated
- `transaction_updated`: Transaction status changed
- `transaction_failed`: Transaction failed
- `customer_created`: New customer registered
- `customer_updated`: Customer information updated

### Webhook Security
All webhooks are signed using HMAC-SHA256. The signature is included in the `X-MoonPay-Signature` header.

## Implementation Details

### Service Architecture

1. **IMoonPayService**: Interface defining MoonPay operations
2. **MoonPayService**: Implementation handling API communication
3. **MoonPayController**: REST API endpoints for client applications
4. **GatewayIntegrationService**: Integration with payment processing pipeline
5. **WebhookController**: Handles MoonPay webhook notifications

### Transaction Flow

#### Buy Transaction Flow
1. User initiates buy request with amount and wallet address
2. System creates payment record with pending status
3. MoonPay API creates transaction and returns widget URL
4. User completes payment through MoonPay widget
5. MoonPay sends webhook notification on completion
6. System updates payment status and stores transaction details

#### Sell Transaction Flow
1. User initiates sell request with crypto amount and bank details
2. System creates withdrawal payment record
3. MoonPay API creates sell transaction
4. User sends crypto to MoonPay address
5. MoonPay processes fiat payout to bank account
6. Webhook notifications update transaction status

### Error Handling

The integration includes comprehensive error handling:
- API connection failures
- Invalid currency pairs
- KYC/AML rejection
- Transaction limits exceeded
- Signature verification failures

### Security Considerations

1. **API Authentication**: Secure API key and secret key storage
2. **Webhook Verification**: HMAC-SHA256 signature validation
3. **Data Encryption**: All sensitive data transmitted over HTTPS
4. **KYC/AML Compliance**: Built-in screening capabilities
5. **Idempotency**: External transaction ID prevents duplicates

## Testing

### Sandbox Environment
Use sandbox credentials for testing:
- Test credit cards provided by MoonPay
- Simulated KYC approval/rejection
- Test webhook notifications

### Test Scenarios
1. Successful buy transaction
2. Failed transaction (insufficient funds)
3. KYC rejection
4. Transaction limits exceeded
5. Webhook signature verification

## Monitoring and Maintenance

### Key Metrics
- Transaction success rate
- Average processing time
- API response times
- Webhook delivery success
- Currency availability

### Health Checks
The integration includes health check endpoints to monitor:
- API connectivity
- Authentication status
- Service availability

## Troubleshooting

### Common Issues

1. **Invalid API Key**
   - Verify API key in configuration
   - Ensure environment (sandbox/production) matches

2. **Transaction Failures**
   - Check transaction limits
   - Verify wallet address format
   - Ensure KYC compliance

3. **Webhook Issues**
   - Verify webhook URL is accessible
   - Check signature verification
   - Monitor webhook logs

4. **Currency Not Supported**
   - Verify currency pair availability
   - Check regional restrictions

## Support

For additional support:
- MoonPay Documentation: https://docs.moonpay.com
- API Reference: https://docs.moonpay.com/api
- Support Email: support@moonpay.com