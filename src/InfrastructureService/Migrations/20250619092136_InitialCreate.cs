using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: false),
                    WalletType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Network = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    LockedBalance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    EncryptedPrivateKey = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PublicKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DerivationPath = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RequiredSignatures = table.Column<int>(type: "integer", nullable: false),
                    TotalSigners = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastTransactionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Hash = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: true),
                    FromAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: false),
                    ToAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    TokenSymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TokenAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: true),
                    GasPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    GasLimit = table.Column<long>(type: "bigint", nullable: false),
                    GasUsed = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Network = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BlockNumber = table.Column<long>(type: "bigint", nullable: true),
                    TransactionIndex = table.Column<int>(type: "integer", nullable: true),
                    Nonce = table.Column<long>(type: "bigint", nullable: false),
                    Data = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiredSignatures = table.Column<int>(type: "integer", nullable: false),
                    CurrentSignatures = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BroadcastAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenSymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TokenAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: true),
                    TokenName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TokenDecimals = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    LockedBalance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    AvailableBalance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    UsdValue = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    TokenPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletBalances_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletSigners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SigningWeight = table.Column<int>(type: "integer", nullable: false),
                    PublicKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Permissions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletSigners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletSigners_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerAddress = table.Column<string>(type: "character varying(42)", maxLength: 42, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Signature = table.Column<string>(type: "character varying(132)", maxLength: 132, nullable: true),
                    R = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: true),
                    S = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: true),
                    V = table.Column<int>(type: "integer", nullable: true),
                    SignatureData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionSignatures_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionSignatures_WalletSigners_SignerId",
                        column: x => x.SignerId,
                        principalTable: "WalletSigners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FromAddress",
                table: "Transactions",
                column: "FromAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Hash",
                table: "Transactions",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ToAddress",
                table: "Transactions",
                column: "ToAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId_Status",
                table: "Transactions",
                columns: new[] { "WalletId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSignatures_SignerId",
                table: "TransactionSignatures",
                column: "SignerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSignatures_Status",
                table: "TransactionSignatures",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSignatures_TransactionId_SignerId",
                table: "TransactionSignatures",
                columns: new[] { "TransactionId", "SignerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_TokenAddress",
                table: "WalletBalances",
                column: "TokenAddress");

            migrationBuilder.CreateIndex(
                name: "IX_WalletBalances_WalletId_TokenSymbol",
                table: "WalletBalances",
                columns: new[] { "WalletId", "TokenSymbol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Address",
                table: "Wallets",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_Status",
                table: "Wallets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId_WalletType",
                table: "Wallets",
                columns: new[] { "UserId", "WalletType" });

            migrationBuilder.CreateIndex(
                name: "IX_WalletSigners_Status",
                table: "WalletSigners",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletSigners_WalletId_UserId",
                table: "WalletSigners",
                columns: new[] { "WalletId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionSignatures");

            migrationBuilder.DropTable(
                name: "WalletBalances");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "WalletSigners");

            migrationBuilder.DropTable(
                name: "Wallets");
        }
    }
}
