using System.ComponentModel.DataAnnotations;

namespace App_CCP.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty; // Numele pe disc

        [Required]
        public string OriginalName { get; set; } = string.Empty; // Numele incarcat de utilizator

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty; // "PrivacyPolicy", "TermsConditions"

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
