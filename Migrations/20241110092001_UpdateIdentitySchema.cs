using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App_CCP.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentitySchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Creeaza rolurile Admin si User in tabela AspNetRoles
            migrationBuilder.Sql("INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (NEWID(), 'Admin', 'ADMIN')");
            migrationBuilder.Sql("INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (NEWID(), 'User', 'USER')");

            // Creeaza utilizatorul Admin
            string adminUserId = Guid.NewGuid().ToString();
            string adminEmail = "admin@example.com";
            string adminPasswordHash = "Parola00*"; // Se poate genera hash-ul parolei, folosind UserManager 

            migrationBuilder.Sql($@"
                INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, SecurityStamp, ConcurrencyStamp)
                VALUES ('{adminUserId}', '{adminEmail}', '{adminEmail.ToUpper()}', '{adminEmail}', '{adminEmail.ToUpper()}', '{adminPasswordHash}', 'someSecurityStamp', 'someConcurrencyStamp');
            ");

            // Obtine ID-ul rolului "Admin"
            string adminRoleId = "SELECT Id FROM AspNetRoles WHERE Name = 'Admin'";

            // Leaga utilizatorul Admin de rolul Admin
            migrationBuilder.Sql($@"
                INSERT INTO AspNetUserRoles (UserId, RoleId) 
                VALUES ('{adminUserId}', ({adminRoleId}));
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sterge utilizatorul Admin din AspNetUsers
            migrationBuilder.Sql("DELETE FROM AspNetUsers WHERE UserName = 'admin@example.com'");

            // Sterge rolurile din AspNetRoles
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Name = 'Admin'");
            migrationBuilder.Sql("DELETE FROM AspNetRoles WHERE Name = 'User'");

            // sterge legatura dintre utilizator si rol in AspNetUserRoles
            migrationBuilder.Sql("DELETE FROM AspNetUserRoles WHERE UserId = (SELECT Id FROM AspNetUsers WHERE UserName = 'admin@example.com')");
        }
    }
}