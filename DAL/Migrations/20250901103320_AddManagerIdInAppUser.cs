using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerIdInAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerId",
                schema: "Security",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                schema: "Security",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerId",
                schema: "Security",
                table: "Users",
                column: "ManagerId",
                principalSchema: "Security",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerId",
                schema: "Security",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerId",
                schema: "Security",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                schema: "Security",
                table: "Users");
        }
    }
}
