using App_CCP.Models;


namespace App_CCP.View_Models
{
    public class StatisticsViewModel
    {
        public DateTime? ExpirationDate { get; set; } // Data selectată de utilizator
        public string ? UniqueCode { get; set; }
        public required IEnumerable<Users> Users { get; set; } // Lista de utilizatori
    }
}