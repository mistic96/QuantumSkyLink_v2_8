# CoreLibrary

A comprehensive .NET library providing authentication middleware, cryptography services, and SurrealDB LINQ extensions.

## Features

- **Authentication Middleware**: Secure API endpoints with ECC-based signature verification
- **Cryptography Services**: ECC operations using the secp256k1 curve
- **SurrealDB Extensions**: LINQ-style query capabilities for SurrealDB
- **Table Permissions**: Granular control over table access with predefined permission packages
- **Ledger Table Support**: Built-in support for tracking and managing ledger tables

## Installation

```bash
dotnet add package CoreLibrary
```

## Usage

### Authentication Middleware

```csharp
// In Program.cs or Startup.cs
using CoreLibrary.Authentication;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add the authentication middleware
app.UseAuthenticationMiddleware();
```

Required configuration in appsettings.json:
```json
{
  "ClientPublicKey": "your_base64_encoded_public_key"
}
```

### Cryptography Services

```csharp
using CoreLibrary.Cryptography;

// Generate new key pair
var (privateKey, publicKey) = ECCService.GenerateKeyPair();

// Verify signature
bool isValid = ECCService.Verify(publicKey, data, r, s);

// Helper methods
string hexString = ECCService.BytesToHex(bytes);
byte[] bytes = ECCService.HexToBytes(hexString);
```

### SurrealDB LINQ Extensions

```csharp
using CoreLibrary.Database;
using SurrealDb.Net;

// Basic query
var results = await db.Select<User>("users", 
    u => u.Age > 18);

// Query with ordering and pagination
var results = await db.Select<User>("users",
    u => u.Age > 18,
    u => u.Name,
    skip: 0,
    take: 10,
    ascending: true);
```

### Table Permissions and Ledger Support

```csharp
using CoreLibrary.Database;
using SurrealDb.Net;

// Create a new table with default read-only permissions
await db.CreateTable("transactions", isLedger: true);

// Apply predefined permission packages to existing tables
await db.ApplyReadOnlyPermissions("users");
await db.ApplyInsertAndUpdatePermissions("orders");

// Mark an existing table as a ledger table
await db.SetLedgerStatus("financial_records", isLedger: true);

// Get all ledger tables
var ledgerTables = await db.GetLedgerTables();

// Initialize all existing tables with default permissions
await db.InitializeAllTables();

// Check current permissions for a table
var permissions = await db.GetTablePermissions("users");
```

Default table permissions:
- All tables default to read-only (SELECT only)
- The InsertAndUpdate package allows SELECT, CREATE, and UPDATE operations
- DELETE operations are restricted by default
- Each table includes an `is_ledger` field for ledger table tracking

## Requirements

- .NET 9.0 or higher
- Dependencies:
  - BouncyCastle.NetCore
  - Microsoft.AspNetCore.Http
  - Microsoft.Extensions.Configuration
  - Microsoft.Extensions.Logging
  - SurrealDB.Net

## Security Considerations

- The authentication middleware enforces timestamp validation to prevent replay attacks
- Uses cryptographically secure ECC operations
- Implements proper error handling to prevent information leakage
- Validates all inputs before processing
- Tables default to read-only access for enhanced security
- DELETE operations are restricted by default and must be explicitly enabled

## License

MIT License. See LICENSE file for details.
