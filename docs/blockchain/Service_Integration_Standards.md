# Service Integration Standards - Zero-Trust Ledger Architecture

## Overview

This document defines the **integration standards** for QuantumSkyLink v2 services to comply with the Zero-Trust Ledger Architecture. **Financial services** must validate monetary transactions with QuantumLedger.Hub, while **non-financial services** leverage Aspire's built-in logging and observability.

## Service Categories

### Financial Services (Require QuantumLedger.Hub Integration)
- **PaymentGatewayService**: Payment processing, transfers
- **TokenService**: Token minting, transfers, burns  
- **MarketplaceService**: Escrow, purchases, sales transactions
- **TreasuryService**: Asset management, treasury operations
- **FeeService**: Fee calculations and collections
- **LiquidationService**: Asset liquidations
- **AccountService**: Balance changes, deposits, withdrawals (financial operations only)

### Non-Financial Services (Use Aspire Defaults Only)
- **UserService**: Registration, profile management
- **NotificationService**: Message delivery
- **ComplianceService**: Reporting, audit queries
- **IdentityVerificationService**: KYC processes
- **GovernanceService**: Voting, proposals (non-monetary)
- **SecurityService**: Authentication, authorization
- **AIReviewService**: Content analysis

### Mixed Services (Context-Dependent Integration)
- **AccountService**: Financial operations (Q.HUB) + Profile updates (Aspire)
- **MarketplaceService**: Transactions (Q.HUB) + Browsing/Search (Aspire)

## Financial Service Integration Requirements

### 1. Required Dependencies for Financial Services

Financial services MUST include these dependencies:

```xml
<!-- In financial service .csproj files -->
<PackageReference Include="QuantumSkyLink.Shared" Version="1.0.0" />
<PackageReference Include="QuantumLedger.Hub.Client" Version="1.0.0" />
<PackageReference Include="SignatureService.Client" Version="1.0.0" />
```

### 2. Financial Service Registration Pattern

Financial services' `Program.cs` MUST include:

```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add Aspire service defaults (logging, health checks, metrics)
    builder.AddServiceDefaults();
    
    // FINANCIAL SERVICES: QuantumLedger.Hub client registration
    builder.Services.AddQuantumLedgerHubClient();
    
    // FINANCIAL SERVICES: SignatureService client registration
    builder.Services.AddSignatureServiceClient();
    
    // Service-specific registrations
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    
    var app = builder.Build();
    
    // Map Aspire defaults (health checks, metrics)
    app.MapDefaultEndpoints();
    
    // FINANCIAL SERVICES: Ledger validation middleware
    app.UseQuantumLedgerValidation();
    
    // FINANCIAL SERVICES: Signature validation middleware
    app.UseSignatureValidation();
    
    // Service-specific middleware and endpoints
    app.MapControllers();
    
    app.Run();
}
```

### 3. Financial Service Aspire References

Financial services in `AppHost.cs` MUST reference QuantumLedger.Hub:

```csharp
// Example for PaymentGatewayService
var paymentGatewayService = builder.AddProject<Projects.PaymentGatewayService>("paymentgatewayservice")
    .WithReference(postgresPaymentGatewayService)
    .WithReference(redis)
    .WithReference(quantumLedgerHub)  // REQUIRED for financial services
    .WithReference(signatureService); // REQUIRED for financial services
```

## Non-Financial Service Integration Requirements

### 1. Dependencies for Non-Financial Services

Non-financial services only need standard dependencies:

```xml
<!-- In non-financial service .csproj files -->
<PackageReference Include="QuantumSkyLink.Shared" Version="1.0.0" />
<!-- NO QuantumLedger.Hub.Client needed -->
<!-- NO SignatureService.Client needed -->
```

### 2. Non-Financial Service Registration Pattern

Non-financial services' `Program.cs` should use Aspire defaults:

