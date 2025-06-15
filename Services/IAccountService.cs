using App_CCP.Models;

namespace App_CCP.Services
{
    public interface IAccountService
    {
        Task AddOrUpdateFullNameClaimAsync(Users user);
        Task ResetUserPasswordAsync(Users user, string newPassword);
    }
}
