# PIX Brazil Integration for PaymentGatewayService

## Overview
This document describes the PIX Brazil instant payment system integration in the PaymentGatewayService.

## Supported Payment Gateways

The PaymentGatewayService now supports the following payment methods:
1. **Stripe** - Global payment processing
2. **Square** - Fintech-optimized payment processing  
3. **BankTransfer** - ACH and Wire transfers
4. **CryptoWallet** - Cryptocurrency payments
5. **ApplePay** - Apple Pay integration
6. **GooglePay** - Google Pay integration
7. **PIXBrazil** - Brazilian instant payment system (NEW)

## PIX-Specific API Endpoints

### PIXController (`/api/pix`)
PIX-specific operations for Brazilian instant payments:

- `POST /api/pix/charge` - Create PIX charge with QR code
- `POST /api/pix/payout` - Create PIX payout
- `GET /api/pix/transaction/{id}` - Get PIX transaction status
- `POST /api/pix/qrcode/static` - Generate static QR code
- `POST /api/pix/validate-key` - Validate PIX key

### WebhookController (`/api/webhook`)
Handles payment provider callbacks (anonymous access):

- `POST /api/webhook/pix/{provider}` - PIX webhook endpoint
- `POST /api/webhook/stripe` - Stripe webhook endpoint
- `POST /api/webhook/square` - Square webhook endpoint

## PIX Integration Features

### 1. PIX Charges (Money In)
- Dynamic QR code generation
- Configurable expiration (5 minutes to 7 days)
- CPF/CNPJ validation for payers
- Real-time payment notifications via webhooks

### 2. PIX Payouts (Money Out)
- Support for all PIX key types:
  - CPF/CNPJ documents
  - Email addresses
  - Phone numbers
  - Random keys (UUID)
- Idempotency support to prevent duplicate payouts
- End-to-end transaction tracking

### 3. QR Code Support
- Dynamic QR codes with specific amounts
- Static QR codes for recurring payments
- Base64 encoded images for easy display
- Copy/paste PIX string format

### 4. Security Features
- CPF/CNPJ document validation
- PIX key validation before transactions
- Webhook signature verification
- Transaction amount limits
- Secure credential management

## Configuration

Add the following to your `appsettings.json`:

```json
{
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
  }
}
```

## Using PIX with the Standard Payment API

The standard `/api/payment` endpoint automatically supports PIX when:
1. Currency is BRL
2. A PIX gateway is configured and active
3. Payment metadata includes PIX-specific fields

Example payment request with PIX metadata:
```json
{
  "userId": "guid",
  "amount": 100.50,
  "currency": "BRL",
  "type": "Deposit",
  "metadata": {
    "payerName": "João Silva",
    "payerDocument": "12345678901",
    "payerEmail": "joao@example.com"
  }
}
```

The response will include PIX data when applicable:
```json
{
  "id": "payment-guid",
  "status": "Pending",
  "pixData": {
    "qrCodeString": "00020126580014BR.GOV.BCB.PIX...",
    "qrCodeImage": "data:image/png;base64,..."
  }
}
```

## Webhook Integration

PIX providers send webhooks to notify about payment status changes:

1. Configure webhook URL in provider dashboard:
   - `https://your-domain.com/api/webhook/pix/liquido`

2. Webhook events handled:
   - `payment.completed` - Payment successful
   - `payment.failed` - Payment failed
   - `payment.cancelled` - Payment cancelled
   - `payout.completed` - Payout successful
   - `payout.failed` - Payout failed

## Testing

Use the sandbox environment for testing:
- Test CPF: 11144477735
- Test CNPJ: 11222333000181
- Test phone: +5511999999999
- Test email: test@example.com

## Error Handling

Common error scenarios:
- Invalid CPF/CNPJ format
- Unsupported PIX key type
- Expired QR codes
- Insufficient balance for payouts
- Network timeouts

## Monitoring

Monitor PIX transactions through:
- Payment status in `/api/payment/{id}`
- Transaction details in `/api/pix/transaction/{id}`
- Webhook status in `/api/webhook/status/{paymentId}`
- Gateway health checks in `/api/paymentgateway/{id}/statistics`