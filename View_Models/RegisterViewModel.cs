using System.ComponentModel.DataAnnotations;

namespace App_CCP.View_Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Se solicită numele.")]
        [Display(Name = "Nume")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Se solicită e-mailul")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Se solicită parola")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "{0} trebuie să aibă între {2} și max {1} caractere lungime.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Parola nu se potrivește.")]
        [Display(Name = "Parola")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Se solicită să confirmați parola.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmă parola")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
