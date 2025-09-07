using Aspire.Hosting;
using Amazon;

var builder = DistributedApplication.CreateBuilder(args);

// Configure AWS SDK and register CloudFormation stacks using Aspire.Hosting.AWS
// Uses appsettings.AWS.json values where available (AWS:Profile, AWS:Region, Deployment:Environment)
var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile(builder.Configuration["AWS:Profile"] ?? "default")
    .WithRegion(Amazon.RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"] ?? "us-east-1"));

// Register IAM deployment role stack (creates qsl-deploy-role) via CloudFormation template
var roleStack = builder.AddAWSCloudFormationTemplate("QSLDeployRoleStack", "../infra/aws/qsl-deploy-role.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "qsl")
    .WithReference(awsConfig);

// Register network infrastructure via CloudFormation template
// Creates VPC, subnets, NAT gateway, security groups, and VPC endpoints
var networkStack = builder.AddAWSCloudFormationTemplate("QSLNetworkStack", "../infra/aws/network-stack.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithReference(awsConfig);

// Register secrets manager for secure credential storage
var secretsStack = builder.AddAWSCloudFormationTemplate("QSLSecretsStack", "../infra/aws/secrets-manager.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithReference(awsConfig);

// Register EventBridge messaging infrastructure via CloudFormation template
// Creates 5 event buses (core, financial, blockchain, business, system)
// 8 SQS queues with DLQs for service consumption
// 3 FIFO queues for financial services requiring strict ordering
var eventBridgeStack = builder.AddAWSCloudFormationTemplate("QSLEventBridge", "../infra/aws/aws-eventbridge-messaging.template.json")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithReference(awsConfig);

// Register ECS cluster with Fargate configuration
var ecsClusterStack = builder.AddAWSCloudFormationTemplate("QSLECSCluster", "../infra/aws/ecs-cluster.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithReference(awsConfig);

// Register storage infrastructure (S3 buckets and CloudFront)
var storageStack = builder.AddAWSCloudFormationTemplate("QSLStorageStack", "../infra/aws/storage-stack.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithReference(awsConfig);

// Register Application Load Balancer for API gateways
var albStack = builder.AddAWSCloudFormationTemplate("QSLALBStack", "../infra/aws/alb-stack.template.json")
    .WithParameter("Environment", builder.Configuration["AWS:Deployment:Environment"] ?? "staging")
    .WithParameter("ProjectName", builder.Configuration["AWS:ProjectName"] ?? "quantumskylink-v2")
    .WithParameter("VpcId", builder.Configuration["AWS:Network:VpcId"] ?? "vpc-placeholder")
    .WithParameter("PublicSubnet1Id", builder.Configuration["AWS:Network:PublicSubnet1Id"] ?? "subnet-placeholder-public-1")
    .WithParameter("PublicSubnet2Id", builder.Configuration["AWS:Network:PublicSubnet2Id"] ?? "subnet-placeholder-public-2")
    .WithParameter("ALBSecurityGroupId", builder.Configuration["AWS:Network:ALBSecurityGroupId"] ?? "sg-alb-placeholder")
    .WithReference(awsConfig);

// AWS Integration - Configuration for AWS deployment
// Configuration will be loaded from appsettings.AWS.json when deployed to AWS environment
// AWS-specific connection strings and settings are managed through configuration

// Infrastructure Services
var redis = builder.AddRedis("cache");

// SurrealDB - High-performance document database for hybrid storage and CQRS operations
var surrealdb = builder.AddContainer("surrealdb", "surrealdb/surrealdb")
    .WithArgs("start", "--log", "trace", "--user", "root", "--pass", "surrealpass", "--bind", "0.0.0.0:8000", "memory")
    .WithHttpEndpoint(port: 8000, targetPort: 8000, name: "surrealdb-http")
    .WithEnvironment("SURREAL_USER", "root")
    .WithEnvironment("SURREAL_PASS", "surrealpass")
    .WithEnvironment("SURREAL_LOG", "trace");

var surrealdbConn = builder.AddConnectionString("surrealdb-connection", "http://localhost:8000");

// Neon PostgreSQL Cloud Database - Database-per-Service Architecture
// Each service gets its own dedicated database following quantumskylink_{service} naming convention
var postgresUserService = builder.AddConnectionString("postgres-userservice");
var postgresAccountService = builder.AddConnectionString("postgres-accountservice");
var postgresPaymentGatewayService = builder.AddConnectionString("postgres-paymentgatewayservice");
var postgresTreasuryService = builder.AddConnectionString("postgres-treasuryservice");
var postgresTokenService = builder.AddConnectionString("postgres-tokenservice");
var postgresFeeService = builder.AddConnectionString("postgres-feeservice");
var postgresGovernanceService = builder.AddConnectionString("postgres-governanceservice");
var postgresComplianceService = builder.AddConnectionString("postgres-complianceservice");
var postgresSecurityService = builder.AddConnectionString("postgres-securityservice");
var postgresNotificationService = builder.AddConnectionString("postgres-notificationservice");
var postgresLiquidationService = builder.AddConnectionString("postgres-liquidationservice");
var postgresInfrastructureService = builder.AddConnectionString("postgres-infrastructureservice");
var postgresIdentityVerificationService = builder.AddConnectionString("postgres-identityverificationservice");
var postgresAiReviewService = builder.AddConnectionString("postgres-aireviewservice");
var postgresMarketplaceService = builder.AddConnectionString("postgres-marketplaceservice");
var postgresSignatureService = builder.AddConnectionString("postgres-signatureservice");
var postgresOrchestrationService = builder.AddConnectionString("postgres-orchestrationservice");
var postgresQuantumLedgerHub = builder.AddConnectionString("postgres-quantumledgerhub");

// RabbitMQ replaced with EventBridge for AWS-native event-driven architecture
// Services now use SQS queues for event consumption via EventBridge rules
// var rabbitmq = builder.AddRabbitMQ("messaging"); // DEPRECATED - Replaced by EventBridge

// Database-per-Service Architecture: Each service has complete data isolation
// with dedicated database instances following quantumskylink_{service} naming

// External Services (running via Docker Compose)
var kestraUsername = builder.AddParameter("kestra-username", secret: true);
var kestraPassword = builder.AddParameter("kestra-password", secret: true);

// Reference external Kestra instance running on localhost:8080
var kestra = builder.AddConnectionString("kestra-external", "http://localhost:8080");

// Reference external MultiChain blockchain network running on localhost:7446
var multichain = builder.AddConnectionString("multichain-external", "http://localhost:7446");

// 6-Network Blockchain Configuration for Comprehensive Testing
// Add Alchemy parameter(s) (UAT key stored in AWS Secrets Manager) and keep Infura param as fallback
var alchemyApiKeySepolia = builder.AddParameter("alchemy-api-key-sepolia", secret: true);
var infuraProjectId = builder.AddParameter("infura-project-id", "37e72ed233a54416abc10f8af0243e70");

// Connection strings prefer Alchemy when the parameter is set (builder will resolve the parameter value at runtime)
var ethereumSepolia = builder.AddConnectionString("ethereum-sepolia", $"https://eth-sepolia.g.alchemy.com/v2/{alchemyApiKeySepolia}");
var polygonMumbai = builder.AddConnectionString("polygon-mumbai", $"https://polygon-mumbai.g.alchemy.com/v2/{alchemyApiKeySepolia}");
var arbitrumSepolia = builder.AddConnectionString("arbitrum-sepolia", $"https://arb-sepolia.g.alchemy.com/v2/{alchemyApiKeySepolia}");
var bitcoinTestnet = builder.AddConnectionString("bitcoin-testnet", "https://testnet.blockstream.info/api");
var bscTestnet = builder.AddConnectionString("bsc-testnet", "https://data-seed-prebsc-1-s1.binance.org:8545");

// Core Domain Services
var userService = builder.AddProject<Projects.UserService>("userservice")
    .WithReference(postgresUserService)
    .WithReference(redis);

var accountService = builder.AddProject<Projects.AccountService>("accountservice")
    .WithReference(postgresAccountService)
    .WithReference(redis)
    .WithReference(userService);

var complianceService = builder.AddProject<Projects.ComplianceService>("complianceservice")
    .WithReference(postgresComplianceService)
    .WithReference(eventBridgeStack)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("ComplianceQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

var feeService = builder.AddProject<Projects.FeeService>("feeservice")
    .WithReference(postgresFeeService)
    .WithReference(redis);

var securityService = builder.AddProject<Projects.SecurityService>("securityservice")
    .WithReference(postgresSecurityService)
    .WithReference(redis);

var tokenService = builder.AddProject<Projects.TokenService>("tokenservice")
    .WithReference(postgresTokenService)
    .WithReference(redis)
    .WithReference(securityService);

var governanceService = builder.AddProject<Projects.GovernanceService>("governanceservice")
    .WithReference(postgresGovernanceService)
    .WithReference(eventBridgeStack)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("GovernanceQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

// Financial Services
var treasuryService = builder.AddProject<Projects.TreasuryService>("treasuryservice")
    .WithReference(postgresTreasuryService)
    .WithReference(redis)
    .WithReference(feeService);

var paymentGatewayService = builder.AddProject<Projects.PaymentGatewayService>("paymentgatewayservice")
    .WithReference(postgresPaymentGatewayService)
    .WithReference(eventBridgeStack)
    .WithReference(treasuryService)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("PaymentQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

var liquidationService = builder.AddProject<Projects.LiquidationService>("liquidationservice")
    .WithReference(postgresLiquidationService)
    .WithReference(eventBridgeStack)
    .WithReference(tokenService)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("LiquidationQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

// Infrastructure Services - Enhanced with 6-Network Blockchain Support
var infrastructureService = builder.AddProject<Projects.InfrastructureService>("infrastructureservice")
    .WithReference(postgresInfrastructureService)
    .WithReference(redis)
    .WithReference(multichain)           // Network 1: MultiChain Private
    .WithReference(ethereumSepolia)      // Network 2: Ethereum Sepolia
    .WithReference(polygonMumbai)        // Network 3: Polygon Mumbai
    .WithReference(arbitrumSepolia)      // Network 4: Arbitrum Sepolia
    .WithReference(bitcoinTestnet)       // Network 5: Bitcoin Testnet
    .WithReference(bscTestnet)           // Network 6: BSC Testnet
    .WithEnvironment("INFURA_PROJECT_ID", infuraProjectId);

var identityVerificationService = builder
    .AddProject<Projects.IdentityVerificationService>("identityverificationservice")
    .WithReference(postgresIdentityVerificationService)
    .WithReference(complianceService);

// Supporting Services
var aiReviewService = builder.AddProject<Projects.AIReviewService>("aireviewservice")
    .WithReference(postgresAiReviewService)
    .WithReference(eventBridgeStack)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("AIReviewQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

var notificationService = builder.AddProject<Projects.NotificationService>("notificationservice")
    .WithReference(postgresNotificationService)
    .WithReference(eventBridgeStack)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("NotificationQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

// Zero-Trust Security Services
var signatureService = builder.AddProject<Projects.SignatureService>("signatureservice")
    .WithReference(postgresSignatureService)
    .WithReference(redis)
    .WithEnvironment("SURREALDB_URL", surrealdb.GetEndpoint("surrealdb-http"))
    .WithEnvironment("SURREALDB_NS", "quantumskylink")
    .WithEnvironment("SURREALDB_DB", "signatures")
    .WithEnvironment("SURREALDB_USER", "root")
    .WithEnvironment("SURREALDB_PASS", "surrealpass");

// Workflow Orchestration Service
var orchestrationService = builder.AddProject<Projects.OrchestrationService>("orchestrationservice")
    .WithReference(postgresOrchestrationService)
    .WithReference(redis)
    .WithReference(kestra)
    .WithReference(signatureService)
    .WithReference(paymentGatewayService)
    .WithReference(userService)
    .WithReference(accountService)
    .WithReference(treasuryService)
    .WithReference(notificationService)
    .WithReference(identityVerificationService)
    .WithEnvironment("KESTRA_USERNAME", kestraUsername)
    .WithEnvironment("KESTRA_PASSWORD", kestraPassword);


var marketplaceService = builder.AddProject<Projects.MarketplaceService>("marketplaceservice")
    .WithReference(postgresMarketplaceService)
    .WithReference(redis)
    .WithReference(tokenService)
    .WithReference(paymentGatewayService)
    .WithReference(feeService)
    .WithReference(userService)
    .WithReference(notificationService)
    .WithReference(complianceService);

// Unified Cart Service - High-performance cart management with SurrealDB
var unifiedCartService = builder.AddProject<Projects.UnifiedCartService>("unifiedcartservice")
    .WithReference(surrealdbConn)
    .WithReference(marketplaceService)
    .WithReference(paymentGatewayService)
    .WithReference(eventBridgeStack)
    .WithEnvironment("AWS__EventBridge__Enabled", "true")
    .WithEnvironment("SURREALDB_URL", surrealdb.GetEndpoint("surrealdb-http"))
    .WithEnvironment("SURREALDB_NS", "quantumskylink")
    .WithEnvironment("SURREALDB_DB", "carts");

// API Gateways
var webApiGateway = builder.AddProject<Projects.WebAPIGateway>("webapigateway")
    .WithReference(userService)
    .WithReference(accountService)
    .WithReference(tokenService)
    .WithReference(marketplaceService)
    .WithExternalHttpEndpoints();

var adminApiGateway = builder.AddProject<Projects.AdminAPIGateway>("adminapigateway")
    .WithReference(userService)
    .WithReference(complianceService)
    .WithReference(governanceService)
    .WithReference(treasuryService)
    .WithExternalHttpEndpoints();

var mobileApiGateway = builder.AddProject<Projects.MobileAPIGateway>("mobileapigateway")
    .WithReference(userService)
    .WithReference(accountService)
    .WithReference(tokenService)
    .WithReference(paymentGatewayService)
    .WithReference(unifiedCartService)
    .WithReference(marketplaceService)
    .WithExternalHttpEndpoints();


// Add QuantumLedger.Hub with reference to external MultiChain network
var quantumLedgerHub = builder.AddProject<Projects.QuantumLedger_Hub>("quantumledgerhub")
    .WithReference(postgresQuantumLedgerHub)
    .WithReference(eventBridgeStack)
    .WithReference(signatureService)
    .WithReference(orchestrationService)
    .WithReference(multichain)
    .WithReplicas(3)
    .WithEnvironment("AWS__SQS__QueueUrl", eventBridgeStack.GetOutput("QuantumLedgerQueueUrl"))
    .WithEnvironment("AWS__EventBridge__Enabled", "true");

// Frontend Applications - React-based user interfaces using Vite
// NOTE: Currently commented out due to path issues with npm on Windows
// To run React apps locally:
// 1. Open separate terminals for each app
// 2. Navigate to src/Apps/[AppName]/[folder-name]
// 3. Run: npm install (first time)
// 4. Run: npm run dev
//
// Uncomment below when using Docker deployment or after fixing npm path issues


// BlockchainExplorer - Blockchain data exploration and analytics interface
var blockchainExplorer = builder.AddNpmApp("blockchainexplorer", "../src/Apps/BlockchainExplorer/quantum-ledger-1452996e", "dev")                     //.AddDockerfile("blockchainexplorer", "src/Apps/BlockchainExplorer/quantum-ledger-1452996e")
    .WithHttpEndpoint(9050)
    .WithReference(webApiGateway)
    .WithReference(infrastructureService)
    .WithReference(quantumLedgerHub)
    //.WaitFor(webApiGateway)
    //.WaitFor(infrastructureService)
    .WithEnvironment("REACT_APP_API_URL", webApiGateway.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_INFRASTRUCTURE_URL", infrastructureService.GetEndpoint("http"))
    //.WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();
/*
// LiquidityProvider - Liquidity management and treasury operations interface
var liquidityProvider = builder.AddDockerfile("liquidityprovider", "src/Apps/LiquidityProvider/quantum-flow-ef2039a0")
    .WithReference(webApiGateway)
    .WithReference(treasuryService)
    .WithReference(paymentGatewayService)
    .WithReference(tokenService)
    .WaitFor(webApiGateway)
    .WaitFor(treasuryService)
    .WithEnvironment("REACT_APP_API_URL", webApiGateway.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_TREASURY_URL", treasuryService.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_PAYMENT_URL", paymentGatewayService.GetEndpoint("http"))
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();

// ManagementPortal - Administrative console for system management
var managementPortal = builder.AddDockerfile("managementportal", "src/Apps/ManagementPortal/quantum-sky-link-admin-console-a1e95f2e")
    .WithReference(adminApiGateway)
    .WithReference(complianceService)
    .WithReference(governanceService)
    .WithReference(userService)
    .WithReference(securityService)
    .WaitFor(adminApiGateway)
    .WaitFor(complianceService)
    .WithEnvironment("REACT_APP_API_URL", webApiGateway.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_COMPLIANCE_URL", complianceService.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_GOVERNANCE_URL", governanceService.GetEndpoint("http"))
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();

// TokenPortal - Token creation, minting, and marketplace interface
var tokenPortal = builder.AddDockerfile("tokenportal", "src/Apps/TokenPortal/quantum-mint-dcb09fa1")
    .WithReference(webApiGateway)
    .WithReference(tokenService)
    .WithReference(marketplaceService)
    .WithReference(paymentGatewayService)
    .WaitFor(webApiGateway)
    .WaitFor(tokenService)
    .WithEnvironment("REACT_APP_API_URL", webApiGateway.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_TOKEN_URL", tokenService.GetEndpoint("http"))
    .WithEnvironment("REACT_APP_MARKETPLACE_URL", marketplaceService.GetEndpoint("http"))
    .WithHttpEndpoint(targetPort: 80)
    .WithExternalHttpEndpoints();
*/

builder.Build().Run();
