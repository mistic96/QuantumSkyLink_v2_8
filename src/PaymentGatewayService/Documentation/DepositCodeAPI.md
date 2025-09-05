# Deposit Code Validation System API Documentation

## Overview

The QuantumSkyLink v2 Deposit Code Validation System is a comprehensive security and compliance framework for managing deposit codes throughout their lifecycle. This system provides robust validation, duplicate detection, compliance review workflows, and administrative management capabilities.

### Key Features
- **Secure Code Generation**: Cryptographically secure 8-character alphanumeric codes
- **Real-time Validation**: Instant validation with comprehensive security checks
- **Duplicate Detection**: Automatic detection and management of duplicate codes
- **Compliance Review**: Admin workflow for reviewing flagged deposits
- **Audit Trail**: Complete activity tracking for compliance and security
- **Multi-service Integration**: Seamless integration with QuantumLedger.Hub and FeeService
- **Automated Rejection Handling**: Smart rejection processing with fee calculation and return-to-sender

### System Architecture
- **PaymentGatewayService**: Core validation and processing engine
- **AdminAPIGateway**: Administrative interface for compliance teams
- **QuantumLedger.Hub**: Distributed ledger integration (optional)
- **FeeService**: Fee calculation service integration
- **RefundService**: Automated return-to-sender processing

---

## PaymentGatewayService Endpoints

### 1. Core Validation Endpoints

#### POST /api/payments/process
**Description**: Process a payment with deposit code validation

**Request Headers**:
```http
Content-Type: application/json
Authorization: Bearer {token}
```

