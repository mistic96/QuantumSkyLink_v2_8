# QuantumSkyLink v2 - Distributed Microservice System

A comprehensive distributed microservice architecture built with .NET 9 and Aspire 9.3.0, implementing a complete blockchain-enabled financial platform with advanced orchestration and observability.

## 🎭 TESTING FRAMEWORK: PLAYWRIGHT ONLY

**⚠️ IMPORTANT**: All testing in QuantumSkyLink v2 MUST use [Playwright](https://playwright.dev/). This is a project-wide standard.

- 📋 **Read**: [TESTING_STANDARDS.md](TESTING_STANDARDS.md) for complete guidelines
- 🚫 **No xUnit/NUnit/MSTest** - Use Playwright exclusively
- 📁 **Location**: All tests in `quantumskylink-playwright-tests/`
- 🤖 **AI Agents**: Must use Playwright for all test creation

```bash
# Quick start testing
cd quantumskylink-playwright-tests
npm install && npx playwright install
npm test
```

## 🏗️ Architecture Overview

QuantumSkyLink v2 is a cloud-native distributed system consisting of **29 total projects** including 17 business services, 3 API gateways, 6 QuantumLedger components, and supporting infrastructure using .NET Aspire orchestration.

### 🎯 Core Components

- **Aspire Orchestration**: Latest .NET Aspire 9.3.0 for service discovery and management
- **17 Business Services**: Domain-driven design with clear separation of concerns
- **3 API Gateways**: Mobile, Web, and Admin interfaces
- **6 QuantumLedger Components**: Blockchain and cryptography integration
- **3 Supporting Components**: Shared libraries and testing infrastructure
- **7 Kestra Workflows**: Production-ready workflow orchestration

## 🚀 Quick Start

### Prerequisites

- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 or VS Code

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd QuantunSkyLink_v2
   ```

2. **Install Aspire templates**
   ```bash
   dotnet new install Aspire.ProjectTemplates::9.3.0
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the Aspire orchestrator**
   ```bash
   dotnet run --project QuantunSkyLink_v2.AppHost
   ```

5. **Access the Aspire Dashboard**
   - Open: `https://localhost:17140`
   - Monitor all services, dependencies, and metrics

## 📋 Service Architecture

### Business Services (17)
- **UserService**: User management and authentication
- **AccountService**: Account operations and management
- **ComplianceService**: KYC/AML compliance workflows
- **FeeService**: Fee calculation and management
- **SecurityService**: Multi-signature and MFA security
- **TokenService**: Token operations and blockchain integration
- **GovernanceService**: Governance and voting mechanisms
- **TreasuryService**: Treasury fund management
- **PaymentGatewayService**: Payment processing integration
- **LiquidationService**: Asset liquidation workflows
- **InfrastructureService**: System infrastructure management
- **IdentityVerificationService**: Identity verification workflows
- **AIReviewService**: AI-powered review and analysis
- **NotificationService**: Multi-channel notifications
- **MarketplaceService**: Marketplace operations
- **SignatureService**: Digital signature validation
- **OrchestrationService**: Workflow orchestration

### API Gateways (3)
- **MobileAPIGateway**: Mobile application gateway
- **WebAPIGateway**: Web application gateway
- **AdminAPIGateway**: Administrative interface gateway

### QuantumLedger Components (6)
- **QuantumLedger.Blockchain**: Blockchain integration
- **QuantumLedger.Cryptography**: Cryptographic operations
- **QuantumLedger.Cryptography.PQC**: Post-quantum cryptography
- **QuantumLedger.Data**: Data layer
- **QuantumLedger.Hub**: SignalR real-time communication
- **QuantumLedger.Models**: Shared models

### Supporting Components (3)
- **QuantumSkyLink.Shared**: Shared utilities
- **RefitClient**: HTTP client library
- **UserService.Tests**: Test suite

## 🛠️ Technology Stack

- **.NET 9.0.301**: Latest .NET framework with performance optimizations
- **Aspire 9.3.0**: Service orchestration and observability
- **PostgreSQL**: Primary database for persistent storage (database-per-service)
- **Redis**: Caching and session management
- **Kestra**: Workflow orchestration engine (7 production workflows)
- **Docker**: Containerization and deployment
- **OpenTelemetry**: Distributed tracing and monitoring

## 📊 Key Features

### 🔧 Infrastructure
- **Aspire Orchestration**: Complete service discovery and management
- **Database-per-Service**: 18 dedicated PostgreSQL databases
- **Workflow Engine**: 7 Kestra workflows for automated processes
- **Health Checks**: Comprehensive health monitoring
- **Distributed Tracing**: End-to-end request tracing
- **Metrics Collection**: Real-time performance metrics

### 🔒 Security
- **Zero-Trust Architecture**: Universal signature validation
- **Multi-Signature Support**: Enhanced transaction security
- **MFA Integration**: Multi-factor authentication
- **JWT Authentication**: Secure API access
- **Role-Based Access**: Granular permission management

### 🌐 Scalability
- **Microservice Architecture**: Independent service scaling
- **API Gateway Pattern**: Centralized mobile/web interfaces
- **Event-Driven**: Real-time updates and notifications
- **Load Balancing**: Intelligent request distribution

### 📱 Mobile-First
- **Dedicated Mobile Gateway**: MobileAPIGateway for mobile apps
- **Offline Support**: Cached operations and queued requests
- **Real-Time Updates**: SignalR integration for live data
- **Biometric Integration**: Secure mobile authentication

## 📁 Project Structure

```
QuantunSkyLink_v2/
├── QuantunSkyLink_v2.AppHost/          # Aspire orchestrator
├── QuantunSkyLink_v2.ServiceDefaults/  # Shared service patterns
├── src/
│   ├── UserService/                    # User management
│   ├── AccountService/                 # Account operations
│   ├── ComplianceService/              # KYC/AML compliance
│   ├── FeeService/                     # Fee management
│   ├── SecurityService/                # Security operations
│   ├── TokenService/                   # Token management
│   ├── GovernanceService/              # Governance workflows
│   ├── TreasuryService/                # Treasury operations
│   ├── PaymentGatewayService/          # Payment processing
│   ├── LiquidationService/             # Asset liquidation
│   ├── InfrastructureService/          # Infrastructure management
│   ├── QuantumLedger.Hub/              # Blockchain integration
│   ├── IdentityVerificationService/    # Identity verification
│   ├── AIReviewService/                # AI-powered analysis
│   ├── NotificationService/            # Notifications
│   ├── MarketplaceService/             # Marketplace operations
│   ├── WebAPIGateway/                  # Web gateway
│   ├── AdminAPIGateway/                # Admin gateway
│   ├── XLiquidusMobileService/         # Mobile gateway
│   ├── RefitClient/                    # HTTP client library
│   └── QuantumSkyLink.Shared/          # Shared components
└── planning/                           # Implementation documentation
```

## 🔄 Development Workflow

### Building
```bash
# Build entire solution
dotnet build

# Build specific service
dotnet build src/UserService
```

### Testing
```bash
# Run Playwright tests (REQUIRED)
cd quantumskylink-playwright-tests
npm test

# Run specific test suites
npm run test:api      # API endpoint tests
npm run test:mobile   # Mobile gateway tests
npm run test:blockchain # Blockchain tests
npm run test:e2e      # End-to-end workflows

# Generate test reports
npm run test:report
```

**Note**: xUnit/NUnit tests are deprecated. Use Playwright exclusively.

### Running Individual Services
```bash
# Run specific service
dotnet run --project src/UserService
```

## 📈 Monitoring & Observability

### Aspire Dashboard
- **Service Status**: Real-time service health monitoring
- **Metrics Visualization**: Performance metrics and trends
- **Distributed Tracing**: Request flow across services
- **Log Aggregation**: Centralized logging and analysis

### Key Metrics
- **Response Times**: Service response performance
- **Error Rates**: Failure tracking and analysis
- **Throughput**: Request volume and capacity
- **Resource Usage**: CPU, memory, and network utilization

## 🚀 Deployment

### Local Development
```bash
# Start all services with Aspire
dotnet run --project QuantunSkyLink_v2.AppHost
```

### Container Deployment
```bash
# Build container images
docker build -t quantumskylink/userservice src/UserService

# Deploy with Docker Compose
docker-compose up -d
```

### Cloud Deployment
- **Azure Container Apps**: Native Aspire deployment
- **Kubernetes**: Helm charts and manifests
- **AWS ECS**: Container orchestration
- **Google Cloud Run**: Serverless containers

## 📚 Documentation

- [Master Implementation Plan](planning/implementation_v2/Master_Implementation_Plan.md)
- [Aspire Integration Strategy](planning/implementation_v2/Aspire_Integration_Strategy.md)
- [API Gateway Implementation](planning/implementation_v2/API_Gateway_Implementation.md)
- [Fail-Fast Architecture](planning/implementation_v2/Fail_Fast_Architecture.md)
- [RefitClient Enhancement](planning/implementation_v2/RefitClient_Library_Enhancement.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Submit a pull request

## 📄 License

This project is proprietary and confidential.

## 🆘 Support

For technical support and questions:
- Create an issue in the repository
- Contact the development team
- Review the documentation in the `planning/` directory

---

**Built with ❤️ using .NET 9 and Aspire 9.3.0**
