using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_CCP.Migrations
{
    /// <inheritdoc />
    public partial class UsersOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "NewsItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_OwnerId",
                table: "NewsItems",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsItems_AspNetUsers_OwnerId",
                table: "NewsItems",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsItems_AspNetUsers_OwnerId",
                table: "NewsItems");

            migrationBuilder.DropIndex(
                name: "IX_NewsItems_OwnerId",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "NewsItems");
        }
    }
}
