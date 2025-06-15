using System.ComponentModel.DataAnnotations;

namespace App_CCP.View_Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; }= string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }=string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Confirmarea parolei nu se potrivește cu parola introdusă.")]
        public string ConfirmPassword { get; set; } =string.Empty;
    }

}
