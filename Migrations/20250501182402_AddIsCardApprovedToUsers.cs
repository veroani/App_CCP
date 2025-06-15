using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_CCP.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCardApprovedToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCardApprovedByAdmin",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCardApprovedByAdmin",
                table: "AspNetUsers");
        }
    }
}
