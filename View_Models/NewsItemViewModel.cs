using App_CCP.Models;
using System.ComponentModel.DataAnnotations;

namespace App_CCP.View_Models
{
    public class NewsItemViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? ExistingImagePath { get; set; }

        public bool IsActive { get; set; } = true;
        public Users? Owner { get; set; }
    }
}