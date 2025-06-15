using App_CCP.Models;
using Microsoft.AspNetCore.Identity;

namespace App_CCP.Data
{
    public static class DatabaseSeeder
    {

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<Users>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                // Creare roluri
                string[] roles = { "Admin", "User", "Partner" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                        if (!roleResult.Succeeded)
                        {
                            logger.LogError("Failed to create role {Role}: {Errors}", role, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                            throw new Exception($"Failed to create role {role}");
                        }
                    }
                }

                // Creare utilizator Admin
                var adminEmail = config["AdminCredentials:Email"];
                var adminPassword = config["AdminCredentials:Password"]
                    ?? throw new InvalidOperationException("Admin password is not set in configuration.");

                if (userManager.Users.All(u => u.Email != adminEmail))
                {
                    var adminUser = new Users
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Administrator",
                        EmailConfirmed = true,
                        Address = "...",
                        DateOfBirth = new DateTime(1982, 1, 1),
                        Nationality = "Romanian",
                        PlaceOfBirth = "...",
                        Occupation = Occupation.Angajat,
                        Mentions = "Admin user",
                        ProfilePicture = "/images/default_admin.png"
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                        throw new Exception("Failed to create admin user.");
                    }

                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                    if (!addToRoleResult.Succeeded)
                    {
                        logger.LogError("Failed to add admin user to Admin role: {Errors}", string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
                        throw new Exception("Failed to assign admin role.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding.");
                throw; // important: retrimite excepția pentru a opri complet pornirea aplicației
            }
        }
    }
}
