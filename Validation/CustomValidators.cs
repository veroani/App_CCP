using System.ComponentModel.DataAnnotations;

namespace App_CCP.Validation
{

    public static class CustomValidators
    {
        public static ValidationResult? ValidateBirthDate(object? value, ValidationContext _)
        {
            if (value is not DateTime date)
                return null;

            if (date > DateTime.Today)
                return new ValidationResult("Data nașterii nu poate fi în viitor.");

            if (date < new DateTime(1900, 1, 1))
                return new ValidationResult("Data nașterii nu poate fi mai veche de anul 1900.");

            if ((DateTime.Today.Year - date.Year) > 120)
                return new ValidationResult("Vârsta nu poate depăși 120 de ani.");

            return null;
        }

        public static ValidationResult? ValidateExpirationDate(DateTime? date, ValidationContext _)
        {
            if (date == null)
                return ValidationResult.Success; // Permitem valoare null

            if (date < DateTime.Today)
                return new ValidationResult("Data expirării nu poate fi în trecut.");

            if (date > DateTime.Today.AddYears(5))
                return new ValidationResult("Data expirării nu poate fi mai mare de 5 ani de la data curentă.");

            return ValidationResult.Success;
        }
    }
}