```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Add Aspire service defaults (logging, health checks, metrics, observability)
    builder.AddServiceDefaults();
    
    // Service-specific registrations (no QuantumLedger.Hub needed)
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    
    var app = builder.Build();
    
    // Map Aspire defaults (health checks, metrics, logging)
    app.MapDefaultEndpoints();
    
    // Service-specific middleware and endpoints
    app.MapControllers();
    
    app.Run();
}
```

### 3. Non-Financial Service Aspire References

Non-financial services in `AppHost.cs` do NOT reference QuantumLedger.Hub:

```csharp
// Example for UserService
var userService = builder.AddProject<Projects.UserService>("userservice")
    .WithReference(postgresUserService)
    .WithReference(redis);
    // NO quantumLedgerHub reference needed
    // NO signatureService reference needed
```

## Service Implementation Patterns

### Pattern 1: Financial Transaction Processing Services

For services that process financial transactions (PaymentGatewayService, TokenService, MarketplaceService):

```csharp
public class PaymentGatewayService // FINANCIAL SERVICE - Requires QuantumLedger.Hub
{
    private readonly IQuantumLedgerHubClient _ledgerHub;
    private readonly ISignatureService _signatureService;
    private readonly ILogger<PaymentGatewayService> _logger;
    
    public PaymentGatewayService(
        IQuantumLedgerHubClient ledgerHub,
        ISignatureService signatureService,
        ILogger<PaymentGatewayService> logger)
    {
        _ledgerHub = ledgerHub ?? throw new ArgumentNullException(nameof(ledgerHub));
        _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        // STEP 1: Pre-validation logging
        _logger.LogInformation("Processing payment request for user {UserId}, amount {Amount}", 
            request.UserId, request.Amount);
        
        // STEP 2: MANDATORY LEDGER VALIDATION FOR FINANCIAL OPERATIONS
        var validationResult = await _ledgerHub.ValidateTransactionAsync(request, cancellationToken);
        if (!validationResult.IsAuthentic)
        {
            _logger.LogWarning("Payment rejected by ledger: {Reason}", validationResult.RejectionReason);
            return PaymentResult.Rejected(validationResult.RejectionReason);
        }
        
        // STEP 3: MANDATORY SIGNATURE VALIDATION FOR FINANCIAL OPERATIONS
        var signatureResult = await _signatureService.ValidateSignatureAsync(request.Signature, cancellationToken);
        if (!signatureResult.IsValid)
        {
            _logger.LogWarning("Payment rejected - invalid signature");
            return PaymentResult.Rejected("Invalid signature");
        }
        
        // STEP 4: Business logic (only after validation)
        var payment = await ProcessPaymentBusinessLogic(request, cancellationToken);
        
        // STEP 5: MANDATORY LEDGER RECORDING FOR FINANCIAL OPERATIONS
        var ledgerEntry = await _ledgerHub.RecordTransactionAsync(payment, cancellationToken);
        
        _logger.LogInformation("Payment processed successfully: {PaymentId}, LedgerEntry: {LedgerEntryId}", 
            payment.Id, ledgerEntry.Id);
        
        return PaymentResult.Success(payment, ledgerEntry.Id);
    }
}
```

### Pattern 2: Non-Financial Data Services

For services that manage non-financial data (UserService, NotificationService):

```csharp
public class UserService // NON-FINANCIAL SERVICE - Uses Aspire defaults only
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    
    public UserService(
        ILogger<UserService> logger,
        IUserRepository userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }
    
    public async Task<UserResult> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // STEP 1: Process non-financial business logic directly
        _logger.LogInformation("Creating user profile for {Email}", request.Email);
        
        // STEP 2: Validate business rules (no financial validation needed)
        if (await _userRepository.UserExistsAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("User creation failed - email already exists: {Email}", request.Email);
            return UserResult.Failed("User with this email already exists");
        }
        
        // STEP 3: Business logic
        var user = await CreateUserBusinessLogic(request, cancellationToken);
        
        // STEP 4: Aspire handles logging and observability
        _logger.LogInformation("User created successfully: {UserId}", user.Id);
        
        return UserResult.Success(user);
    }
}
```

