# QuantumSkyLink v2 - .NET Aspire E2E Testing Plan

**Date**: January 17, 2025  
**Version**: 1.0  
**Status**: Comprehensive Plan Complete - Ready for Implementation

---

## Executive Summary

This document outlines a comprehensive .NET Aspire End-to-End (E2E) testing strategy for the QuantumSkyLink v2 platform. The approach focuses on mobile-first testing through the MobileAPIGateway as a single entry point, with emphasis on critical financial operations including deposits and liquidation/withdrawal workflows.

### Key Highlights
- **Mobile-Focused Architecture**: Single entry point through MobileAPIGateway for realistic testing
- **Financial Operations Priority**: Comprehensive deposit and withdrawal/liquidation workflow testing
- **Industry-Standard Patterns**: DistributedApplicationTestingBuilder implementation
- **Complete Backend Validation**: Tests all 24 services through mobile interface
- **Performance Targets**: Specific goals for financial operations (deposits ≤2s, withdrawals ≤5s)

---

## Table of Contents

1. [Platform Overview](#platform-overview)
2. [Testing Strategy](#testing-strategy)
3. [Technical Architecture](#technical-architecture)
4. [Implementation Plan](#implementation-plan)
5. [Code Examples](#code-examples)
6. [Performance Targets](#performance-targets)
7. [Success Metrics](#success-metrics)
8. [Integration with Existing Infrastructure](#integration-with-existing-infrastructure)

---

## Platform Overview

### QuantumSkyLink v2 Current Status
- ✅ **24 Microservices**: All services building successfully and fully functional
- ✅ **500+ REST Endpoints**: Comprehensive API coverage across 53 controllers
- ✅ **18 Dedicated Databases**: Database-per-service architecture with Neon PostgreSQL
- ✅ **3 API Gateways**: Web, Admin, and Mobile gateways operational
- ✅ **Aspire Orchestration**: Complete service discovery and observability

### Mobile API Gateway Architecture
The MobileAPIGateway serves as the primary entry point for testing, featuring:
- **13 Controllers**: Complete mobile functionality coverage
- **Financial Operations**: Deposit, withdrawal, wallet, and payment endpoints
- **Authentication**: JWT-based authentication with role-based access
- **Service Integration**: Refit clients for all backend services

**Key Mobile Controllers**:
- `AuthController` - Authentication and authorization
- `UserController` - User management and profiles  
- `WalletController` - Wallet operations and management
- `CartsController` - Shopping cart functionality
- `MarketsController` - Market data and operations
- `SearchController` - Search and discovery
- `GlobalController` - Global platform data
- `DashboardController` - User dashboard data
- `CardManagementController` - Payment card management

---

## Testing Strategy

### Core Testing Principles

#### 1. Mobile-First Approach
- **Single Entry Point**: All testing through MobileAPIGateway
- **Realistic Usage Patterns**: Mirrors actual mobile application usage
- **Complete Backend Validation**: Tests entire backend through mobile interface
- **Performance Optimization**: Mobile-specific performance requirements

#### 2. Financial Operations Focus
- **Deposit Workflows**: Cryptocurrency and fiat deposit testing
- **Withdrawal/Liquidation**: Complete liquidation service testing
- **Trading Lifecycle**: End-to-end trading workflow validation
- **Multi-Currency Support**: BTC, ETH, USD, EUR operations

#### 3. Industry-Standard Implementation
- **DistributedApplicationTestingBuilder**: .NET Aspire testing patterns
- **xUnit Collection Fixtures**: Shared application lifecycle management
- **Performance Optimization**: Shared fixtures for faster test execution
- **Comprehensive Coverage**: All critical workflows and edge cases

---

## Technical Architecture

### .NET Aspire Testing Foundation

#### Core Technologies
- **.NET 9**: Latest framework with performance optimizations
- **Aspire 9.3.0**: Service orchestration and testing capabilities
- **xUnit**: Testing framework with collection fixtures
- **FluentAssertions**: Readable test assertions
- **DistributedApplicationTestingBuilder**: Aspire testing pattern

#### Testing Framework Dependencies
```xml
<PackageReference Include="Aspire.Hosting.Testing" Version="9.3.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

### Project Structure

```
src/QuantumSkyLink.Mobile.E2E.Tests/
├── Fixtures/
│   ├── MobileAppHostFixture.cs        # Aspire app lifecycle management
│   ├── MobileTestCollection.cs        # xUnit collection for shared fixtures
│   └── MobileTestDataFixture.cs       # Test data management
├── Tests/
│   ├── Authentication/                # Auth flow testing
│   │   ├── AuthenticationE2ETests.cs
│   │   └── UserOnboardingE2ETests.cs
│   ├── FinancialOperations/           # Deposit/withdrawal testing
│   │   ├── DepositWorkflowE2ETests.cs
│   │   ├── WithdrawalWorkflowE2ETests.cs
│   │   └── LiquidationE2ETests.cs
│   ├── UserJourneys/                  # Complete user workflows
│   │   ├── CompleteTradeJourneyE2ETests.cs
│   │   └── CrossCurrencyOperationsE2ETests.cs
│   ├── Performance/                   # Load and performance testing
│   │   ├── ConcurrentUsersE2ETests.cs
│   │   └── PerformanceValidationE2ETests.cs
│   └── WorkflowIntegration/           # Kestra workflow testing
│       ├── MobileWorkflowE2ETests.cs
│       └── PaymentWorkflowE2ETests.cs
├── Utilities/
│   ├── MobileApiClient.cs             # Mobile API client wrapper
│   ├── FinancialTestDataFactory.cs    # Financial test data generation
│   └── MobileTestHelpers.cs           # Mobile-specific test utilities
└── QuantumSkyLink.Mobile.E2E.Tests.csproj
```

---

## Implementation Plan

### Phase 1: Mobile-Focused Aspire Test Foundation (Week 1)

#### Objectives
- Create E2E test project with Aspire testing dependencies
- Implement shared application lifecycle management
- Set up mobile API client wrappers
- Establish basic test infrastructure

#### Deliverables
1. **Create Test Project**
   ```bash
   dotnet new xunit -n QuantumSkyLink.Mobile.E2E.Tests
   cd QuantumSkyLink.Mobile.E2E.Tests
   dotnet add package Aspire.Hosting.Testing --version 9.3.0
   dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0
   dotnet add package FluentAssertions --version 6.12.0
   ```

2. **Implement MobileAppHostFixture**
3. **Create MobileApiClient wrapper**
4. **Set up xUnit collection fixtures**
5. **Implement basic test utilities**

#### Success Criteria
- Test project builds successfully
- Aspire application starts and stops correctly in tests
- Mobile API client can communicate with all 13 controllers
- Basic authentication flow test passes

### Phase 2: Complete Mobile User Journey Testing (Week 2)

#### Objectives
- Implement comprehensive financial operations testing
- Create complete user journey scenarios
- Add inter-service integration testing
- Validate API gateway routing and authentication

#### Deliverables
1. **Authentication and Onboarding Tests**
2. **Financial Operations Test Suite**
3. **Market Operations Testing**
4. **Cross-Service Integration Tests**
5. **Database Integration Testing**

#### Success Criteria
- All 13 mobile controllers have E2E test coverage
- Complete deposit-to-withdrawal lifecycle tests pass
- Authentication and authorization flows validated
- Database-per-service architecture tested

### Phase 3: Advanced Testing and Performance Validation (Week 3)

#### Objectives
- Implement performance and load testing
- Add security and compliance testing
- Create monitoring integration
- Complete production readiness validation

#### Deliverables
1. **Performance Testing Suite**
2. **Security and Compliance Tests**
3. **Kestra Workflow Integration**
4. **Monitoring and Observability Integration**
5. **Production Readiness Validation**

#### Success Criteria
- 500+ concurrent users performance test passes
- All performance targets met
- Security and compliance scenarios validated
- Complete production deployment readiness

---

## Code Examples

### 1. Mobile App Host Fixture

```csharp
public class MobileAppHostFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; }
    public HttpClient MobileApiClient { get; private set; }
    
    public async Task InitializeAsync()
    {
        var appHost = await new DistributedApplicationTestingBuilder<Projects.QuantunSkyLink_v2_AppHost>()
            .BuildAsync();
        App = appHost;
        await App.StartAsync();
        
        // Single entry point - Mobile API Gateway
        MobileApiClient = App.CreateHttpClient("mobileapigateway");
    }
    
    public async Task DisposeAsync()
    {
        MobileApiClient?.Dispose();
        await App.StopAsync();
    }
}

[CollectionDefinition("MobileE2E")]
public class MobileE2ETestCollection : ICollectionFixture<MobileAppHostFixture> { }
```

### 2. Financial Operations Testing

```csharp
[Collection("MobileE2E")]
public class MobileFinancialOperationsE2ETests
{
    private readonly HttpClient _mobileApi;
    
    public MobileFinancialOperationsE2ETests(MobileAppHostFixture fixture)
    {
        _mobileApi = fixture.MobileApiClient;
    }
    
    [Fact]
    public async Task CompleteDepositFlow_CryptocurrencyDeposit_Success()
    {
        // Test complete deposit workflow through mobile gateway
        // 1. User authentication via AuthController
        // 2. Get deposit address via WalletController.GetDepositAddressAsync()
        // 3. Simulate external blockchain deposit
        // 4. Verify balance update via WalletController.GetWalletBalanceAsync()
        // 5. Check transaction history via WalletController.GetWalletTransactionsAsync()
        
        // This tests: UserService → InfrastructureService → WalletService → AccountService
        var depositRequest = new DepositRequest
        {
            WalletId = "test-wallet-id",
            CurrencyCode = "BTC",
            BlockchainNetwork = "Bitcoin"
        };
        
        var depositResponse = await _mobileApi.PostAsJsonAsync("/api/wallets/deposit", depositRequest);
        depositResponse.EnsureSuccessStatusCode();
        
        var depositData = await depositResponse.Content.ReadFromJsonAsync<DepositResponse>();
        depositData.DepositAddress.Should().NotBeNull();
        depositData.MinimumDepositAmount.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public async Task CompleteWithdrawalFlow_LiquidationWithdrawal_Success()
    {
        // Test complete withdrawal/liquidation workflow
        // 1. Check available balance via WalletController.GetWalletBalanceAsync()
        // 2. Validate withdrawal eligibility (LiquidationService.AssetEligibilityController)
        // 3. Get current market pricing (LiquidationService.MarketPricingController)
        // 4. Process withdrawal via WalletController.WithdrawAsync()
        // 5. Verify compliance checks (LiquidationService.ComplianceController)
        // 6. Confirm transaction completion
        
        // This tests: WalletService → LiquidationService → ComplianceService → PaymentGatewayService
        var withdrawRequest = new WithdrawRequest
        {
            WalletId = "test-wallet-id",
            Amount = 0.001m,
            CurrencyCode = "BTC",
            DestinationAddress = "bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh",
            WithdrawalType = "Cryptocurrency"
        };
        
        var withdrawResponse = await _mobileApi.PostAsJsonAsync("/api/wallets/withdraw", withdrawRequest);
        withdrawResponse.EnsureSuccessStatusCode();
        
        var withdrawData = await withdrawResponse.Content.ReadFromJsonAsync<WithdrawResponse>();
        withdrawData.TransactionId.Should().NotBeNull();
        withdrawData.Status.Should().Be("Pending");
    }
}
```

### 3. Mobile API Client Wrapper

```csharp
public class MobileApiClient
{
    private readonly HttpClient _httpClient;
    
    public MobileApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    // Authentication operations
    public async Task<T> AuthenticateAsync<T>(object request) =>
        await PostAsync<T>("/api/auth/login", request);
        
    public async Task<T> RegisterUserAsync<T>(object request) =>
        await PostAsync<T>("/api/auth/register", request);
    
    // User operations
    public async Task<T> GetUserProfileAsync<T>() =>
        await GetAsync<T>("/api/user/profile");
        
    public async Task<T> UpdateUserProfileAsync<T>(object request) =>
        await PutAsync<T>("/api/user/profile", request);
    
    // Wallet operations
    public async Task<T> GetWalletBalanceAsync<T>() =>
        await GetAsync<T>("/api/wallets/balances");
        
    public async Task<T> GetDepositAddressAsync<T>(object request) =>
        await PostAsync<T>("/api/wallets/deposit", request);
        
    public async Task<T> ProcessWithdrawalAsync<T>(object request) =>
        await PostAsync<T>("/api/wallets/withdraw", request);
    
    // Market operations
    public async Task<T> SearchMarketsAsync<T>(string query) =>
        await GetAsync<T>($"/api/search?q={query}");
        
    public async Task<T> GetMarketDataAsync<T>() =>
        await GetAsync<T>("/api/markets");
    
    // Cart operations
    public async Task<T> AddToCartAsync<T>(object request) =>
        await PostAsync<T>("/api/carts/items", request);
        
    public async Task<T> GetCartAsync<T>() =>
        await GetAsync<T>("/api/carts");
    
    // Helper methods
    private async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
    
    private async Task<T> PostAsync<T>(string endpoint, object request)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
    
    private async Task<T> PutAsync<T>(string endpoint, object request)
    {
        var response = await _httpClient.PutAsJsonAsync(endpoint, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
```

### 4. Complete User Journey Testing

```csharp
[Collection("MobileE2E")]
public class MobileCompleteJourneyE2ETests
{
    private readonly MobileApiClient _mobileApi;
    
    public MobileCompleteJourneyE2ETests(MobileAppHostFixture fixture)
    {
        _mobileApi = new MobileApiClient(fixture.MobileApiClient);
    }
    
    [Fact]
    public async Task CompleteTrading_DepositTradeLiquidate_Success()
    {
        // Complete trading lifecycle through mobile gateway
        // 1. User authentication and onboarding
        var authRequest = FinancialTestDataFactory.CreateAuthRequest();
        var authResponse = await _mobileApi.AuthenticateAsync<AuthResponse>(authRequest);
        authResponse.Token.Should().NotBeNull();
        
        // 2. Deposit funds via WalletController.GetDepositAddressAsync()
        var depositRequest = FinancialTestDataFactory.CreateCryptoDepositRequest();
        var depositResponse = await _mobileApi.GetDepositAddressAsync<DepositResponse>(depositRequest);
        depositResponse.DepositAddress.Should().NotBeNull();
        
        // 3. Purchase tokens via MarketsController
        var marketData = await _mobileApi.GetMarketDataAsync<MarketDataResponse>();
        marketData.Markets.Should().NotBeEmpty();
        
        // 4. Trade on secondary markets via SecondaryMarketsController
        var searchResults = await _mobileApi.SearchMarketsAsync<SearchResponse>("BTC");
        searchResults.Results.Should().NotBeEmpty();
        
        // 5. Liquidate positions via WalletController.WithdrawAsync()
        var withdrawRequest = FinancialTestDataFactory.CreateCryptoWithdrawRequest();
        var withdrawResponse = await _mobileApi.ProcessWithdrawalAsync<WithdrawResponse>(withdrawRequest);
        withdrawResponse.TransactionId.Should().NotBeNull();
        
        // This tests the complete financial ecosystem through mobile gateway
        // Validates: UserService → AccountService → MarketplaceService → 
        //           PaymentGatewayService → LiquidationService → TreasuryService
    }
    
    [Fact]
    public async Task CrossCurrencyOperations_MultiAsset_Success()
    {
        // Test multi-currency operations through mobile
        // 1. Deposit multiple cryptocurrencies
        var btcDeposit = await _mobileApi.GetDepositAddressAsync<DepositResponse>(
            FinancialTestDataFactory.CreateCryptoDepositRequest("BTC"));
        var ethDeposit = await _mobileApi.GetDepositAddressAsync<DepositResponse>(
            FinancialTestDataFactory.CreateCryptoDepositRequest("ETH"));
        
        btcDeposit.DepositAddress.Should().NotBeNull();
        ethDeposit.DepositAddress.Should().NotBeNull();
        
        // 2. Internal currency conversion
        var balances = await _mobileApi.GetWalletBalanceAsync<WalletBalanceResponse>();
        balances.Balances.Should().NotBeEmpty();
        
        // 3. Cross-currency trading
        var markets = await _mobileApi.GetMarketDataAsync<MarketDataResponse>();
        markets.Markets.Should().Contain(m => m.BaseCurrency == "BTC" && m.QuoteCurrency == "ETH");
        
        // 4. Multi-asset withdrawal
        var btcWithdraw = await _mobileApi.ProcessWithdrawalAsync<WithdrawResponse>(
            FinancialTestDataFactory.CreateCryptoWithdrawRequest("BTC"));
        var ethWithdraw = await _mobileApi.ProcessWithdrawalAsync<WithdrawResponse>(
            FinancialTestDataFactory.CreateCryptoWithdrawRequest("ETH"));
        
        btcWithdraw.TransactionId.Should().NotBeNull();
        ethWithdraw.TransactionId.Should().NotBeNull();
        
        // This tests: TreasuryService → FeeService → MarketplaceService
    }
}
```

### 5. Performance Testing

```csharp
[Collection("MobileE2E")]
public class MobilePerformanceE2ETests
{
    private readonly MobileApiClient _mobileApi;
    
    public MobilePerformanceE2ETests(MobileAppHostFixture fixture)
    {
        _mobileApi = new MobileApiClient(fixture.MobileApiClient);
    }
    
    [Fact]
    public async Task HighConcurrency_500MobileUsers_Success()
    {
        // Simulate 500 concurrent mobile users
        // All traffic goes through MobileAPIGateway
        // Tests entire backend under mobile load
        // Validates response times for mobile-specific requirements
        
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 500; i++)
        {
            tasks.Add(SimulateMobileUserJourney(i));
        }
        
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        // Validate all operations completed within performance targets
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max for 500 users
        
        // Individual operation targets validated within SimulateMobileUserJourney
    }
    
    private async Task SimulateMobileUserJourney(int userId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // 1. Authentication (should be ≤1 second)
        var authRequest = FinancialTestDataFactory.CreateAuthRequest($"testuser{userId}");
        var authResponse = await _mobileApi.AuthenticateAsync<AuthResponse>(authRequest);
        var authTime = stopwatch.ElapsedMilliseconds;
        authTime.Should().BeLessThan(1000); // ≤1 second
        
        stopwatch.Restart();
        
        // 2. Get deposit address (should be ≤2 seconds)
        var depositRequest = FinancialTestDataFactory.CreateCryptoDepositRequest();
        var depositResponse = await _mobileApi.GetDepositAddressAsync<DepositResponse>(depositRequest);
        var depositTime = stopwatch.ElapsedMilliseconds;
        depositTime.Should().BeLessThan(2000); // ≤2 seconds
        
        stopwatch.Restart();
        
        // 3. Check balance (should be ≤1 second)
        var balanceResponse = await _mobileApi.GetWalletBalanceAsync<WalletBalanceResponse>();
        var balanceTime = stopwatch.ElapsedMilliseconds;
        balanceTime.Should().BeLessThan(1000); // ≤1 second
        
        stopwatch.Restart();
        
        // 4. Process withdrawal (should be ≤5 seconds for crypto)
        var withdrawRequest = FinancialTestDataFactory.CreateCryptoWithdrawRequest();
        var withdrawResponse = await _mobileApi.ProcessWithdrawalAsync<WithdrawResponse>(withdrawRequest);
        var withdrawTime = stopwatch.ElapsedMilliseconds;
        withdrawTime.Should().BeLessThan(5000); // ≤5 seconds for crypto
        
        // Validate responses
        authResponse.Token.Should().NotBeNull();
        depositResponse.DepositAddress.Should().NotBeNull();
        balanceResponse.Balances.Should().NotBeNull();
        withdrawResponse.TransactionId.Should().NotBeNull();
    }
}
```

---

## Performance Targets

### Financial Operations Performance Requirements

| Operation | Target Time | Validation Method |
|-----------|-------------|-------------------|
| **Deposit Address Generation** | ≤2 seconds | API response time measurement |
| **Cryptocurrency Withdrawal** | ≤5 seconds | End-to-end processing time |
| **Fiat Withdrawal** | ≤10 seconds | Complete workflow validation |
| **Balance Updates** | ≤1 second | After transaction confirmation |
| **Liquidation Execution** | ≤3 seconds | Market order processing |
| **Authentication** | ≤1 second | JWT token generation |
| **Market Data Retrieval** | ≤500ms | Real-time data access |
| **Search Operations** | ≤300ms | Search index response |

### Concurrent User Performance

| Scenario | Target | Validation Method |
|----------|--------|-------------------|
| **Concurrent Mobile Users** | 500+ users | Load testing simulation |
| **Simultaneous Deposits** | 100+ concurrent | Parallel operation testing |
| **Simultaneous Withdrawals** | 50+ concurrent | Compliance bottleneck testing |
| **Market Data Requests** | 1000+ concurrent | Cache effectiveness testing |

### System Performance Requirements

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Response Time** | 95th percentile ≤1s | Performance monitoring |
| **Database Query Time** | 95th percentile ≤200ms | Database performance metrics |
| **Service-to-Service Calls** | 95th percentile ≤100ms | Distributed tracing |
| **Memory Usage** | ≤2GB per service | Resource monitoring |
| **CPU Usage** | ≤70% under load | System metrics |

---

## Success Metrics

### Coverage Goals

#### API Coverage
- **100% Mobile Controller Coverage**: All 13 controllers tested end-to-end
- **100% Financial Endpoint Coverage**: All deposit/withdrawal endpoints
- **95% Overall Endpoint Coverage**: 475+ of 500+ REST endpoints tested
- **100% Authentication Flow Coverage**: All auth scenarios validated

#### Workflow Coverage
- **100% Financial Workflow Coverage**: Complete deposit-to-withdrawal lifecycle
- **100% User Journey Coverage**: Registration through trading to withdrawal
- **90% Edge Case Coverage**: Error scenarios and boundary conditions
- **100% Multi-Currency Coverage**: BTC, ETH, USD, EUR operations

#### Service Integration Coverage
- **100% Service Chain Coverage**: All 24 services tested through mobile gateway
- **100% Database Coverage**: All 18 databases tested through service operations
- **100% API Gateway Coverage**: Mobile gateway routing and authentication
- **90% Inter-Service Communication**: Service-to-service call validation

### Quality Assurance Metrics

#### Reliability
- **99.9% Test Pass Rate**: Consistent test execution success
- **Zero Flaky Tests**: Reliable, repeatable test execution
- **100% Deterministic Results**: Consistent outcomes across runs
- **Complete Isolation**: Tests don't interfere with each other

#### Performance Validation
- **100% Performance Target Achievement**: All operations meet defined targets
- **Load Testing Success**: 500+ concurrent users without degradation
- **Stress Testing Validation**: System behavior under extreme load
- **Resource Utilization Optimization**: Efficient resource usage validation

#### Security and Compliance
- **100% Authentication Scenario Coverage**: All auth flows validated
- **100% Authorization Testing**: Role-based access control validation
- **Complete Audit Trail Validation**: All financial operations logged
- **Compliance Workflow Testing**: KYC/AML process validation

---

## Integration with Existing Infrastructure

### UserService.Tests Foundation

The existing `UserService.Tests` project provides a solid foundation for expansion:

```csharp
// Existing TestBase.cs pattern
public abstract class TestBase : IDisposable
{
    protected UserDbContext Context { get; private set; }
    protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new UserDbContext(options);
        SeedTestData();
    }
    
    // Enhanced for E2E testing
    protected virtual void SeedTestData()
    {
        // Existing test data seeding
        // Enhanced with financial operation test data
    }
}
```

**Enhancement Strategy**:
1. **Extend TestBase**: Add mobile-specific test utilities
2. **Database Integration**: Leverage in-memory database patterns
3. **Service Mocking**: Build on existing mock patterns
4. **Test Data Management**: Enhance existing test data factory

### Kestra Workflow Integration

The platform has 19 existing Kestra workflows that can be integrated into E2E testing:

#### Production Workflows (7 workflows)
1. `marketplace-listing-creation.yaml` - ≤3 seconds target
2. `marketplace-order-processing.yaml` - ≤5 seconds target  
3. `marketplace-escrow-management.yaml` - ≤7 seconds target
4. `marketplace-analytics-processing.yaml` - ≤10 seconds target
5. `payment-processing-zero-trust.yaml` - ≤5 seconds target
6. `user-onboarding-optimized.yaml` - ≤10 seconds target
7. `treasury-operations-secure.yaml` - ≤15 seconds target

#### Mobile Test Workflows (12 workflows)
- `mobile-e2e-testing.yaml` - Complete mobile E2E testing
- `mobile-user-onboarding.yaml` - Mobile user onboarding flow
- `mobile-payment-processing.yaml` - Mobile payment processing
- `mobile-marketplace-operations.yaml` - Mobile marketplace operations
- `test-mobile-api-*.yaml` (8 specific API test workflows)

**Integration Approach**:
```csharp
[Collection("MobileE2E")]
public class MobileWorkflowIntegrationE2ETests
{
    [Fact]
    public async Task MobilePaymentProcessing_Workflow_Success()
    {
        // Test mobile-payment-processing.yaml workflow
        // Triggered by mobile deposit/withdrawal actions
        // Target: ≤5 seconds with zero-trust validation
        // Validates: SignatureService → PaymentGatewayService → LiquidationService
        
        var workflowTrigger = new WorkflowTriggerRequest
        {
            WorkflowId = "mobile-payment-processing",
            Parameters = new { UserId = "test-user", Amount = 0.001m, Currency = "BTC" }
        };
        
        var stopwatch = Stopwatch.StartNew();
        var result = await _kestraClient.TriggerWorkflowAsync(workflowTrigger);
        stopwatch.Stop();
        
        result.Status.Should().Be("SUCCESS");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // ≤5 seconds target
    }
}
```

### Database-per-Service Architecture Integration

The platform's 18 dedicated databases require specific testing approaches:

#### Database Testing Strategy
```csharp
public class DatabaseIntegrationTestBase : TestBase
{
    protected readonly Dictionary<string, DbContext> ServiceDatabases;
    
    protected DatabaseIntegrationTestBase()
    {
        ServiceDatabases = new Dictionary<string, DbContext>
        {
            ["UserService"] = CreateInMemoryContext<UserDbContext>(),
            ["AccountService"] = CreateInMemoryContext<AccountDbContext>(),
            ["PaymentGatewayService"] = CreateInMemoryContext<PaymentDbContext>(),
            // ... all 18 service databases
        };
    }
    
    private T CreateInMemoryContext<T>() where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return (T)Activator.CreateInstance(typeof(T), options);
    }
}
```

### Aspire Service Discovery Integration

Leverage existing Aspire service registration patterns:

```csharp
// Integration with existing AppHost.cs service registration
public class AspireServiceIntegrationTests
{
    [Fact]
    public async Task AllServices_RegisteredCorrectly_Success()
    {
        // Validate all 24 services are registered and discoverable
        var services = new[]
        {
            "userservice", "accountservice", "paymentgatewayservice",
            "treasuryservice", "tokenservice", "feeservice",
            // ... all 24 services
        };
        
        foreach (var serviceName in services)
        {
            var client = _appHost.CreateHttpClient(serviceName);
            var healthResponse = await client.GetAsync("/health");
            healthResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
