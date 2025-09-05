using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TokenService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalSupply = table.Column<decimal>(type: "numeric(36,0)", nullable: false),
                    Decimals = table.Column<int>(type: "integer", nullable: false),
                    TokenType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantumLedgerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantumLedgerExternalOwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QuantumLedgerSubstitutionKeyId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssetMetadata = table.Column<string>(type: "jsonb", nullable: true),
                    CrossChainEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Network = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MultiChainAssetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(36,18)", nullable: false),
                    LockedBalance = table.Column<decimal>(type: "numeric(36,18)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedWithQuantumLedger = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastQuantumLedgerTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenBalances_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TokenSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenPurpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UseCase = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "jsonb", nullable: false),
                    AiComplianceScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewComments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AiRecommendations = table.Column<string>(type: "jsonb", nullable: true),
                    AiRedFlags = table.Column<string>(type: "jsonb", nullable: true),
                    AssetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssetVerificationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssetVerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AssetVerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenSubmissions_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TokenTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(36,18)", nullable: false),
                    QuantumLedgerTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalTransactionHash = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: true),
                    MultiChainTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TransactionFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    FeeCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    GasUsed = table.Column<decimal>(type: "numeric(18,0)", nullable: true),
                    GasPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenTransfers_Tokens_TokenId",
                        column: x => x.TokenId,
                        principalTable: "Tokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokenBalances_AccountId",
                table: "TokenBalances",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenBalances_TokenId",
                table: "TokenBalances",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenBalances_TokenId_AccountId",
                table: "TokenBalances",
                columns: new[] { "TokenId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ApprovalStatus",
                table: "Tokens",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_CreatorId_Status",
                table: "Tokens",
                columns: new[] { "CreatorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_QuantumLedgerAccountId",
                table: "Tokens",
                column: "QuantumLedgerAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_Symbol_Network",
                table: "Tokens",
                columns: new[] { "Symbol", "Network" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenSubmissions_AiComplianceScore",
                table: "TokenSubmissions",
                column: "AiComplianceScore");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSubmissions_ApprovalStatus",
                table: "TokenSubmissions",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSubmissions_CreatorId_SubmissionDate",
                table: "TokenSubmissions",
                columns: new[] { "CreatorId", "SubmissionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenSubmissions_TokenId",
                table: "TokenSubmissions",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenTransfers_FromAccountId_Status",
                table: "TokenTransfers",
                columns: new[] { "FromAccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenTransfers_QuantumLedgerTransactionId",
                table: "TokenTransfers",
                column: "QuantumLedgerTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokenTransfers_ToAccountId_Status",
                table: "TokenTransfers",
                columns: new[] { "ToAccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TokenTransfers_TokenId_CreatedAt",
                table: "TokenTransfers",
                columns: new[] { "TokenId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenBalances");

            migrationBuilder.DropTable(
                name: "TokenSubmissions");

            migrationBuilder.DropTable(
                name: "TokenTransfers");

            migrationBuilder.DropTable(
                name: "Tokens");
        }
    }
}