### Pattern 3: Non-Financial Query Services

For services that primarily query data (ComplianceService, GovernanceService):

```csharp
public class ComplianceService // NON-FINANCIAL SERVICE - Uses Aspire defaults only
{
    private readonly ILogger<ComplianceService> _logger;
    private readonly IComplianceRepository _complianceRepository;
    
    public ComplianceService(
        ILogger<ComplianceService> logger,
        IComplianceRepository complianceRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _complianceRepository = complianceRepository ?? throw new ArgumentNullException(nameof(complianceRepository));
    }
    
    public async Task<ComplianceReport> GenerateComplianceReportAsync(ComplianceReportRequest request, CancellationToken cancellationToken = default)
    {
        // STEP 1: Process non-financial business logic directly
        _logger.LogInformation("Generating compliance report for requester {RequesterId}", request.RequesterId);
        
        // STEP 2: Business logic (no financial validation needed)
        var report = await GenerateReportBusinessLogic(request, cancellationToken);
        
        // STEP 3: Aspire handles logging and observability
        _logger.LogInformation("Compliance report generated: {ReportId}", report.Id);
        
        return report;
    }
}
```

### Pattern 4: Mixed Services (Context-Dependent)

For services that handle both financial and non-financial operations (AccountService):

```csharp
public class AccountService // MIXED SERVICE - Context-dependent routing
{
    private readonly IQuantumLedgerHubClient _ledgerHub; // For financial operations
    private readonly ISignatureService _signatureService; // For financial operations
    private readonly ILogger<AccountService> _logger; // Aspire logging for all operations
    private readonly IAccountRepository _accountRepository;
    
    public AccountService(
        IQuantumLedgerHubClient ledgerHub,
        ISignatureService signatureService,
        ILogger<AccountService> logger,
        IAccountRepository accountRepository)
    {
        _ledgerHub = ledgerHub ?? throw new ArgumentNullException(nameof(ledgerHub));
        _signatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }
    
    // FINANCIAL OPERATION - Uses QuantumLedger.Hub
    public async Task<DepositResult> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default)
    {
        // FINANCIAL OPERATION: Validate with QuantumLedger.Hub
        _logger.LogInformation("Processing deposit - validating with QuantumLedger.Hub");
        var validationResult = await _ledgerHub.ValidateTransactionAsync(request, cancellationToken);
        
        if (!validationResult.IsAuthentic)
        {
            return DepositResult.Rejected(validationResult.RejectionReason);
        }
        
        // Process deposit and record in ledger
        var deposit = await ProcessDepositBusinessLogic(request, cancellationToken);
        var ledgerEntry = await _ledgerHub.RecordTransactionAsync(deposit, cancellationToken);
        
        return DepositResult.Success(deposit, ledgerEntry.Id);
    }
    
    // NON-FINANCIAL OPERATION - Uses Aspire logging only
    public async Task<ProfileResult> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        // NON-FINANCIAL OPERATION: Process directly with Aspire logging
        _logger.LogInformation("Updating user profile for {UserId}", request.UserId);
        
        var profile = await UpdateProfileBusinessLogic(request, cancellationToken);
        
        _logger.LogInformation("Profile updated successfully for {UserId}", request.UserId);
        return ProfileResult.Success(profile);
    }
}
```

## Controller Implementation Standards

### Financial Service Base Controller

