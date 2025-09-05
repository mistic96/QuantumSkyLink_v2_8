using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiquidationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetEligibilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumLiquidationAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MaximumLiquidationAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    DailyLiquidationLimit = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MonthlyLiquidationLimit = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    LockupPeriodDays = table.Column<int>(type: "integer", nullable: true),
                    MinimumHoldingPeriodDays = table.Column<int>(type: "integer", nullable: true),
                    CoolingOffPeriodHours = table.Column<int>(type: "integer", nullable: true),
                    RequiresKyc = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresEnhancedDueDiligence = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresMultiSignature = table.Column<bool>(type: "boolean", nullable: false),
                    MultiSignatureThreshold = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    SupportedOutputCurrencies = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RestrictedCountries = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AllowedCountries = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeePercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    FixedFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    EstimatedProcessingTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxProcessingTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    RequiresSpecialHandling = table.Column<bool>(type: "boolean", nullable: false),
                    SpecialHandlingInstructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ComplianceNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RegulatoryClassification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsSecurityToken = table.Column<bool>(type: "boolean", nullable: true),
                    IsStablecoin = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivacyCoin = table.Column<bool>(type: "boolean", nullable: false),
                    BlockchainNetwork = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: true),
                    FirstEligibleAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextReviewDue = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewFrequencyDays = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StatusChangedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetEligibilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiquidityProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    KycVerified = table.Column<bool>(type: "boolean", nullable: false),
                    KycCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReserveVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ReserveVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinimumTransactionAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MaximumTransactionAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    SupportedAssets = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SupportedOutputCurrencies = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeePercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    LiquidityPoolAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AvailableLiquidity = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    TotalLiquidityProvided = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    TotalFeesEarned = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    SuccessfulLiquidations = table.Column<int>(type: "integer", nullable: false),
                    FailedLiquidations = table.Column<int>(type: "integer", nullable: false),
                    AverageResponseTimeMinutes = table.Column<double>(type: "double precision", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    OperatingHours = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    OutputType = table.Column<int>(type: "integer", nullable: false),
                    OutputSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DestinationType = table.Column<int>(type: "integer", nullable: false),
                    DestinationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DestinationDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MarketPriceAtRequest = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    EstimatedOutputAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ActualOutputAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Fees = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    LiquidityProviderId = table.Column<Guid>(type: "uuid", nullable: true),
                    KycVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ComplianceApproved = table.Column<bool>(type: "boolean", nullable: false),
                    AssetEligibilityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    RequiresMultiSignature = table.Column<bool>(type: "boolean", nullable: false),
                    MultiSignatureApproved = table.Column<bool>(type: "boolean", nullable: false),
                    TransactionHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidationRequests_LiquidityProviders_LiquidityProviderId",
                        column: x => x.LiquidityProviderId,
                        principalTable: "LiquidityProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiquidationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckType = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    ExternalReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RiskScore = table.Column<int>(type: "integer", nullable: true),
                    RiskLevel = table.Column<int>(type: "integer", nullable: true),
                    CheckDetails = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequiresManualReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewComments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsOverridden = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalResult = table.Column<int>(type: "integer", nullable: true),
                    OverrideReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: true),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceChecks_LiquidationRequests_LiquidationRequestId",
                        column: x => x.LiquidationRequestId,
                        principalTable: "LiquidationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidationTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiquidationRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    LiquidityProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssetSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    OutputSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OutputAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    MarketPriceAtExecution = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    SlippagePercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    ProviderFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    PlatformFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    NetworkFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    TotalFees = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    NetAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    TransactionHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: true),
                    Confirmations = table.Column<int>(type: "integer", nullable: true),
                    SmartContractAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentGateway = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EstimatedExecutionTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    ActualExecutionTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    ExecutionStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsReversible = table.Column<bool>(type: "boolean", nullable: false),
                    ReversibleUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReversalTransactionHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReversalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidationTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidationTransactions_LiquidationRequests_LiquidationRequ~",
                        column: x => x.LiquidationRequestId,
                        principalTable: "LiquidationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LiquidationTransactions_LiquidityProviders_LiquidityProvide~",
                        column: x => x.LiquidityProviderId,
                        principalTable: "LiquidityProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketPriceSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LiquidationRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OutputSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    BidPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    AskPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Spread = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Volume24h = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Change24hPercent = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    High24h = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Low24h = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MarketCap = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AvailableLiquidity = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    PriceSource = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TradingPair = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConfidenceLevel = table.Column<int>(type: "integer", nullable: true),
                    IsSuitableForLiquidation = table.Column<bool>(type: "boolean", nullable: false),
                    UnsuitabilityReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstimatedSlippage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    MinTransactionSize = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MaxTransactionSize = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    SourceTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LatencyMs = table.Column<int>(type: "integer", nullable: true),
                    IsRealTime = table.Column<bool>(type: "boolean", nullable: false),
                    DelaySeconds = table.Column<int>(type: "integer", nullable: true),
                    ValidityMinutes = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsedForLiquidation = table.Column<bool>(type: "boolean", nullable: false),
                    UsedForLiquidationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPriceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketPriceSnapshots_LiquidationRequests_LiquidationRequest~",
                        column: x => x.LiquidationRequestId,
                        principalTable: "LiquidationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetEligibilities_AssetSymbol",
                table: "AssetEligibilities",
                column: "AssetSymbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetEligibilities_CreatedAt",
                table: "AssetEligibilities",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AssetEligibilities_Status",
                table: "AssetEligibilities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_CheckType",
                table: "ComplianceChecks",
                column: "CheckType");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_CreatedAt",
                table: "ComplianceChecks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_LiquidationRequestId",
                table: "ComplianceChecks",
                column: "LiquidationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecks_Result",
                table: "ComplianceChecks",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationRequests_AssetSymbol",
                table: "LiquidationRequests",
                column: "AssetSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationRequests_CreatedAt",
                table: "LiquidationRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationRequests_LiquidityProviderId",
                table: "LiquidationRequests",
                column: "LiquidityProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationRequests_Status",
                table: "LiquidationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationRequests_UserId",
                table: "LiquidationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTransactions_CreatedAt",
                table: "LiquidationTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTransactions_LiquidationRequestId",
                table: "LiquidationTransactions",
                column: "LiquidationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTransactions_LiquidityProviderId",
                table: "LiquidationTransactions",
                column: "LiquidityProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTransactions_Status",
                table: "LiquidationTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidationTransactions_TransactionHash",
                table: "LiquidationTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityProviders_CreatedAt",
                table: "LiquidityProviders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityProviders_Status",
                table: "LiquidityProviders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityProviders_UserId",
                table: "LiquidityProviders",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceSnapshots_AssetSymbol",
                table: "MarketPriceSnapshots",
                column: "AssetSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceSnapshots_CreatedAt",
                table: "MarketPriceSnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceSnapshots_LiquidationRequestId",
                table: "MarketPriceSnapshots",
                column: "LiquidationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceSnapshots_OutputSymbol",
                table: "MarketPriceSnapshots",
                column: "OutputSymbol");

            migrationBuilder.CreateIndex(
                name: "IX_MarketPriceSnapshots_PriceSource",
                table: "MarketPriceSnapshots",
                column: "PriceSource");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetEligibilities");

            migrationBuilder.DropTable(
                name: "ComplianceChecks");

            migrationBuilder.DropTable(
                name: "LiquidationTransactions");

            migrationBuilder.DropTable(
                name: "MarketPriceSnapshots");

            migrationBuilder.DropTable(
                name: "LiquidationRequests");

            migrationBuilder.DropTable(
                name: "LiquidityProviders");
        }
    }
}
