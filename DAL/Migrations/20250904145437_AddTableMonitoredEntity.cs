using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMonitoredEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Users_UserId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_UserId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Cameras");

            migrationBuilder.AddColumn<int>(
                name: "MonitoredEntityId",
                table: "Cameras",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MonitoredEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoredEntity_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Security",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_MonitoredEntityId",
                table: "Cameras",
                column: "MonitoredEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredEntity_UserId",
                table: "MonitoredEntity",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_MonitoredEntity_MonitoredEntityId",
                table: "Cameras",
                column: "MonitoredEntityId",
                principalTable: "MonitoredEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_MonitoredEntity_MonitoredEntityId",
                table: "Cameras");

            migrationBuilder.DropTable(
                name: "MonitoredEntity");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_MonitoredEntityId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "MonitoredEntityId",
                table: "Cameras");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Cameras",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_UserId",
                table: "Cameras",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Users_UserId",
                table: "Cameras",
                column: "UserId",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
