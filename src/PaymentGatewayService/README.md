# PaymentGatewayService

A comprehensive payment processing microservice for QuantumSkyLink v2 that supports multiple payment gateways, currencies, and payment methods. Built with .NET 9 and follows microservices architecture patterns.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Supported Payment Gateways](#supported-payment-gateways)
- [API Documentation](#api-documentation)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Authentication & Authorization](#authentication--authorization)
- [Integration Examples](#integration-examples)
- [Webhooks](#webhooks)
- [Testing](#testing)
- [Monitoring & Health Checks](#monitoring--health-checks)
- [Deployment](#deployment)

## Overview

The PaymentGatewayService is a critical component of the QuantumSkyLink v2 platform that handles all payment-related operations. It provides a unified interface for processing payments through multiple gateways while maintaining consistency, security, and reliability.

### Key Capabilities

- **Multi-Gateway Support**: Stripe, Square, Bank Transfer, Crypto, PayPal, PIX Brazil, Dots.dev, MoonPay, and Coinbase
- **Multi-Currency**: Support for USD, EUR, GBP, BRL, and more
- **Payment Methods**: Credit/debit cards, bank accounts, digital wallets, cryptocurrencies
- **Real-time Processing**: Instant payment processing with webhook notifications
- **Security**: PCI compliance, encrypted storage, secure API endpoints
- **Scalability**: Redis caching, background job processing with Hangfire

## Features

- ✅ Process deposits, withdrawals, transfers, and refunds
- ✅ Store and manage multiple payment methods per user
- ✅ Automatic gateway selection based on currency and amount
- ✅ Real-time webhook processing for payment status updates
- ✅ Comprehensive payment history and audit trails
- ✅ Retry logic for failed transactions
- ✅ Idempotency support to prevent duplicate charges
- ✅ Fee calculation and gateway cost tracking
- ✅ Background job processing for async operations
- ✅ Health monitoring and gateway statistics

## Architecture

### Technology Stack

- **Framework**: .NET 9, ASP.NET Core
- **Database**: PostgreSQL with Entity Framework Core
- **Caching**: Redis via StackExchange.Redis
- **Background Jobs**: Hangfire
- **Logging**: Serilog
- **Mapping**: Mapster
- **Authentication**: JWT Bearer tokens

### Service Structure

```
PaymentGatewayService/
├── Controllers/          # API endpoints
├── Data/                # Database context and entities
├── Models/              # DTOs and view models
├── Services/            # Business logic
│   ├── Interfaces/      # Service contracts
│   └── Integrations/    # Gateway-specific implementations
├── Utils/               # Utility classes
└── Migrations/          # Database migrations
```

## Supported Payment Gateways

### 1. Stripe (Global Coverage)
- **Type**: `PaymentGatewayType.Stripe`
- **Features**: Cards, bank debits, wallets, 135+ currencies
- **Best for**: International payments, SCA compliance
- **API Version**: 2023-10-16

### 2. Square (Fintech Optimized)
- **Type**: `PaymentGatewayType.Square`
- **Features**: Cards, digital wallets, in-person payments
- **Best for**: US/CA/UK/JP/AU markets, integrated POS
- **API Version**: 2023-10-18

### 3. Bank Transfer
- **Type**: `PaymentGatewayType.BankTransfer`
- **Features**: ACH, wire transfers, SEPA
- **Best for**: Large amounts, B2B payments
- **Processing**: 1-3 business days

### 4. Cryptocurrency
- **Type**: `PaymentGatewayType.CryptoWallet`
- **Features**: Bitcoin, Ethereum, stablecoins
- **Best for**: Cross-border, privacy-focused users
- **Confirmations**: Network-dependent

### 5. PayPal
- **Type**: `PaymentGatewayType.PayPal`
- **Features**: PayPal balance, linked cards/banks
- **Best for**: Consumer payments, buyer protection
- **Coverage**: 200+ markets worldwide

### 6. PIX Brazil
- **Type**: `PaymentGatewayType.PIXBrazil`
- **Features**: Instant transfers, QR codes, 24/7 availability
- **Best for**: Brazilian market, BRL transactions
- **Settlement**: Real-time (≤5 seconds)

### 7. Dots.dev (Global Payouts)
- **Type**: `PaymentGatewayType.DotsDev`
- **Features**: Payouts to 190+ countries, automatic payment method selection
- **Best for**: International payouts, multi-region disbursements
- **Coverage**: US (Venmo, PayPal, ACH), EU (SEPA), India (UPI), Philippines (GCash), and more
- **Compliance**: Built-in KYC/AML flows

### 8. MoonPay (Fiat-to-Crypto Gateway)
- **Type**: `PaymentGatewayType.MoonPay`
- **Features**: Buy/sell crypto, fiat on/off-ramps, widget integration
- **Best for**: Cryptocurrency purchases, fiat-to-crypto conversions
- **Supported Crypto**: BTC, ETH, USDT, USDC, SOL, ADA, MATIC
- **Supported Fiat**: USD, EUR, GBP, CAD, AUD
- **Compliance**: Built-in KYC/AML screening

### 9. Coinbase (Advanced Trade API)
- **Type**: `PaymentGatewayType.Coinbase`
- **Features**: Cryptocurrency trading, market/limit/stop orders, real-time market data
- **Best for**: Advanced crypto trading, institutional traders
- **Products**: BTC-USDC, ETH-USDC, SOL-USDC, MATIC-USDC, and more
- **API Features**: WebSocket feeds, portfolio management, order fills, historical data
- **Authentication**: ECDSA signature-based

## API Documentation

### Base URL
```
https://your-domain.com/api
```

### Authentication
All endpoints except webhooks require JWT authentication:
```
Authorization: Bearer {your-jwt-token}
```

### Controllers

#### 1. PaymentController (`/api/payment`)

Main payment processing endpoints:

```http
POST /api/payment
```
Process a new payment
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100.50,
  "currency": "USD",
  "type": "Deposit",
  "paymentMethodId": "pm_123456",
  "metadata": {
    "orderId": "12345",
    "description": "Premium subscription"
  }
}
```

```http
GET /api/payment/{id}
```
Get payment details

```http
GET /api/payment/user/{userId}
```
Get user's payment history

```http
POST /api/payment/{id}/refund
```
Create a refund for a payment
```json
{
  "amount": 50.25,
  "reason": "Customer request"
}
```

#### 2. PaymentMethodController (`/api/paymentmethod`)

Manage user payment methods:

```http
POST /api/paymentmethod
```
Add a new payment method
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "CreditCard",
  "paymentGatewayId": "gw_123456",
  "metadata": {
    "cardNumber": "4111111111111111",
    "expiryMonth": "12",
    "expiryYear": "2025",
    "cvv": "123"
  }
}
```

```http
GET /api/paymentmethod/user/{userId}
```
Get user's payment methods

```http
DELETE /api/paymentmethod/{id}
```
Delete a payment method

#### 3. PaymentGatewayController (`/api/paymentgateway`)

Gateway management and statistics:

```http
GET /api/paymentgateway
```
List all active gateways

```http
GET /api/paymentgateway/{id}/statistics
```
Get gateway performance statistics

```http
POST /api/paymentgateway/{id}/enable
```
Enable a gateway

```http
POST /api/paymentgateway/{id}/disable
```
Disable a gateway

#### 4. PIXController (`/api/pix`)

PIX Brazil specific operations:

```http
POST /api/pix/charge
```
Create PIX charge with QR code
```json
{
  "amount": 100.50,
  "payerName": "João Silva",
  "payerDocument": "12345678901",
  "payerEmail": "joao@example.com",
  "description": "Order #12345"
}
```

```http
POST /api/pix/payout
```
Create PIX payout
```json
{
  "amount": 500.00,
  "pixKey": "joao@example.com",
  "pixKeyType": "email",
  "recipientName": "João Silva",
  "recipientDocument": "12345678901"
}
```

```http
GET /api/pix/transaction/{id}
```
Get PIX transaction status

```http
POST /api/pix/validate-key
```
Validate a PIX key

#### 5. MoonPayController (`/api/moonpay`)

MoonPay fiat-to-crypto operations:

```http
POST /api/moonpay/buy
```
Create fiat-to-crypto transaction
```json
{
  "cryptoCurrency": "BTC",
  "amount": 100.00,
  "fiatCurrency": "USD",
  "walletAddress": "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa",
  "email": "user@example.com"
}
```

```http
POST /api/moonpay/sell
```
Create crypto-to-fiat transaction
```json
{
  "cryptoCurrency": "BTC",
  "cryptoAmount": 0.01,
  "fiatCurrency": "USD",
  "bankAccount": {
    "accountNumber": "12345678",
    "accountHolderName": "John Doe"
  }
}
```

```http
GET /api/moonpay/currencies
```
Get supported cryptocurrencies

```http
GET /api/moonpay/quote?cryptoCurrency=BTC&fiatCurrency=USD
```
Get current exchange rates

```http
POST /api/moonpay/widget
```
Create embedded widget session

#### 6. DotsDevController (`/api/dotsdev`)

Dots.dev global payout operations:

```http
POST /api/dotsdev/payout
```
Create international payout
```json
{
  "amount": 100.00,
  "currency": "USD",
  "recipientName": "John Doe",
  "recipientEmail": "john@example.com",
  "recipientCountry": "US",
  "paymentMethod": "venmo"
}
```

#### 7. CoinbaseController (`/api/coinbase`)

Coinbase Advanced Trade operations:

```http
POST /api/coinbase/orders
```
Create trading order
```json
{
  "productId": "BTC-USDC",
  "side": "buy",
  "orderType": "market",
  "quoteSize": 100.00
}
```

```http
GET /api/coinbase/accounts
```
Get account balances

```http
GET /api/coinbase/products/{productId}/ticker
```
Get current market ticker

```http
POST /api/coinbase/websocket/subscribe
```
Subscribe to real-time market data
```json
{
  "channels": ["ticker", "level2"],
  "productIds": ["BTC-USDC", "ETH-USDC"]
}
```

#### 8. WebhookController (`/api/webhook`)

Payment provider webhooks (no authentication required):

```http
POST /api/webhook/stripe
```
Stripe webhook endpoint

```http
POST /api/webhook/square
```
Square webhook endpoint

```http
POST /api/webhook/pix/{provider}
```
PIX webhook endpoint (provider: liquido, ebanx, cielo)

```http
POST /api/webhook/moonpay
```
MoonPay webhook endpoint

```http
POST /api/webhook/dotsdev
```
Dots.dev webhook endpoint

```http
POST /api/webhook/coinbase
```
Coinbase webhook endpoint

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=paymentgateway;Username=postgres;Password=yourpassword",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "your-secret-key-at-least-32-characters-long",
    "Issuer": "QuantumSkyLink",
    "Audience": "QuantumSkyLinkUsers"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "Square": {
    "Environment": "sandbox",
    "AccessToken": "EAAAE...",
    "ApplicationId": "sandbox-sq0idb-...",
    "LocationId": "L..."
  },
  "PIXBrazil": {
    "Provider": "Liquido",
    "ApiKey": "your-api-key",
    "AccessToken": "your-access-token",
    "BaseUrl": "https://api.liquido.com/v1/payments/",
    "WebhookSecret": "your-webhook-secret",
    "Environment": "sandbox",
    "DefaultExpirationSeconds": 86400,
    "MaxDescriptionLength": 43,
    "SupportedKeyTypes": ["email", "cpf", "phone", "random"]
  },
  "MoonPay": {
    "ApiKey": "your-api-key",
    "SecretKey": "your-secret-key",
    "BaseUrl": "https://api.moonpay.com/v3/",
    "WebhookSecret": "your-webhook-secret",
    "Environment": "sandbox",
    "SupportedCryptoCurrencies": ["BTC", "ETH", "USDT", "USDC"],
    "SupportedFiatCurrencies": ["USD", "EUR", "GBP"]
  },
  "DotsDev": {
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "BaseUrl": "https://api.dots.dev/v2/",
    "WebhookSecret": "your-webhook-secret",
    "Environment": "sandbox",
    "SupportedRegions": ["US", "EU", "IN", "PH", "GLOBAL"]
  },
  "Coinbase": {
    "ApiKey": "your-api-key",
    "PrivateKey": "your-private-key-pem-base64",
    "BaseUrl": "https://api.coinbase.com/api/v3/brokerage/",
    "WebSocketUrl": "wss://advanced-trade-ws.coinbase.com",
    "Environment": "sandbox"
  }
}
```

### Environment Variables

For production, use environment variables:

```bash
# Database
ConnectionStrings__DefaultConnection="Host=prod-db;Database=paymentgateway;..."
ConnectionStrings__Redis="prod-redis:6379"

# JWT
Jwt__Key="your-production-secret-key"

# Payment Gateways
Stripe__SecretKey="sk_live_..."
Square__AccessToken="EAAAE..."
PIXBrazil__ApiKey="prod-api-key"
MoonPay__ApiKey="prod-api-key"
MoonPay__SecretKey="prod-secret-key"
DotsDev__ApiKey="prod-api-key"
DotsDev__ApiSecret="prod-api-secret"
Coinbase__ApiKey="prod-api-key"
Coinbase__PrivateKey="prod-private-key-base64"
```

## Database Setup

### Prerequisites
- PostgreSQL 13+
- Redis 6+

### Initial Setup

1. Create database:
```sql
CREATE DATABASE paymentgateway;
```

2. Run migrations:
```bash
dotnet ef database update
```

### Entity Relationships

```
Payment
├── PaymentGateway (many-to-one)
├── PaymentMethod (many-to-one)
├── PaymentAttempts (one-to-many)
├── Refunds (one-to-many)
└── PaymentWebhooks (one-to-many)

PaymentMethod
├── PaymentGateway (many-to-one)
└── Payments (one-to-many)
```

## Authentication & Authorization

### JWT Configuration

The service uses JWT Bearer tokens for authentication. Tokens should include:

```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "User|Admin",
  "exp": 1234567890
}
```

### Authorization Policies

- **User**: Can manage their own payments and payment methods
- **Admin**: Full access to all operations

### Securing Webhooks

Webhooks are secured using:
- IP whitelisting
- Signature verification
- Timestamp validation
- Idempotency checks

## Integration Examples

### Processing a Payment

```csharp
// Client-side example
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);

var payment = new ProcessPaymentRequest
{
    UserId = userId,
    Amount = 100.50m,
    Currency = "USD",
    Type = PaymentType.Deposit,
    PaymentMethodId = methodId
};

var response = await client.PostAsJsonAsync(
    "https://api.example.com/api/payment", 
    payment);

var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();
```

### Creating a PIX Charge

```csharp
var pixCharge = new CreatePIXChargeRequest
{
    Amount = 100.50m,
    PayerName = "João Silva",
    PayerDocument = "12345678901",
    Description = "Order #12345"
};

var response = await client.PostAsJsonAsync(
    "https://api.example.com/api/pix/charge", 
    pixCharge);

var result = await response.Content.ReadFromJsonAsync<PIXChargeResponse>();
// result.QRCodeImage contains the QR code for payment
```

## Webhooks

### Webhook Flow

1. Provider sends POST request to webhook endpoint
2. Service validates signature and timestamp
3. Webhook stored in database
4. Payment status updated
5. Acknowledgment returned to provider

### Retry Logic

Failed webhooks are retried with exponential backoff:
- 1st retry: 1 minute
- 2nd retry: 5 minutes
- 3rd retry: 30 minutes

### Testing Webhooks

Use the test endpoint for development:
```http
POST /api/webhook/test
Authorization: Bearer {token}
```

## Testing

### Unit Tests

```bash
dotnet test PaymentGatewayService.Tests
```

### Integration Tests

Test with sandbox environments:

**Stripe Test Cards:**
- Success: 4242 4242 4242 4242
- Decline: 4000 0000 0000 0002
- 3D Secure: 4000 0000 0000 3220

**Square Sandbox:**
- Nonce: cnon:card-nonce-ok
- Error: cnon:card-nonce-declined

**PIX Test Data:**
- CPF: 11144477735
- CNPJ: 11222333000181
- Email: test@example.com
- Phone: +5511999999999

### Load Testing

Use k6 or similar tools:
```javascript
import http from 'k6/http';

export default function() {
    http.post('https://api.example.com/api/payment', 
        JSON.stringify({...}), 
        { headers: { 'Authorization': 'Bearer ...' }});
}
```

## Monitoring & Health Checks

### Health Check Endpoint

```http
GET /health
```

Returns:
```json
{
  "status": "Healthy",
  "results": {
    "npgsql": { "status": "Healthy" },
    "redis": { "status": "Healthy" }
  }
}
```

### Metrics

Monitor these key metrics:
- Payment success rate
- Average processing time
- Gateway availability
- Webhook processing lag
- Error rates by gateway

### Logging

Structured logging with Serilog:
```
2024-01-20 10:15:30 [INF] Processing payment for user 123 amount 100.50 USD
2024-01-20 10:15:31 [INF] Payment completed. PaymentId: 456, Gateway: Stripe
```

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PaymentGatewayService.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentGatewayService.dll"]
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: payment-gateway-service
spec:
  replicas: 3
  selector:
    matchLabels:
      app: payment-gateway-service
  template:
    metadata:
      labels:
        app: payment-gateway-service
    spec:
      containers:
      - name: payment-gateway-service
        image: your-registry/payment-gateway-service:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: payment-db-secret
              key: connection-string
```

### Production Checklist

- [ ] SSL/TLS certificates configured
- [ ] Database connection pooling optimized
- [ ] Redis cluster configured
- [ ] Secrets stored in secure vault
- [ ] Rate limiting enabled
- [ ] CORS policies configured
- [ ] Monitoring and alerting set up
- [ ] Backup and disaster recovery plan
- [ ] PCI compliance verified
- [ ] Load balancing configured

## Support

For issues, questions, or contributions:
- GitHub: [QuantumSkyLink_v2](https://github.com/your-org/QuantumSkyLink_v2)
- Documentation: [Wiki](https://github.com/your-org/QuantumSkyLink_v2/wiki)
- Email: support@quantumskylink.com

## License

Copyright © 2024 QuantumSkyLink. All rights reserved.