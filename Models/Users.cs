using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace App_CCP.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string ? Address { get; set; }
        public string? Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PlaceOfBirth { get; set; }
        public Occupation Occupation { get; set; }
        public string ? Mentions { get; set; }
        public string ? ProfilePicture { get; set; }
        public int? Age
        {
            get
            {
                if (!DateOfBirth.HasValue) return null;
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Value.Year;
                if (today < DateOfBirth.Value.AddYears(age)) age--;
                return age;
            }
        }


        public bool IsCardApprovedByAdmin { get; set; }
        public bool IsCardRevoked { get; set; } = false;

        // Campul pentru data expirării cardului cultural
        public DateTime ExpirationDate { get; set; }
     
        public bool IsFirstLogin { get; set; } = true;
        public void InitializeCardData()
        {
            if (ExpirationDate == default)
            {
                ExpirationDate = DateTime.Now.AddYears(1);
            }

            if (string.IsNullOrEmpty(UniqueCode))
            {
                GenerateUniqueCode();
            }
        }
        public string UniqueCode { get; set; } = string.Empty;
        public void GenerateUniqueCode()
        {
            if (string.IsNullOrEmpty(UniqueCode))
            {
                // generam un număr aleator între 10000 și 99999
                var randomNumber = new Random().Next(10000, 99999);

                // adaugam litera "A" la începutul codului
                UniqueCode = $"A{randomNumber}";
            }
        }
    }
        public enum Occupation
    {
        [Display(Name = "Nicio ocupație selectată")]
        _,
        [Display(Name = "Copil")]
        Copil,
        [Display(Name = "Elev")]
        Elev,
        [Display(Name = "Student")]
        Student,
        [Display(Name = "Profesor de franceză")]
        ProfesorFranceza,
        [Display(Name = "Angajat")]
        Angajat,
        [Display(Name = "Șomer")]
        Somer,
        [Display(Name = "Pensionar")]
        Pensionar
    }
}
