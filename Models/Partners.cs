using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App_CCP.Models
{
    public class Partners
    {
        public int Id { get; set; } // ID unic

        [Required(ErrorMessage = "Numele partenerului este obligatoriu.")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Numele partenerului

        [MaxLength(500, ErrorMessage = "Descrierea nu poate depăși 500 de caractere.")]
        public string Description { get; set; } = string.Empty; // Descrierea partenerului

        [MaxLength(255)]
        public string DiscountDetails { get; set; } = string.Empty; // Detalii despre reducere

        [MaxLength(255)]
        public string LogoUrl { get; set; } = string.Empty; // URL-ul catre logo-ul partenerului

        [MaxLength(100)]
        public string WebsiteUrl { get; set; } = string.Empty; // URL catre site-ul partenerului
                                                               // Legatura cu utilizatorul
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }  // Users = clasa  personalizata pentru Identity
    }
}