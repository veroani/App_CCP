using App_CCP.Models;
using App_CCP.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App_CCP.View_Models
{
    public class AdminEditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Numele complet este obligatoriu.")]
        [StringLength(100)]
        [Display(Name = "Nume complet")]
        public string ? FullName { get; set; }
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mailul este obligatoriu.")]
        [Display(Name = "Email")] 
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adresa este obligatorie.")]
        [StringLength(200)]
        [Display(Name = "Adresa")] 
        public string ? Address { get; set; }

        [Required(ErrorMessage = "Numărul de telefon este obligatoriu.")]
        [Phone]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Numărul de telefon nu este valid.")]
        [Display(Name = "Telefon")]
        public string ? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Naționalitatea este obligatorie.")]
        [StringLength(100)]
        [Display(Name = "Naționalitate")] 
        public string ? Nationality { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data nașterii")]
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.ValidateBirthDate))]
        public DateTime ? DateOfBirth { get; set; }

        [Display(Name = "Vârstă")] 
        public int Age { get; set; } // Doar pentru afișare
        [Required(ErrorMessage = "Locul nașterii este obligatoriu.")]
        [StringLength(100)]
        [Display(Name = "Locul nașterii")] 
        public string ? PlaceOfBirth { get; set; }
        
        [Required]
        [Display(Name = "Ocupație")]
        public Occupation Occupation { get; set; } = Occupation.Copil;

        [Display(Name = "Fotografie de profil")] 
        public IFormFile? ProfilePicture { get; set; } // Pentru incarcarea de fisiere

        [Display(Name = "Imaginea curentă")] 
        public string? ProfilePicturePath { get; set; } // Pentru calea imaginii existente

        [DataType(DataType.Date)]
        [Display(Name = "Data expirării cardului")]
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.ValidateExpirationDate))]
        public DateTime ExpirationDate { get; set; }

        public string ? ConcurrencyStamp { get; set; }

        public string UniqueCode { get; set; } = string.Empty;
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> OccupationList { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
