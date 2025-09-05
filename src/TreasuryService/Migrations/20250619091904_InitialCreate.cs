using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreasuryService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllocationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RuleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    DefaultPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    FixedAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ThresholdAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TriggerCondition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TriggerValue = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    RecurrencePattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecurrenceInterval = table.Column<int>(type: "integer", nullable: true),
                    ExecutionTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Conditions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Actions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastExecuted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextExecution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionCount = table.Column<int>(type: "integer", nullable: false),
                    MaxExecutions = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AccountType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    ReservedBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    MinimumBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    MaximumBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExternalAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoutingNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SwiftCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    InterestRate = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AllocationPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    AllocationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AllocationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutionDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NextAllocationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundAllocations_AllocationRules_AllocationRuleId",
                        column: x => x.AllocationRuleId,
                        principalTable: "AllocationRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAllocations_TreasuryAccounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundAllocations_TreasuryAccounts_TargetAccountId",
                        column: x => x.TargetAccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReserveRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequirementName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequiredAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    RequiredPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    CurrentReserve = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Shortfall = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Excess = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComplianceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCalculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextCalculation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalculationFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegulatoryReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Authority = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PenaltyRate = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    PenaltyAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    AutoAdjust = table.Column<bool>(type: "boolean", nullable: false),
                    BufferAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    BufferPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    AlertThreshold = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AlertsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastAlertSent = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlertCount = table.Column<int>(type: "integer", nullable: false),
                    CalculationMethod = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ComplianceNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReserveRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReserveRequirements_TreasuryAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BalanceType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    ReservedBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    PendingCredits = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    PendingDebits = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    DayChange = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    DayChangePercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    BalanceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AsOfTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    TransactionCount = table.Column<int>(type: "integer", nullable: false),
                    HighestBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    LowestBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    AverageBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    InterestEarned = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    FeesCharged = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    IsReconciled = table.Column<bool>(type: "boolean", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReconciledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReconciliationNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExternalBalanceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalBalance = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    VarianceAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    VarianceReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangeAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReconciliationReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreasuryBalances_TreasuryAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SettledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CounterpartyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CounterpartyAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    FundAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    OriginalCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ProcessingDetails = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_FundAllocations_FundAllocationId",
                        column: x => x.FundAllocationId,
                        principalTable: "FundAllocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_TreasuryAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_TreasuryAccounts_RelatedAccountId",
                        column: x => x.RelatedAccountId,
                        principalTable: "TreasuryAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TreasuryTransactions_TreasuryTransactions_ParentTransaction~",
                        column: x => x.ParentTransactionId,
                        principalTable: "TreasuryTransactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRules_Priority",
                table: "AllocationRules",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRules_RuleName",
                table: "AllocationRules",
                column: "RuleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRules_RuleType",
                table: "AllocationRules",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationRules_Status",
                table: "AllocationRules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FundAllocations_AllocationDate",
                table: "FundAllocations",
                column: "AllocationDate");

            migrationBuilder.CreateIndex(
                name: "IX_FundAllocations_AllocationRuleId",
                table: "FundAllocations",
                column: "AllocationRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAllocations_SourceAccountId",
                table: "FundAllocations",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundAllocations_Status",
                table: "FundAllocations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FundAllocations_TargetAccountId",
                table: "FundAllocations",
                column: "TargetAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveRequirements_AccountId",
                table: "ReserveRequirements",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveRequirements_Currency",
                table: "ReserveRequirements",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveRequirements_EffectiveDate",
                table: "ReserveRequirements",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveRequirements_RequirementType",
                table: "ReserveRequirements",
                column: "RequirementType");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveRequirements_Status",
                table: "ReserveRequirements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAccounts_AccountNumber",
                table: "TreasuryAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAccounts_AccountType",
                table: "TreasuryAccounts",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAccounts_Currency",
                table: "TreasuryAccounts",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAccounts_Status",
                table: "TreasuryAccounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryBalances_AccountId",
                table: "TreasuryBalances",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryBalances_BalanceDate",
                table: "TreasuryBalances",
                column: "BalanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryBalances_BalanceType",
                table: "TreasuryBalances",
                column: "BalanceType");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryBalances_Currency",
                table: "TreasuryBalances",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_AccountId",
                table: "TreasuryTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_FundAllocationId",
                table: "TreasuryTransactions",
                column: "FundAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_ParentTransactionId",
                table: "TreasuryTransactions",
                column: "ParentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Reference",
                table: "TreasuryTransactions",
                column: "Reference");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_RelatedAccountId",
                table: "TreasuryTransactions",
                column: "RelatedAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Status",
                table: "TreasuryTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionDate",
                table: "TreasuryTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionNumber",
                table: "TreasuryTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionType",
                table: "TreasuryTransactions",
                column: "TransactionType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReserveRequirements");

            migrationBuilder.DropTable(
                name: "TreasuryBalances");

            migrationBuilder.DropTable(
                name: "TreasuryTransactions");

            migrationBuilder.DropTable(
                name: "FundAllocations");

            migrationBuilder.DropTable(
                name: "AllocationRules");

            migrationBuilder.DropTable(
                name: "TreasuryAccounts");
        }
    }
}
