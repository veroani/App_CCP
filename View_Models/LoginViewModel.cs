using System.ComponentModel.DataAnnotations;

namespace App_CCP.View_Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Se solicită adresa de e-mail!")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Se solicită parola!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ține-mă minte?")]
        public bool RememberMe { get; set; }
    }
}
