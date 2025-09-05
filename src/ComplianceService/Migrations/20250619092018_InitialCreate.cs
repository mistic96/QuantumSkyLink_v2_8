using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComplianceService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "case_number_seq");

            migrationBuilder.CreateTable(
                name: "KycVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplyCubeClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ComplyCubeCheckId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    KycLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggerReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ComplyCubeResult = table.Column<string>(type: "text", nullable: true),
                    RiskScore = table.Column<decimal>(type: "numeric(3,2)", precision: 5, scale: 2, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceCases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KycVerificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValueSql: "'CASE-' || EXTRACT(YEAR FROM NOW()) || '-' || LPAD(NEXTVAL('case_number_seq')::TEXT, 6, '0')"),
                    CaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Resolution = table.Column<string>(type: "text", nullable: true),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresComplianceOfficerReview = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresAIReview = table.Column<bool>(type: "boolean", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceCases_KycVerifications_KycVerificationId",
                        column: x => x.KycVerificationId,
                        principalTable: "KycVerifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CaseDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileData = table.Column<byte[]>(type: "bytea", nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProcessingResult = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseDocuments_ComplianceCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewResult = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DetailedAnalysis = table.Column<string>(type: "text", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(3,2)", precision: 5, scale: 2, nullable: true),
                    RecommendedAction = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NextReviewBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AIAnalysisData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseReviews_ComplianceCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KycVerificationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiresInvestigation = table.Column<bool>(type: "boolean", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceEvents_ComplianceCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "ComplianceCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceEvents_KycVerifications_KycVerificationId",
                        column: x => x.KycVerificationId,
                        principalTable: "KycVerifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_CaseId_DocumentType",
                table: "CaseDocuments",
                columns: new[] { "CaseId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_UploadedAt",
                table: "CaseDocuments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_CaseId_ReviewType",
                table: "CaseReviews",
                columns: new[] { "CaseId", "ReviewType" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_ReviewedAt",
                table: "CaseReviews",
                column: "ReviewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_ReviewedBy",
                table: "CaseReviews",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCases_CaseNumber",
                table: "ComplianceCases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCases_KycVerificationId",
                table: "ComplianceCases",
                column: "KycVerificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCases_Priority_Status",
                table: "ComplianceCases",
                columns: new[] { "Priority", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceCases_UserId_Status",
                table: "ComplianceCases",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_CaseId",
                table: "ComplianceEvents",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_CorrelationId",
                table: "ComplianceEvents",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_KycVerificationId",
                table: "ComplianceEvents",
                column: "KycVerificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_Severity",
                table: "ComplianceEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_Timestamp",
                table: "ComplianceEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceEvents_UserId_EventType",
                table: "ComplianceEvents",
                columns: new[] { "UserId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_ComplyCubeClientId",
                table: "KycVerifications",
                column: "ComplyCubeClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_CorrelationId",
                table: "KycVerifications",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_KycVerifications_UserId_Status",
                table: "KycVerifications",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseDocuments");

            migrationBuilder.DropTable(
                name: "CaseReviews");

            migrationBuilder.DropTable(
                name: "ComplianceEvents");

            migrationBuilder.DropTable(
                name: "ComplianceCases");

            migrationBuilder.DropTable(
                name: "KycVerifications");

            migrationBuilder.DropSequence(
                name: "case_number_seq");
        }
    }
}
