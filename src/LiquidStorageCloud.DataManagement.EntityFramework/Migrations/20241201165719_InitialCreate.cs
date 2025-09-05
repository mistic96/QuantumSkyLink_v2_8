using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Downstream.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Namespace = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolidState = table.Column<bool>(type: "bit", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EntityId",
                table: "Messages",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EntityType",
                table: "Messages",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EntityType_EntityId",
                table: "Messages",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EntityType_SolidState",
                table: "Messages",
                columns: new[] { "EntityType", "SolidState" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SolidState",
                table: "Messages",
                column: "SolidState");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
