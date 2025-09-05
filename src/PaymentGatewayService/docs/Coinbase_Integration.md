# Coinbase Advanced Trade API Integration

This document describes the Coinbase Advanced Trade API integration in the PaymentGatewayService.

## Overview

The Coinbase integration provides cryptocurrency trading capabilities through the Advanced Trade API, enabling:
- Market, limit, and stop-limit orders
- Real-time WebSocket market data
- Portfolio management
- Order tracking and fills
- Historical market data

## Features

### Trading Operations
- **Create Orders**: Market, limit, and stop-limit orders
- **Cancel Orders**: Cancel pending orders
- **Order Status**: Track order execution status
- **Order History**: List historical orders with filtering

### Account Management
- **Account Balances**: View cryptocurrency and fiat balances
- **Portfolio Management**: Create and manage isolated portfolios
- **Fee Information**: Get current fee tier and rates

### Market Data
- **Product Information**: List available trading pairs
- **Ticker Data**: Current bid/ask prices
- **Order Book**: Level 1 and Level 2 order book data
- **Historical Candles**: OHLC data for various time granularities
- **WebSocket Feeds**: Real-time market updates

## Configuration

Add the following configuration to `appsettings.json`:

```json
{
  "Coinbase": {
    "ApiKey": "your-api-key",
    "PrivateKey": "your-private-key-pem-base64",
    "BaseUrl": "https://api.coinbase.com/api/v3/brokerage/",
    "WebSocketUrl": "wss://advanced-trade-ws.coinbase.com",
    "Environment": "sandbox",
    "SupportedProducts": ["BTC-USDC", "ETH-USDC", "SOL-USDC", "MATIC-USDC"],
    "OrderTypes": ["market", "limit", "stop_limit"],
    "TimeInForce": ["gtc", "gtd", "ioc", "fok"],
    "WebhookEndpoint": "/api/webhook/coinbase",
    "WebSocketChannels": ["level2", "ticker", "ticker_batch", "status", "market_trades", "full"],
    "Limits": {
      "MinOrderSize": 0.001,
      "MaxOrderSize": 10000,
      "RateLimitPerSecond": 10
    }
  }
}
```

### Configuration Parameters

- **ApiKey**: Your Coinbase API key
- **PrivateKey**: Your ECDSA private key in PEM format (base64 encoded)
- **BaseUrl**: Coinbase API base URL
- **WebSocketUrl**: WebSocket endpoint for real-time data
- **Environment**: `sandbox` or `production`
- **SupportedProducts**: List of trading pairs to support
- **OrderTypes**: Supported order types
- **TimeInForce**: Supported time-in-force options
- **WebhookEndpoint**: Endpoint for receiving webhooks
- **WebSocketChannels**: Available WebSocket channels
- **Limits**: Trading limits and rate limits

## API Endpoints

### Trading Endpoints

#### Create Order
```http
POST /api/coinbase/orders
Authorization: Bearer {token}

{
  "productId": "BTC-USDC",
  "side": "buy",
  "orderType": "market",
  "quoteSize": 100.00
}
```

#### Cancel Order
```http
DELETE /api/coinbase/orders/{orderId}
Authorization: Bearer {token}
```

#### Get Order
```http
GET /api/coinbase/orders/{orderId}
Authorization: Bearer {token}
```

#### List Orders
```http
GET /api/coinbase/orders?productId=BTC-USDC&status=filled&limit=100
Authorization: Bearer {token}
```

### Account Endpoints

#### Get Accounts
```http
GET /api/coinbase/accounts
Authorization: Bearer {token}
```

#### Create Portfolio
```http
POST /api/coinbase/portfolios
Authorization: Bearer {token}

{
  "name": "Trading Portfolio"
}
```

### Market Data Endpoints

#### Get Products
```http
GET /api/coinbase/products
```

#### Get Ticker
```http
GET /api/coinbase/products/{productId}/ticker
```

#### Get Order Book
```http
GET /api/coinbase/products/{productId}/book?level=2
```

#### Get Historical Candles
```http
GET /api/coinbase/products/{productId}/candles?granularity=3600&start=2024-01-01&end=2024-01-02
```

### WebSocket Endpoints

#### Subscribe to WebSocket
```http
POST /api/coinbase/websocket/subscribe
Authorization: Bearer {token}

{
  "channels": ["ticker", "level2"],
  "productIds": ["BTC-USDC", "ETH-USDC"]
}
```

#### Unsubscribe from WebSocket
```http
DELETE /api/coinbase/websocket/subscribe/{subscriptionId}
Authorization: Bearer {token}
```