**Request Body**:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "amount": 1500.00,
  "currency": "USD",
  "type": "Deposit",
  "depositCode": "ABC12345",
  "description": "Customer deposit via bank transfer",
  "paymentMethodId": "987fcdeb-51a2-43d7-b678-123456789abc",
  "preferredGatewayId": "456e7890-a12b-34c5-d678-901234567def",
  "clientIpAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
  "metadata": {
    "wallet_address": "0x742d35Cc6634C0532925a3b8D8Ec90532",
    "transaction_hash": "0xabc123def456789...",
    "source": "web_portal"
  }
}
```

**Response (Success - 200)**:
```json
{
  "success": true,
  "paymentId": "789e0123-f45a-67b8-c901-234567890abc",
  "status": "Pending",
  "amount": 1500.00,
  "currency": "USD",
  "feeAmount": 45.30,
  "netAmount": 1454.70,
  "depositCodeUsed": "ABC12345",
  "createdAt": "2024-08-04T10:30:00Z",
  "expiresAt": "2024-08-05T10:30:00Z"
}
```

**Response (Validation Error - 400)**:
```json
{
  "success": false,
  "error": "Deposit code validation failed",
  "code": "INVALID_DEPOSIT_CODE",
  "message": "Deposit code has expired",
  "details": {
    "depositCode": "ABC12345",
    "expiredAt": "2024-08-03T10:30:00Z"
  }
}
```

**Response (Deposit Rejected - 422)**:
```json
{
  "success": false,
  "paymentId": "789e0123-f45a-67b8-c901-234567890abc",
  "status": "Failed",
  "rejectionReason": "Deposit code contains invalid characters. Only alphanumeric characters are allowed",
  "originalAmount": 1500.00,
  "rejectionFees": 75.00,
  "netReturnAmount": 1425.00,
  "feeBreakdown": "Wire Fees: $25.00, Internal Fees: $50.00",
  "refundTransactionId": "REF-789e0123-1691140200",
  "processedAt": "2024-08-04T10:30:00Z"
}
```

#### POST /api/deposit-codes/generate
**Description**: Generate a new deposit code

**Request Body**:
```json
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "amount": 1000.00,
  "currency": "USD",
  "expiryHours": 24
}
```

**Response (Success - 200)**:
```json
{
  "success": true,
  "depositCode": "QK7N8M9P",
  "expiresAt": "2024-08-05T10:30:00Z",
  "amount": 1000.00,
  "currency": "USD",
  "createdAt": "2024-08-04T10:30:00Z"
}
```

#### POST /api/deposit-codes/validate
**Description**: Validate a deposit code without processing payment

**Request Body**:
```json
{
  "depositCode": "QK7N8M9P",
  "amount": 1000.00,
  "currency": "USD",
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (Valid - 200)**:
```json
{
  "valid": true,
  "depositCode": "QK7N8M9P",
  "status": "Active",
  "amount": 1000.00,
  "currency": "USD",
  "expiresAt": "2024-08-05T10:30:00Z",
  "authorizedUserId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (Invalid - 400)**:
```json
{
  "valid": false,
  "error": "Deposit code has expired",
  "depositCode": "QK7N8M9P",
  "status": "Expired",
  "expiredAt": "2024-08-03T10:30:00Z"
}
```

---

## AdminAPIGateway Endpoints

### 2. Review Management Endpoints

#### GET /api/admin/deposit-codes/pending-review
**Description**: Get all deposit codes pending admin review

**Query Parameters**:
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page (max 100)
- `sortBy` (string, default: "createdAt"): Sort field (createdAt, amount, code)
- `sortDescending` (bool, default: true): Sort direction
- `filterReason` (string, optional): Filter by review reason

**Request Headers**:
```http
Authorization: Bearer {admin_token}
```

**Response (Success - 200)**:
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "code": "ABC12345",
      "status": "UnderReview",
      "reviewReason": "Duplicate code detected",
      "amount": 1500.00,
      "currency": "USD",
      "userId": "987fcdeb-51a2-43d7-b678-123456789abc",
      "userEmail": "user@example.com",
      "createdAt": "2024-08-04T10:30:00Z",
      "heldSince": "2024-08-04T11:00:00Z",
      "duplicateCount": 3
    }
  ],
  "totalCount": 45,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

#### GET /api/admin/deposit-codes/review/{depositCodeId}
**Description**: Get detailed information about a specific deposit code under review

**Path Parameters**:
- `depositCodeId` (guid): The deposit code ID to review

**Response (Success - 200)**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "code": "ABC12345",
  "status": "UnderReview",
  "reviewReason": "Duplicate code detected",
  "amount": 1500.00,
  "currency": "USD",
  "userId": "987fcdeb-51a2-43d7-b678-123456789abc",
  "user": {
    "id": "987fcdeb-51a2-43d7-b678-123456789abc",
    "email": "user@example.com",
    "fullName": "John Doe",
    "kycStatus": "Verified",
    "registeredAt": "2024-01-15T08:30:00Z"
  },
  "associatedPayment": {
    "id": "456e7890-a12b-34c5-d678-901234567def",
    "amount": 1500.00,
    "currency": "USD",
    "status": "Held",
    "paymentMethod": "BankTransfer",
    "createdAt": "2024-08-04T10:30:00Z"
  },
  "duplicates": [
    {
      "id": "789e0123-f45a-67b8-c901-234567890abc",
      "code": "abc12345",
      "userId": "111e2222-a33b-44c5-d666-777888999aaa",
      "userEmail": "another@example.com",
      "createdAt": "2024-08-03T15:20:00Z",
      "status": "Active"
    }
  ],
  "metadata": {
    "reviewReason": "Duplicate code detected",
    "flaggedAt": "2024-08-04T11:00:00Z",
    "duplicatePattern": "case_insensitive",
    "riskScore": 75
  },
  "createdAt": "2024-08-04T10:30:00Z",
  "heldSince": "2024-08-04T11:00:00Z"
}
```

#### POST /api/admin/deposit-codes/review/{depositCodeId}/approve
**Description**: Approve a deposit code that was held for review

**Path Parameters**:
- `depositCodeId` (guid): The deposit code ID to approve

**Request Body**:
```json
{
  "approvalReason": "Manual review completed - legitimate transaction",
  "approvalNotes": "User provided additional documentation confirming transaction validity",
  "processDuplicatesAsWell": true
}
```

**Response (Success - 200)**:
```json
{
  "success": true,
  "depositCodeId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Active",
  "paymentProcessed": true,
  "duplicatesProcessed": 2
}
```

#### POST /api/admin/deposit-codes/review/{depositCodeId}/reject
**Description**: Reject a deposit code and initiate refund process

**Path Parameters**:
- `depositCodeId` (guid): The deposit code ID to reject

**Request Body**:
```json
{
  "rejectionReason": "Fraudulent transaction detected",
  "rejectionNotes": "Multiple red flags: suspicious timing, duplicate codes, inconsistent user data",
  "initiateRefund": true,
  "customerCommunication": "Your deposit has been rejected due to security concerns. A refund will be processed within 5-7 business days."
}
```

**Response (Success - 200)**:
```json
{
  "success": true,
  "depositCodeId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Rejected",
  "refundInitiated": true,
  "refundAmount": 1425.00,
  "refundTransactionId": "REF-123e4567-1691140200"
}
```

### 3. Bulk Operations

#### POST /api/admin/deposit-codes/bulk-action
**Description**: Perform bulk actions on multiple deposit codes

**Request Body**:
```json
{
  "depositCodeIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "987fcdeb-51a2-43d7-b678-123456789abc",
    "456e7890-a12b-34c5-d678-901234567def"
  ],
  "action": "Approve",
  "reason": "Batch approval after manual review",
  "notes": "All codes verified through enhanced due diligence process"
}
```

**Response (Success - 200)**:
```json
{
  "totalCount": 3,
  "successCount": 2,
  "failedCount": 1,
  "results": [
    {
      "depositCodeId": "123e4567-e89b-12d3-a456-426614174000",
      "success": true
    },
    {
      "depositCodeId": "987fcdeb-51a2-43d7-b678-123456789abc",
      "success": true
    },
    {
      "depositCodeId": "456e7890-a12b-34c5-d678-901234567def",
      "success": false,
      "errorMessage": "Deposit code already processed"
    }
  ]
}
```

### 4. Analytics and Reporting

#### GET /api/admin/deposit-codes/duplicate-statistics
**Description**: Get deposit code duplicate patterns and statistics

**Query Parameters**:
- `startDate` (datetime, optional): Start date for analysis (default: 30 days ago)
- `endDate` (datetime, optional): End date for analysis (default: now)

**Response (Success - 200)**:
```json
{
  "startDate": "2024-07-05T00:00:00Z",
  "endDate": "2024-08-04T23:59:59Z",
  "totalDuplicates": 127,
  "uniqueDuplicateSets": 23,
  "resolvedDuplicates": 89,
  "pendingDuplicates": 38,
  "topPatterns": [
    {
      "pattern": "ABC12345",
      "count": 8,
      "examples": ["ABC12345", "abc12345", "Abc12345"]
    },
    {
      "pattern": "TEST1234",
      "count": 6,
      "examples": ["TEST1234", "test1234", "Test1234"]
    }
  ],
  "duplicatesByStatus": {
    "Active": 15,
    "UnderReview": 38,
    "Used": 52,
    "Rejected": 22
  }
}
```

#### POST /api/admin/deposit-codes/search
**Description**: Search deposit codes with advanced filters

**Request Body**:
```json
{
  "code": "ABC",
  "statuses": ["Active", "UnderReview"],
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "startDate": "2024-08-01T00:00:00Z",
  "endDate": "2024-08-04T23:59:59Z",
  "minAmount": 100.00,
  "maxAmount": 5000.00,
  "currency": "USD",
  "hasDuplicates": true,
  "page": 1,
  "pageSize": 50,
  "sortBy": "createdAt",
  "sortDescending": true
}
```

**Response (Success - 200)**:
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "code": "ABC12345",
      "status": "UnderReview",
      "amount": 1500.00,
      "currency": "USD",
      "userId": "987fcdeb-51a2-43d7-b678-123456789abc",
      "userEmail": "user@example.com",
      "createdAt": "2024-08-04T10:30:00Z",
      "usedAt": null,
      "hasDuplicates": true
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 50
}
```

### 5. Audit and Compliance

#### GET /api/admin/deposit-codes/{depositCodeId}/audit-trail
**Description**: Get audit trail for a specific deposit code

**Path Parameters**:
- `depositCodeId` (guid): The deposit code ID

**Response (Success - 200)**:
```json
[
  {
    "id": "audit-001",
    "timestamp": "2024-08-04T10:30:00Z",
    "action": "Created",
    "performedBy": "123e4567-e89b-12d3-a456-426614174000",
    "details": "Deposit code ABC12345 created"
  },
  {
    "id": "audit-002",
    "timestamp": "2024-08-04T11:00:00Z",
    "action": "Flagged",
    "performedBy": "System",
    "details": "Duplicate detection triggered review",
    "changes": {
      "status": {
        "from": "Active",
        "to": "UnderReview"
      }
    }
  },
  {
    "id": "audit-003",
    "timestamp": "2024-08-04T14:30:00Z",
    "action": "Approved",
    "performedBy": "admin@quantumskylink.com",
    "details": "Manual review completed - legitimate transaction"
  }
]
```

#### POST /api/admin/deposit-codes/export
**Description**: Export deposit code data for reporting

**Request Body**:
```json
{
  "searchCriteria": {
    "startDate": "2024-08-01T00:00:00Z",
    "endDate": "2024-08-04T23:59:59Z",
    "statuses": ["Active", "Used", "Rejected"]
  },
  "format": "CSV",
  "includeFields": [
    "Id",
    "Code", 
    "Status",
    "Amount",
    "Currency",
    "UserId",
    "CreatedAt",
    "UsedAt"
  ]
}
```

**Response (Success - 200)**:
```http
Content-Type: text/csv
Content-Disposition: attachment; filename="deposit_codes_export_20240804_143000.csv"