Financial service controllers should inherit from `QuantumFinancialBaseController`:

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class QuantumFinancialBaseController : ControllerBase
{
    protected readonly IQuantumLedgerHubClient LedgerHub;
    protected readonly ISignatureService SignatureService;
    protected readonly ILogger Logger;
    
    protected QuantumFinancialBaseController(
        IQuantumLedgerHubClient ledgerHub,
        ISignatureService signatureService,
        ILogger logger)
    {
        LedgerHub = ledgerHub ?? throw new ArgumentNullException(nameof(ledgerHub));
        SignatureService = signatureService ?? throw new ArgumentNullException(nameof(signatureService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    protected async Task<bool> ValidateFinancialRequestAsync<T>(T request, CancellationToken cancellationToken = default)
    {
        var validationResult = await LedgerHub.ValidateTransactionAsync(request, cancellationToken);
        if (!validationResult.IsAuthentic)
        {
            Logger.LogWarning("Financial request validation failed: {Reason}", validationResult.RejectionReason);
            return false;
        }
        return true;
    }
}
```

### Non-Financial Service Base Controller

Non-financial service controllers should inherit from standard `ControllerBase`:

```csharp
[ApiController]
[Route("api/[controller]")]
public abstract class QuantumNonFinancialBaseController : ControllerBase
{
    protected readonly ILogger Logger;
    
    protected QuantumNonFinancialBaseController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### Financial Controller Implementation Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class PaymentController : QuantumFinancialBaseController
{
    private readonly IPaymentService _paymentService;
    
    public PaymentController(
        IQuantumLedgerHubClient ledgerHub,
        ISignatureService signatureService,
        IPaymentService paymentService,
        ILogger<PaymentController> logger)
        : base(ledgerHub, signatureService, logger)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }
    
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPaymentAsync([FromBody] PaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Financial validation is handled by the service layer
            var result = await _paymentService.ProcessPaymentAsync(request, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogWarning(ex, "Unauthorized payment attempt");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing payment");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

### Non-Financial Controller Implementation Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : QuantumNonFinancialBaseController
{
    private readonly IUserService _userService;
    
    public UserController(
        IUserService userService,
        ILogger<UserController> logger)
        : base(logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Non-financial processing - no ledger validation needed
            var result = await _userService.CreateUserAsync(request, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }
}
```

## Error Handling Standards

### Standard Error Response Format

```csharp
public class QuantumErrorResponse
{
    public string ErrorCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
    public string LedgerValidationId { get; set; } // For audit trail
}
```

### Error Handling Implementation

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IQuantumLedgerHubClient _ledgerHub;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = new QuantumErrorResponse
        {
            ErrorCode = GetErrorCode(exception),
            Message = exception.Message,
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };
        
        // Log error to ledger for audit trail
        await _ledgerHub.RecordTransactionAsync(new ErrorEvent
        {
            ErrorCode = errorResponse.ErrorCode,
            Message = errorResponse.Message,
            TraceId = errorResponse.TraceId,
            OccurredAt = errorResponse.Timestamp
        });
        
        context.Response.StatusCode = GetStatusCode(exception);
        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
```

## Validation Middleware Implementation

### QuantumLedgerValidationMiddleware

```csharp
public class QuantumLedgerValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IQuantumLedgerHubClient _ledgerHub;
    private readonly ILogger<QuantumLedgerValidationMiddleware> _logger;
    
    public QuantumLedgerValidationMiddleware(
        RequestDelegate next,
        IQuantumLedgerHubClient ledgerHub,
        ILogger<QuantumLedgerValidationMiddleware> logger)
    {
        _next = next;
        _ledgerHub = ledgerHub;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip validation for health checks and metrics
        if (IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        // Extract and validate request
        var request = await ExtractRequestAsync(context);
        if (request != null)
        {
            var validationResult = await _ledgerHub.ValidateTransactionAsync(request);
            if (!validationResult.IsAuthentic)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Transaction not authenticated by ledger");
                return;
            }
        }
        
        await _next(context);
    }
    
    private bool IsExcludedPath(PathString path)
    {
        var excludedPaths = new[] { "/health", "/metrics", "/alive", "/ready" };
        return excludedPaths.Any(excluded => path.StartsWithSegments(excluded));
    }
}
```

## Testing Standards

### Financial Service Unit Test Requirements

Financial services MUST include unit tests that verify ledger integration:

```csharp
[TestClass]
public class PaymentServiceTests // FINANCIAL SERVICE TESTS
{
    private Mock<IQuantumLedgerHubClient> _mockLedgerHub;
    private Mock<ISignatureService> _mockSignatureService;
    private PaymentService _paymentService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockLedgerHub = new Mock<IQuantumLedgerHubClient>();
        _mockSignatureService = new Mock<ISignatureService>();
        _paymentService = new PaymentService(_mockLedgerHub.Object, _mockSignatureService.Object, Mock.Of<ILogger<PaymentService>>());
    }
    
    [TestMethod]
    public async Task ProcessPaymentAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new PaymentRequest { UserId = "user1", Amount = 100 };
        _mockLedgerHub.Setup(x => x.ValidateTransactionAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransactionValidationResult { IsAuthentic = true });
        _mockSignatureService.Setup(x => x.ValidateSignatureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SignatureValidationResult { IsValid = true });
        
        // Act
        var result = await _paymentService.ProcessPaymentAsync(request);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockLedgerHub.Verify(x => x.ValidateTransactionAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        _mockLedgerHub.Verify(x => x.RecordTransactionAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [TestMethod]
    public async Task ProcessPaymentAsync_InvalidLedgerValidation_ReturnsRejected()
    {
        // Arrange
        var request = new PaymentRequest { UserId = "user1", Amount = 100 };
        _mockLedgerHub.Setup(x => x.ValidateTransactionAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransactionValidationResult { IsAuthentic = false, RejectionReason = "Invalid transaction" });
        
        // Act
        var result = await _paymentService.ProcessPaymentAsync(request);
        
        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("Invalid transaction", result.ErrorMessage);
        _mockLedgerHub.Verify(x => x.RecordTransactionAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

### Non-Financial Service Unit Test Requirements

Non-financial services should include standard unit tests without ledger integration:

```csharp
[TestClass]
public class UserServiceTests // NON-FINANCIAL SERVICE TESTS
{
    private Mock<IUserRepository> _mockUserRepository;
    private UserService _userService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _userService = new UserService(Mock.Of<ILogger<UserService>>(), _mockUserRepository.Object);
    }
    
    [TestMethod]
    public async Task CreateUserAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "test@example.com", FirstName = "Test", LastName = "User" };
        _mockUserRepository.Setup(x => x.UserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockUserRepository.Verify(x => x.UserExistsAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        // NO ledger validation or recording needed for non-financial operations
    }
    
    [TestMethod]
    public async Task CreateUserAsync_UserExists_ReturnsFailed()
    {
        // Arrange
        var request = new CreateUserRequest { Email = "existing@example.com", FirstName = "Test", LastName = "User" };
        _mockUserRepository.Setup(x => x.UserExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("User with this email already exists", result.ErrorMessage);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
```

## Integration Test Requirements

### Financial Service Integration Tests

Financial services MUST include integration tests that verify end-to-end ledger integration:

```csharp
[TestClass]
public class PaymentServiceIntegrationTests // FINANCIAL SERVICE INTEGRATION TESTS
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task ProcessPayment_WithValidLedgerValidation_ReturnsSuccess()
    {
        // This test requires actual QuantumLedger.Hub and SignatureService instances
        // Use test containers or test doubles that implement the full interface
        
        var request = new PaymentRequest
        {
            UserId = "test-user",
            Amount = 50.00m,
            Currency = "USD",
            Signature = "valid-test-signature"
        };
        
        var response = await _client.PostAsJsonAsync("/api/payment/process", request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.LedgerEntryId);
    }
}
```

### Non-Financial Service Integration Tests

Non-financial services should include standard integration tests without ledger dependencies:

```csharp
[TestClass]
public class UserServiceIntegrationTests // NON-FINANCIAL SERVICE INTEGRATION TESTS
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [TestMethod]
    public async Task CreateUser_WithValidRequest_ReturnsSuccess()
    {
        // This test uses standard Aspire testing without QuantumLedger.Hub dependencies
        
        var request = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };
        
        var response = await _client.PostAsJsonAsync("/api/user/create", request);
        
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<UserResult>();
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.User);
        // NO ledger entry ID expected for non-financial operations
    }
}
```

## Performance Requirements

### Response Time Standards

- **Ledger validation**: < 100ms (95th percentile)
- **Signature validation**: < 50ms (95th percentile)
- **Total transaction processing**: < 500ms (95th percentile)

### Monitoring Requirements

Every service MUST implement these metrics:

```csharp
public class ServiceMetrics
{
    private readonly Counter<int> _ledgerValidationCounter;
    private readonly Histogram<double> _ledgerValidationDuration;
    private readonly Counter<int> _signatureValidationCounter;
    private readonly Histogram<double> _signatureValidationDuration;
    
    public ServiceMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("QuantumSkyLink.Service");
        
        _ledgerValidationCounter = meter.CreateCounter<int>("ledger_validations_total");
        _ledgerValidationDuration = meter.CreateHistogram<double>("ledger_validation_duration_ms");
        _signatureValidationCounter = meter.CreateCounter<int>("signature_validations_total");
        _signatureValidationDuration = meter.CreateHistogram<double>("signature_validation_duration_ms");
    }
    
    public void RecordLedgerValidation(bool success, double durationMs)
    {
        _ledgerValidationCounter.Add(1, new KeyValuePair<string, object?>("success", success));
        _ledgerValidationDuration.Record(durationMs);
    }
}
```

## Compliance Checklist

### Financial Service Deployment Checklist

Before deploying any **financial service**, verify:

- [ ] QuantumLedger.Hub client is registered and injected
- [ ] SignatureService client is registered and injected
- [ ] All financial transaction methods validate with ledger first
- [ ] All financial transactions are recorded in ledger
- [ ] Error handling includes ledger audit logging
- [ ] Unit tests cover ledger validation scenarios
- [ ] Integration tests verify end-to-end ledger flow
- [ ] Financial service metrics are implemented and exported
- [ ] Service references QuantumLedger.Hub in AppHost.cs
- [ ] Service references SignatureService in AppHost.cs
- [ ] Financial validation middleware is configured
- [ ] Signature validation middleware is configured

### Non-Financial Service Deployment Checklist

Before deploying any **non-financial service**, verify:

- [ ] Aspire service defaults are configured (`builder.AddServiceDefaults()`)
- [ ] Standard logging is implemented using ILogger
- [ ] Health check endpoints are configured (`app.MapDefaultEndpoints()`)
- [ ] Service-specific business logic is implemented
- [ ] Unit tests cover business logic scenarios
- [ ] Integration tests verify service functionality
- [ ] Standard Aspire metrics are exported
- [ ] Service does NOT reference QuantumLedger.Hub unnecessarily
- [ ] Service does NOT reference SignatureService unnecessarily
- [ ] No financial validation middleware is configured

### Mixed Service Deployment Checklist

Before deploying any **mixed service** (handles both financial and non-financial operations), verify:

- [ ] QuantumLedger.Hub client is registered for financial operations
- [ ] SignatureService client is registered for financial operations
- [ ] Aspire service defaults are configured for all operations
- [ ] Financial operations validate with ledger first
- [ ] Non-financial operations use Aspire logging only
- [ ] Context-dependent routing is implemented correctly
- [ ] Unit tests cover both financial and non-financial scenarios
- [ ] Integration tests verify both operation types
- [ ] Conditional validation middleware is configured
- [ ] Service references QuantumLedger.Hub in AppHost.cs
- [ ] Service references SignatureService in AppHost.cs

This ensures every service in the QuantumSkyLink v2 platform follows the appropriate integration pattern for its service type while maintaining the highest levels of security and auditability for financial operations.
