using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketplaceService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetSymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    MarketType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PricingStrategy = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TotalQuantity = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    RemainingQuantity = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    MinimumPurchaseQuantity = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    MaximumPurchaseQuantity = table.Column<decimal>(type: "numeric(28,8)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PricingConfiguration = table.Column<string>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    ListingFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false),
                    OrderCount = table.Column<int>(type: "integer", nullable: false),
                    VolumeSold = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactInfo = table.Column<string>(type: "jsonb", nullable: true),
                    DocumentationUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RoadmapUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WhitepaperUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SocialLinks = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketListings", x => x.Id);
                    table.CheckConstraint("CK_MarketListing_BasePrice_Positive", "\"BasePrice\" IS NULL OR \"BasePrice\" > 0");
                    table.CheckConstraint("CK_MarketListing_CommissionPercentage_Range", "\"CommissionPercentage\" >= 0 AND \"CommissionPercentage\" <= 1");
                    table.CheckConstraint("CK_MarketListing_MinimumPurchaseQuantity_Positive", "\"MinimumPurchaseQuantity\" > 0");
                    table.CheckConstraint("CK_MarketListing_PlatformToken_TokenId", "(\"AssetType\" = 1 AND \"TokenId\" IS NOT NULL) OR (\"AssetType\" != 1 AND \"AssetSymbol\" IS NOT NULL)");
                    table.CheckConstraint("CK_MarketListing_RemainingQuantity_LessOrEqual_Total", "\"RemainingQuantity\" <= \"TotalQuantity\"");
                    table.CheckConstraint("CK_MarketListing_RemainingQuantity_NonNegative", "\"RemainingQuantity\" >= 0");
                    table.CheckConstraint("CK_MarketListing_TotalQuantity_Positive", "\"TotalQuantity\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    PricePerToken = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    TransactionFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    TotalFees = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    FinalAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    EscrowAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BlockchainTransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    PricingDetails = table.Column<string>(type: "jsonb", nullable: false),
                    BuyerNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SellerNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedCompletionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "integer", nullable: false),
                    RequiresManualReview = table.Column<bool>(type: "boolean", nullable: false),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceOrders", x => x.Id);
                    table.CheckConstraint("CK_MarketplaceOrder_FinalAmount_Positive", "\"FinalAmount\" > 0");
                    table.CheckConstraint("CK_MarketplaceOrder_MaxRetryAttempts_Positive", "\"MaxRetryAttempts\" > 0");
                    table.CheckConstraint("CK_MarketplaceOrder_PricePerToken_Positive", "\"PricePerToken\" > 0");
                    table.CheckConstraint("CK_MarketplaceOrder_Quantity_Positive", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_MarketplaceOrder_RetryCount_NonNegative", "\"RetryCount\" >= 0");
                    table.CheckConstraint("CK_MarketplaceOrder_TotalAmount_Positive", "\"TotalAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_MarketplaceOrders_MarketListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PricingStrategy = table.Column<int>(type: "integer", nullable: false),
                    PricePerToken = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MarketPrice = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MarginAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    MarginPercentage = table.Column<decimal>(type: "numeric(5,4)", nullable: true),
                    PricingConfiguration = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAutomaticUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TradingVolume = table.Column<decimal>(type: "numeric(28,8)", nullable: true),
                    ActiveOrders = table.Column<int>(type: "integer", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                    table.CheckConstraint("CK_PriceHistory_MarginPercentage_Range", "\"MarginPercentage\" IS NULL OR (\"MarginPercentage\" >= -1 AND \"MarginPercentage\" <= 10)");
                    table.CheckConstraint("CK_PriceHistory_MarketPrice_Positive", "\"MarketPrice\" IS NULL OR \"MarketPrice\" > 0");
                    table.CheckConstraint("CK_PriceHistory_PricePerToken_Positive", "\"PricePerToken\" > 0");
                    table.ForeignKey(
                        name: "FK_PriceHistory_MarketListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscrowAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EscrowAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TokenQuantity = table.Column<decimal>(type: "numeric(28,8)", nullable: false),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetSymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AssetType = table.Column<int>(type: "integer", nullable: false),
                    WalletAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LockTransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReleaseTransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EscrowConditions = table.Column<string>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    EscrowFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    DisputeFee = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    TotalFees = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    NetReleaseAmount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDisputed = table.Column<bool>(type: "boolean", nullable: false),
                    DisputeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisputeInitiatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DisputeInitiatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisputeResolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisputeResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DisputeResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AutoReleaseEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AutoReleaseDelayHours = table.Column<int>(type: "integer", nullable: false),
                    AutoReleaseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequiresManualApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReleaseAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxReleaseAttempts = table.Column<int>(type: "integer", nullable: false),
                    LastReleaseError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowAccounts", x => x.Id);
                    table.CheckConstraint("CK_EscrowAccount_AutoReleaseDelayHours_Positive", "\"AutoReleaseDelayHours\" > 0");
                    table.CheckConstraint("CK_EscrowAccount_EscrowAmount_Positive", "\"EscrowAmount\" > 0");
                    table.CheckConstraint("CK_EscrowAccount_MaxReleaseAttempts_Positive", "\"MaxReleaseAttempts\" > 0");
                    table.CheckConstraint("CK_EscrowAccount_PlatformToken_TokenId", "(\"AssetType\" = 1 AND \"TokenId\" IS NOT NULL) OR (\"AssetType\" != 1 AND \"AssetSymbol\" IS NOT NULL)");
                    table.CheckConstraint("CK_EscrowAccount_ReleaseAttempts_NonNegative", "\"ReleaseAttempts\" >= 0");
                    table.CheckConstraint("CK_EscrowAccount_TokenQuantity_Positive", "\"TokenQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_EscrowAccounts_MarketplaceOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "MarketplaceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsSystemAction = table.Column<bool>(type: "boolean", nullable: false),
                    SystemComponent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActionDetails = table.Column<string>(type: "jsonb", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderHistory_MarketplaceOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "MarketplaceOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EscrowHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EscrowAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsSystemAction = table.Column<bool>(type: "boolean", nullable: false),
                    SystemComponent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ActionDetails = table.Column<string>(type: "jsonb", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowHistory", x => x.Id);
                    table.CheckConstraint("CK_EscrowHistory_Amount_Positive", "\"Amount\" IS NULL OR \"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_EscrowHistory_EscrowAccounts_EscrowAccountId",
                        column: x => x.EscrowAccountId,
                        principalTable: "EscrowAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_BuyerId",
                table: "EscrowAccounts",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_CreatedAt",
                table: "EscrowAccounts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_ExpiresAt",
                table: "EscrowAccounts",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_OrderId",
                table: "EscrowAccounts",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_SellerId",
                table: "EscrowAccounts",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowAccounts_Status",
                table: "EscrowAccounts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHistory_CreatedAt",
                table: "EscrowHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHistory_EscrowAccountId",
                table: "EscrowHistory",
                column: "EscrowAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowHistory_Status",
                table: "EscrowHistory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_AssetType",
                table: "MarketListings",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_CreatedAt",
                table: "MarketListings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_ExpiresAt",
                table: "MarketListings",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_MarketType",
                table: "MarketListings",
                column: "MarketType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_PricingStrategy",
                table: "MarketListings",
                column: "PricingStrategy");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_SellerId",
                table: "MarketListings",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_Status",
                table: "MarketListings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_TokenId",
                table: "MarketListings",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_BuyerId",
                table: "MarketplaceOrders",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_CompletedAt",
                table: "MarketplaceOrders",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_CreatedAt",
                table: "MarketplaceOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_ListingId",
                table: "MarketplaceOrders",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_SellerId",
                table: "MarketplaceOrders",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_Status",
                table: "MarketplaceOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOrders_TransactionType",
                table: "MarketplaceOrders",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_CreatedAt",
                table: "OrderHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_OrderId",
                table: "OrderHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_Status",
                table: "OrderHistory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_CreatedAt",
                table: "PriceHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_ListingId",
                table: "PriceHistory",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_PricingStrategy",
                table: "PriceHistory",
                column: "PricingStrategy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EscrowHistory");

            migrationBuilder.DropTable(
                name: "OrderHistory");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "EscrowAccounts");

            migrationBuilder.DropTable(
                name: "MarketplaceOrders");

            migrationBuilder.DropTable(
                name: "MarketListings");
        }
    }
}