Id,Code,Status,Amount,Currency,UserId,CreatedAt,UsedAt
123e4567-e89b-12d3-a456-426614174000,ABC12345,Used,1500.00,USD,987fcdeb-51a2-43d7-b678-123456789abc,2024-08-04T10:30:00Z,2024-08-04T12:15:00Z
```

---

## Error Codes and Responses

### Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `INVALID_DEPOSIT_CODE` | 400 | Deposit code format is invalid or not found |
| `EXPIRED_DEPOSIT_CODE` | 400 | Deposit code has expired |
| `INACTIVE_DEPOSIT_CODE` | 400 | Deposit code is not active (used, rejected, etc.) |
| `UNAUTHORIZED_CODE_USAGE` | 403 | User not authorized to use this deposit code |
| `AMOUNT_MISMATCH` | 400 | Payment amount doesn't match deposit code amount |
| `CURRENCY_MISMATCH` | 400 | Payment currency doesn't match deposit code currency |
| `DUPLICATE_CODE_DETECTED` | 422 | Deposit code flagged for duplicate review |
| `PAYMENT_LIMITS_EXCEEDED` | 400 | User payment limits exceeded |
| `SUSPICIOUS_ACTIVITY` | 422 | Transaction flagged for suspicious activity |
| `GATEWAY_UNAVAILABLE` | 503 | Payment gateway temporarily unavailable |
| `VALIDATION_FAILED` | 400 | General validation failure |
| `INSUFFICIENT_PERMISSIONS` | 403 | User lacks required permissions |
| `RESOURCE_NOT_FOUND` | 404 | Requested resource not found |
| `RATE_LIMIT_EXCEEDED` | 429 | API rate limit exceeded |

### Error Response Format

```json
{
  "success": false,
  "error": "Human-readable error message",
  "code": "ERROR_CODE",
  "message": "Detailed technical message",
  "details": {
    "field": "Additional context",
    "timestamp": "2024-08-04T10:30:00Z",
    "correlationId": "corr-123-456-789"
  },
  "supportReference": "SUP-2024080410300001"
}
```

---

## Integration Examples

### 1. Web Application Integration

#### JavaScript/TypeScript Example
```typescript
interface DepositCodeValidation {
  depositCode: string;
  amount: number;
  currency: string;
  userId?: string;
}