## Authentication

The Coinbase integration uses ECDSA signatures for API authentication:

1. **API Key**: Identifies your application
2. **Private Key**: Used to sign requests
3. **Timestamp**: Current Unix timestamp
4. **Signature**: ECDSA signature of `timestamp + method + path + body`

For WebSocket connections, a JWT token is generated using the ECDSA private key.

## Order Types

### Market Orders
Execute immediately at the best available price:
```json
{
  "orderType": "market",
  "side": "buy",
  "quoteSize": 100.00  // For buy orders in quote currency
}
```

### Limit Orders
Execute at a specific price or better:
```json
{
  "orderType": "limit",
  "side": "sell",
  "baseSize": 0.01,
  "limitPrice": 50000.00,
  "postOnly": true
}
```

### Stop-Limit Orders
Trigger a limit order when stop price is reached:
```json
{
  "orderType": "stop_limit",
  "side": "sell",
  "baseSize": 0.01,
  "limitPrice": 49000.00,
  "stopPrice": 49500.00,
  "stopDirection": "stop_direction_stop_down"
}
```

## Webhooks

Coinbase sends webhooks for order events:

### Webhook Events
- `orders.filled`: Order completely filled
- `orders.cancelled`: Order cancelled
- `orders.updated`: Order status updated
- `orders.failed`: Order failed

### Webhook Payload Example
```json
{
  "id": "webhook-id",
  "resource": "event",
  "event_type": "orders.filled",
  "data": {
    "order_id": "order-123",
    "client_order_id": "payment-456",
    "product_id": "BTC-USDC",
    "side": "buy",
    "status": "filled",
    "filled_size": "0.01",
    "average_filled_price": "50000.00",
    "total_fees": "2.50"
  },
  "created_at": "2024-01-01T12:00:00Z"
}
```

## WebSocket Channels

### Ticker Channel
Real-time price updates:
```json
{
  "channel": "ticker",
  "events": [{
    "type": "update",
    "products": [{
      "product_id": "BTC-USDC",
      "price": "50000.00",
      "volume_24h": "1000.00"
    }]
  }]
}
```

### Level2 Channel
Order book updates:
```json
{
  "channel": "level2",
  "events": [{
    "type": "update",
    "product_id": "BTC-USDC",
    "updates": [{
      "side": "bid",
      "event_time": "2024-01-01T12:00:00Z",
      "price_level": "49999.00",
      "new_quantity": "0.5"
    }]
  }]
}
```

## Error Handling

Common error responses:

### Invalid Signature
```json
{
  "error": "AUTHENTICATION_ERROR",
  "message": "Invalid API key or signature"
}
```

### Insufficient Funds
```json
{
  "error": "INSUFFICIENT_FUNDS",
  "message": "Insufficient balance for order"
}
```

### Rate Limit Exceeded
```json
{
  "error": "RATE_LIMIT_EXCEEDED",
  "message": "Too many requests"
}
```

## Integration with Payment Gateway

The Coinbase integration maps to the payment gateway system:

1. **Buy Orders** → `PaymentType.Deposit`
2. **Sell Orders** → `PaymentType.Withdrawal`
3. **Order Status** → `PaymentStatus` mapping:
   - `filled` → `Completed`
   - `pending` → `Pending`
   - `cancelled` → `Cancelled`
   - `failed` → `Failed`

## Security Considerations

1. **Private Key Storage**: Store ECDSA private keys securely
2. **API Key Rotation**: Regularly rotate API keys
3. **IP Whitelisting**: Use Coinbase IP whitelist feature
4. **Webhook Verification**: Verify webhook signatures
5. **Rate Limiting**: Implement client-side rate limiting

## Testing

Use the Coinbase Sandbox environment for testing:

1. Create sandbox API credentials
2. Set `Environment` to `sandbox` in configuration
3. Use sandbox base URLs:
   - API: `https://api-sandbox.coinbase.com/api/v3/brokerage/`
   - WebSocket: `wss://ws-sandbox.exchange.coinbase.com`

## Monitoring

Monitor the following metrics:

1. **Order Success Rate**: Track successful vs failed orders
2. **API Response Times**: Monitor latency
3. **WebSocket Connection Health**: Track disconnections
4. **Rate Limit Usage**: Monitor API rate limit consumption
5. **Balance Changes**: Track account balance updates

## Support

For issues with the Coinbase integration:

1. Check the [Coinbase API documentation](https://docs.cloud.coinbase.com/advanced-trade-api/docs)
2. Review error logs in the application
3. Verify API credentials and permissions
4. Check Coinbase system status