using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_CCP.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPartners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Partners",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_UserId",
                table: "Partners",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Partners_AspNetUsers_UserId",
                table: "Partners",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Partners_AspNetUsers_UserId",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_UserId",
                table: "Partners");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Partners");
        }
    }
}
