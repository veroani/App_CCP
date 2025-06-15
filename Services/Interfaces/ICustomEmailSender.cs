namespace App_CCP.Services.Interfaces

{
    using System.Threading.Tasks;
    public interface ICustomEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