class DepositCodeService {
  private baseUrl = 'https://api.quantumskylink.com';
  private apiKey: string;

  constructor(apiKey: string) {
    this.apiKey = apiKey;
  }

  async validateDepositCode(validation: DepositCodeValidation): Promise<boolean> {
    try {
      const response = await fetch(`${this.baseUrl}/api/deposit-codes/validate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.apiKey}`
        },
        body: JSON.stringify(validation)
      });

      const result = await response.json();
      
      if (response.ok) {
        return result.valid;
      } else {
        console.error('Validation failed:', result.error);
        return false;
      }
    } catch (error) {
      console.error('Network error:', error);
      return false;
    }
  }

  async processPayment(paymentRequest: PaymentRequest): Promise<PaymentResult> {
    try {
      const response = await fetch(`${this.baseUrl}/api/payments/process`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.apiKey}`
        },
        body: JSON.stringify(paymentRequest)
      });

      const result = await response.json();
      
      if (response.ok) {
        return { success: true, payment: result };
      } else {
        return { success: false, error: result.error };
      }
    } catch (error) {
      return { success: false, error: 'Network error occurred' };
    }
  }
}

// Usage example
const depositService = new DepositCodeService('your-api-key');

// Validate before processing
const isValid = await depositService.validateDepositCode({
  depositCode: 'ABC12345',
  amount: 1500.00,
  currency: 'USD',
  userId: 'user-123'
});

if (isValid) {
  // Process the payment
  const result = await depositService.processPayment({
    userId: 'user-123',
    amount: 1500.00,
    currency: 'USD',
    type: 'Deposit',
    depositCode: 'ABC12345'
  });
  
  if (result.success) {
    console.log('Payment processed:', result.payment.paymentId);
  } else {
    console.error('Payment failed:', result.error);
  }
}
```

### 2. Mobile Application Integration

#### Swift (iOS) Example
```swift
import Foundation

struct DepositCodeValidation: Codable {
    let depositCode: String
    let amount: Double
    let currency: String
    let userId: String?
}

struct ValidationResponse: Codable {
    let valid: Bool
    let error: String?
    let depositCode: String
    let status: String
}

class DepositCodeService {
    private let baseURL = "https://api.quantumskylink.com"
    private let apiKey: String
    
    init(apiKey: String) {
        self.apiKey = apiKey
    }
    
    func validateDepositCode(_ validation: DepositCodeValidation) async throws -> Bool {
        guard let url = URL(string: "\(baseURL)/api/deposit-codes/validate") else {
            throw URLError(.badURL)
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue("Bearer \(apiKey)", forHTTPHeaderField: "Authorization")
        
        let jsonData = try JSONEncoder().encode(validation)
        request.httpBody = jsonData
        
        let (data, response) = try await URLSession.shared.data(for: request)
        
        guard let httpResponse = response as? HTTPURLResponse else {
            throw URLError(.badServerResponse)
        }
        
        let validationResponse = try JSONDecoder().decode(ValidationResponse.self, from: data)
        
        if httpResponse.statusCode == 200 {
            return validationResponse.valid
        } else {
            print("Validation failed: \(validationResponse.error ?? "Unknown error")")
            return false
        }
    }
}

// Usage example
let service = DepositCodeService(apiKey: "your-api-key")

Task {
    do {
        let validation = DepositCodeValidation(
            depositCode: "ABC12345",
            amount: 1500.00,
            currency: "USD",
            userId: "user-123"
        )
        
        let isValid = try await service.validateDepositCode(validation)
        
        if isValid {
            print("Deposit code is valid")
            // Proceed with payment processing
        } else {
            print("Deposit code is invalid")
        }
    } catch {
        print("Error validating deposit code: \(error)")
    }
}
```

### 3. Backend Service Integration

#### C# .NET Example
```csharp
using System.Text.Json;
using System.Text;

public class DepositCodeService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public DepositCodeService(HttpClient httpClient, string baseUrl, string apiKey)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _apiKey = apiKey;
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<bool> ValidateDepositCodeAsync(string depositCode, decimal amount, string currency, Guid? userId = null)
    {
        try
        {
            var validation = new
            {
                depositCode,
                amount,
                currency,
                userId
            };

            var json = JsonSerializer.Serialize(validation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/deposit-codes/validate", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ValidationResponse>(responseContent);
                return result?.Valid ?? false;
            }
            else
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                throw new InvalidOperationException($"Validation failed: {error?.Error}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error: {ex.Message}", ex);
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/api/payments/process", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<PaymentResult>(responseContent) 
                    ?? throw new InvalidOperationException("Invalid response format");
            }
            else
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                return new PaymentResult 
                { 
                    Success = false, 
                    Error = error?.Error ?? "Unknown error" 
                };
            }
        }
        catch (HttpRequestException ex)
        {
            return new PaymentResult 
            { 
                Success = false, 
                Error = $"Network error: {ex.Message}" 
            };
        }
    }
}

// Usage example
public class PaymentController : ControllerBase
{
    private readonly DepositCodeService _depositCodeService;

    public PaymentController(DepositCodeService depositCodeService)
    {
        _depositCodeService = depositCodeService;
    }

    [HttpPost("process-deposit")]
    public async Task<IActionResult> ProcessDeposit([FromBody] DepositRequest request)
    {
        try
        {
            // Validate deposit code first
            var isValid = await _depositCodeService.ValidateDepositCodeAsync(
                request.DepositCode, 
                request.Amount, 
                request.Currency, 
                request.UserId);

            if (!isValid)
            {
                return BadRequest("Invalid deposit code");
            }

            // Process payment
            var paymentRequest = new ProcessPaymentRequest
            {
                UserId = request.UserId,
                Amount = request.Amount,
                Currency = request.Currency,
                Type = PaymentType.Deposit,
                DepositCode = request.DepositCode,
                Description = request.Description
            };

            var result = await _depositCodeService.ProcessPaymentAsync(paymentRequest);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Error);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal error: {ex.Message}");
        }
    }
}
```

---

## Security Considerations

### 1. Authentication and Authorization
- **API Keys**: All requests require valid API keys with appropriate scopes
- **JWT Tokens**: Admin endpoints require JWT tokens with admin privileges
- **Rate Limiting**: API calls are rate-limited to prevent abuse
- **IP Allowlisting**: Production environments support IP allowlisting

### 2. Deposit Code Security
- **Cryptographic Generation**: Codes use cryptographically secure random generation
- **Character Set**: Excludes ambiguous characters (0, O, 1, I, l) to prevent confusion
- **Case Insensitive**: Validation is case-insensitive to improve usability
- **Expiration**: All codes have configurable expiration periods
- **Single Use**: Codes can only be used once and are marked as used immediately

### 3. Data Protection
- **Encryption**: Sensitive data encrypted at rest and in transit
- **PII Protection**: Personal information is masked in logs and responses
- **Audit Logging**: All operations are logged for compliance and security
- **Metadata Validation**: Input sanitization prevents injection attacks

### 4. Compliance Features
- **Duplicate Detection**: Automatic detection of duplicate codes across users
- **Review Workflows**: Manual review process for flagged transactions
- **Audit Trails**: Complete activity history for regulatory compliance
- **Reporting**: Comprehensive reporting for compliance teams

### 5. Fraud Prevention
- **Pattern Recognition**: Detection of suspicious usage patterns
- **User Limits**: Configurable daily and transaction limits
- **Geographic Restrictions**: Optional geo-blocking capabilities
- **Risk Scoring**: Automated risk assessment for transactions

### 6. Best Practices for Integration

#### Secure API Key Management
```typescript
// Good: Use environment variables
const apiKey = process.env.QUANTUMSKYLINK_API_KEY;

// Good: Use secure storage in production
const apiKey = await secretManager.getSecret('quantumskylink-api-key');

// Bad: Never hardcode API keys
const apiKey = 'qsl_live_abc123def456'; // DON'T DO THIS
```

#### Error Handling
```typescript
try {
  const result = await depositService.validateDepositCode(validation);
  // Handle success
} catch (error) {
  // Log error for debugging (without sensitive data)
  console.error('Deposit validation failed:', {
    code: error.code,
    message: error.message,
    timestamp: new Date().toISOString()
  });
  
  // Show user-friendly message
  showUserError('Unable to validate deposit code. Please try again.');
}
```

#### Validation Before Processing
```typescript
// Always validate before processing payments
const validationChecks = [
  () => validateDepositCode(code, amount, currency),
  () => validateUserLimits(userId, amount),
  () => validatePaymentMethod(paymentMethodId),
  () => validateCompliance(userId, amount)
];

const allValid = await Promise.all(validationChecks);

if (allValid.every(check => check === true)) {
  await processPayment(paymentRequest);
} else {
  throw new Error('Validation failed');
}
```

---

## Monitoring and Analytics

### 1. Performance Metrics
- **Response Times**: Average API response times tracked per endpoint
- **Success Rates**: Percentage of successful operations vs. failures
- **Throughput**: Requests per second and concurrent user capacity
- **Error Rates**: Detailed breakdown of error types and frequencies

### 2. Business Metrics
- **Code Usage**: Daily/monthly deposit code generation and usage statistics
- **Rejection Rates**: Percentage of codes rejected and common reasons
- **Duplicate Detection**: Frequency and patterns of duplicate codes
- **Review Queue**: Number of codes pending admin review

### 3. Security Monitoring
- **Suspicious Activity**: Detection of unusual patterns or potential fraud
- **Failed Validations**: Tracking of repeated validation failures
- **Rate Limit Violations**: Monitoring of API abuse attempts
- **Authentication Failures**: Failed login attempts and unauthorized access

### 4. Compliance Reporting
- **Audit Logs**: Complete transaction history for regulatory review
- **Data Retention**: Automated data lifecycle management
- **Export Capabilities**: Standardized reporting formats for compliance teams
- **Real-time Alerts**: Immediate notification of high-risk transactions

---

## Support and Troubleshooting

### Common Issues and Solutions

#### 1. "Invalid deposit code" Error
**Cause**: Code not found, expired, or already used
**Solution**: 
- Verify code format (8 alphanumeric characters)
- Check expiration date
- Confirm code hasn't been used previously

#### 2. "Amount mismatch" Error
**Cause**: Payment amount doesn't match deposit code amount
**Solution**:
- Verify the correct amount for the deposit code
- Check if code has amount restrictions

#### 3. "Duplicate code detected" Status
**Cause**: Same code detected from multiple sources
**Solution**:
- Admin review required through AdminAPIGateway
- Investigate source of duplication
- Approve or reject through admin interface

#### 4. "Payment limits exceeded" Error
**Cause**: User has reached daily or transaction limits
**Solution**:
- Check user's payment history
- Verify limits configuration
- Consider limit increase if legitimate

#### 5. High Response Times
**Cause**: Database performance or network issues
**Solution**:
- Monitor database query performance
- Check network connectivity
- Implement caching if not already enabled

### Contact Information
- **API Support**: api-support@quantumskylink.com
- **Security Issues**: security@quantumskylink.com
- **Business Inquiries**: business@quantumskylink.com
- **Documentation**: docs@quantumskylink.com

### Development Resources
- **API Documentation**: https://docs.quantumskylink.com/api
- **SDK Downloads**: https://github.com/quantumskylink/sdks
- **Status Page**: https://status.quantumskylink.com
- **Community Forum**: https://community.quantumskylink.com

---

## Changelog

### Version 2.0.0 (Current)
- Complete rewrite of deposit code validation system
- Enhanced security with cryptographic code generation
- Automated duplicate detection and review workflows
- Integration with QuantumLedger.Hub for distributed tracking
- Advanced fee calculation and rejection handling
- Comprehensive audit trails and compliance features

### Version 1.5.0
- Added bulk operations for admin management
- Improved error handling and response formats
- Enhanced monitoring and analytics capabilities
- Security improvements and fraud prevention

### Version 1.0.0
- Initial release of deposit code validation system
- Basic validation and processing capabilities
- Core API endpoints for code management
- Admin interface for manual review

---

*This documentation is current as of August 4, 2024. For the latest updates and API changes, please refer to the official QuantumSkyLink v2 documentation portal.*