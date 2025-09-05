using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeeService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DistributionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(8,6)", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Conditions = table.Column<string>(type: "jsonb", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MaximumAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ToCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Bid = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Ask = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Volume24h = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Change24h = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Metadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CalculationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FlatFeeAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    PercentageRate = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    MinimumFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MaximumFee = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TieredStructure = table.Column<string>(type: "jsonb", nullable: true),
                    DiscountRules = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionIds = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SettlementMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SettlementReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeeCalculationResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CalculatedFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    FeeCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "numeric(8,6)", nullable: true),
                    DiscountReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FinalFeeAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    UsedExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CalculationDetails = table.Column<string>(type: "jsonb", nullable: true),
                    AppliedRules = table.Column<string>(type: "jsonb", nullable: true),
                    FeeConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExchangeRateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeCalculationResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeCalculationResults_ExchangeRates_ExchangeRateId",
                        column: x => x.ExchangeRateId,
                        principalTable: "ExchangeRates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeCalculationResults_FeeConfigurations_FeeConfigurationId",
                        column: x => x.FeeConfigurationId,
                        principalTable: "FeeConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ConvertedAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ConvertedCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PaymentReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    FeeConfigurationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationResultId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeTransactions_FeeCalculationResults_CalculationResultId",
                        column: x => x.CalculationResultId,
                        principalTable: "FeeCalculationResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeTransactions_FeeConfigurations_FeeConfigurationId",
                        column: x => x.FeeConfigurationId,
                        principalTable: "FeeConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeDistributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipientId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(8,6)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    FeeTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeDistributions_DistributionRules_DistributionRuleId",
                        column: x => x.DistributionRuleId,
                        principalTable: "DistributionRules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FeeDistributions_FeeTransactions_FeeTransactionId",
                        column: x => x.FeeTransactionId,
                        principalTable: "FeeTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeDistributions_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRules_EffectiveFrom_EffectiveUntil",
                table: "DistributionRules",
                columns: new[] { "EffectiveFrom", "EffectiveUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRules_FeeType_IsActive",
                table: "DistributionRules",
                columns: new[] { "FeeType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionRules_Priority_IsActive",
                table: "DistributionRules",
                columns: new[] { "Priority", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_FromCurrency_ToCurrency_Timestamp",
                table: "ExchangeRates",
                columns: new[] { "FromCurrency", "ToCurrency", "Timestamp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_IsActive",
                table: "ExchangeRates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeRates_Provider_Timestamp",
                table: "ExchangeRates",
                columns: new[] { "Provider", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeCalculationResults_ExchangeRateId",
                table: "FeeCalculationResults",
                column: "ExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeCalculationResults_FeeConfigurationId",
                table: "FeeCalculationResults",
                column: "FeeConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeCalculationResults_FeeType_CreatedAt",
                table: "FeeCalculationResults",
                columns: new[] { "FeeType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeCalculationResults_ReferenceId_ReferenceType",
                table: "FeeCalculationResults",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeCalculationResults_UserId_CreatedAt",
                table: "FeeCalculationResults",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeConfigurations_EffectiveFrom_EffectiveUntil",
                table: "FeeConfigurations",
                columns: new[] { "EffectiveFrom", "EffectiveUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeConfigurations_FeeType_EntityType_IsActive",
                table: "FeeConfigurations",
                columns: new[] { "FeeType", "EntityType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeDistributions_DistributionRuleId",
                table: "FeeDistributions",
                column: "DistributionRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDistributions_FeeTransactionId_RecipientType",
                table: "FeeDistributions",
                columns: new[] { "FeeTransactionId", "RecipientType" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeDistributions_RecipientId_Status",
                table: "FeeDistributions",
                columns: new[] { "RecipientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeDistributions_SettlementId",
                table: "FeeDistributions",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDistributions_Status_CreatedAt",
                table: "FeeDistributions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_CalculationResultId",
                table: "FeeTransactions",
                column: "CalculationResultId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_FeeConfigurationId",
                table: "FeeTransactions",
                column: "FeeConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_ReferenceId_ReferenceType",
                table: "FeeTransactions",
                columns: new[] { "ReferenceId", "ReferenceType" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_Status_CreatedAt",
                table: "FeeTransactions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_TransactionType_CreatedAt",
                table: "FeeTransactions",
                columns: new[] { "TransactionType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FeeTransactions_UserId_Status",
                table: "FeeTransactions",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeeDistributions");

            migrationBuilder.DropTable(
                name: "DistributionRules");

            migrationBuilder.DropTable(
                name: "FeeTransactions");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "FeeCalculationResults");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "FeeConfigurations");
        }
    }
}
