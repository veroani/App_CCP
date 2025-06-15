using System.ComponentModel.DataAnnotations;

namespace App_CCP.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;// ID-ul utilizatorului care a lasat review-ul
        public string UserName { get; set; } = string.Empty; // Numele utilizatorului
        public string Content { get; set; } = string.Empty; // Textul review-ului
        public DateTime DateCreated { get; set; } = DateTime.Now; // Data crearii review-ului
        public bool IsPublished { get; set; } = false; //  review-ul este publicat sau nu?
        public string ? Alias { get; set; } // alias introdus de utilizator
    }
}
