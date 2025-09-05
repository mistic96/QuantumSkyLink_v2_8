using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "MfaTokens",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeliveryMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryTarget = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MfaTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiSignatureRequests",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OperationData = table.Column<string>(type: "jsonb", nullable: false),
                    RequiredSignatures = table.Column<int>(type: "integer", nullable: false),
                    CurrentSignatures = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiSignatureRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventData = table.Column<string>(type: "jsonb", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequiresInvestigation = table.Column<bool>(type: "boolean", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityPolicies",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Configuration = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiSignatureApprovals",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SignatureData = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SignatureMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiSignatureApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultiSignatureApprovals_MultiSignatureRequests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "security",
                        principalTable: "MultiSignatureRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MfaTokens_TokenHash",
                schema: "security",
                table: "MfaTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfaTokens_UserId_ExpiresAt",
                schema: "security",
                table: "MfaTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MfaTokens_UserId_TokenType_IsUsed",
                schema: "security",
                table: "MfaTokens",
                columns: new[] { "UserId", "TokenType", "IsUsed" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureApprovals_ApprovedBy_CreatedAt",
                schema: "security",
                table: "MultiSignatureApprovals",
                columns: new[] { "ApprovedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureApprovals_RequestId_ApprovedBy",
                schema: "security",
                table: "MultiSignatureApprovals",
                columns: new[] { "RequestId", "ApprovedBy" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureApprovals_RequestId_Status",
                schema: "security",
                table: "MultiSignatureApprovals",
                columns: new[] { "RequestId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureRequests_AccountId_Status",
                schema: "security",
                table: "MultiSignatureRequests",
                columns: new[] { "AccountId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureRequests_CreatedAt",
                schema: "security",
                table: "MultiSignatureRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureRequests_OperationType_Status",
                schema: "security",
                table: "MultiSignatureRequests",
                columns: new[] { "OperationType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MultiSignatureRequests_RequestedBy_Status",
                schema: "security",
                table: "MultiSignatureRequests",
                columns: new[] { "RequestedBy", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_CorrelationId",
                schema: "security",
                table: "SecurityEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_EventType_Severity_Timestamp",
                schema: "security",
                table: "SecurityEvents",
                columns: new[] { "EventType", "Severity", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_Timestamp",
                schema: "security",
                table: "SecurityEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId_EventType_Timestamp",
                schema: "security",
                table: "SecurityEvents",
                columns: new[] { "UserId", "EventType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicies_UserId_IsActive",
                schema: "security",
                table: "SecurityPolicies",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicies_UserId_PolicyType",
                schema: "security",
                table: "SecurityPolicies",
                columns: new[] { "UserId", "PolicyType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MfaTokens",
                schema: "security");

            migrationBuilder.DropTable(
                name: "MultiSignatureApprovals",
                schema: "security");

            migrationBuilder.DropTable(
                name: "SecurityEvents",
                schema: "security");

            migrationBuilder.DropTable(
                name: "SecurityPolicies",
                schema: "security");

            migrationBuilder.DropTable(
                name: "MultiSignatureRequests",
                schema: "security");
        }
    }
}
