using App_CCP.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace App_CCP.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<Users> _userManager;

        public AccountService(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

        public async Task AddOrUpdateFullNameClaimAsync(Users user)
        {
            if (string.IsNullOrEmpty(user.FullName))
            {
                user.FullName = user.Email ?? "No Name Provided";
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var existingClaim = claims.FirstOrDefault(c => c.Type == "FullName");

            if (existingClaim != null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            await _userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
        }

        public async Task ResetUserPasswordAsync(Users user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
