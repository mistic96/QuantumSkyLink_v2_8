using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovernanceService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GovernanceRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApplicableType = table.Column<int>(type: "integer", nullable: false),
                    MinimumQuorum = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ApprovalThreshold = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    VotingPeriod = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ExecutionDelay = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MinimumTokensRequired = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ProposalDeposit = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    RequiresMultiSig = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredSignatures = table.Column<int>(type: "integer", nullable: true),
                    AllowDelegation = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VotingStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VotingEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuorumPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ApprovalThreshold = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ExecutionParameters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    RequestedCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VotingDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecificType = table.Column<int>(type: "integer", nullable: true),
                    DelegationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedById = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxDelegationPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProposalExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ExecutionParameters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ExecutionResult = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TransactionHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GasUsed = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    ExecutionCost = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalExecutions_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Choice = table.Column<int>(type: "integer", nullable: false),
                    VotingPower = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CastAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDelegated = table.Column<bool>(type: "boolean", nullable: false),
                    DelegatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceRules_ApplicableType_Active_Unique",
                table: "GovernanceRules",
                column: "ApplicableType",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceRules_IsActive",
                table: "GovernanceRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalExecutions_ExecutedAt",
                table: "ProposalExecutions",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalExecutions_ProposalId",
                table: "ProposalExecutions",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalExecutions_Status",
                table: "ProposalExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CreatorId",
                table: "Proposals",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Status",
                table: "Proposals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Type",
                table: "Proposals",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_VotingEndTime",
                table: "Proposals",
                column: "VotingEndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_VotingStartTime",
                table: "Proposals",
                column: "VotingStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_CastAt",
                table: "Votes",
                column: "CastAt");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ProposalId",
                table: "Votes",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_ProposalId_VoterId_Unique",
                table: "Votes",
                columns: new[] { "ProposalId", "VoterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_VoterId",
                table: "Votes",
                column: "VoterId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingDelegations_DelegateId",
                table: "VotingDelegations",
                column: "DelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingDelegations_Delegator_Delegate_Type_Unique",
                table: "VotingDelegations",
                columns: new[] { "DelegatorId", "DelegateId", "SpecificType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotingDelegations_DelegatorId",
                table: "VotingDelegations",
                column: "DelegatorId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingDelegations_IsActive",
                table: "VotingDelegations",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GovernanceRules");

            migrationBuilder.DropTable(
                name: "ProposalExecutions");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "VotingDelegations");

            migrationBuilder.DropTable(
                name: "Proposals");
        }
    }
}
