using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_CCP.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCodeToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adăugare coloana UniqueCode
            migrationBuilder.AddColumn<string>(
                name: "UniqueCode",
                table: "AspNetUsers",
                type: "nvarchar(50)", // Lungime maximă pentru cod
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Populare coloana UniqueCode pentru utilizatorii existenți
            migrationBuilder.Sql(@"
            UPDATE AspNetUsers
            SET UniqueCode = 'A' + RIGHT('00000' + CAST(Id AS NVARCHAR), 5)
            WHERE UniqueCode IS NULL OR UniqueCode = ''
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ștergere coloana UniqueCode
            migrationBuilder.DropColumn(
                name: "UniqueCode",
                table: "AspNetUsers");
        }
    }
}