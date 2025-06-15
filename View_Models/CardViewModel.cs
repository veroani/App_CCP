namespace App_CCP.View_Models
{
    public class CardViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string ? ProfilePicture { get; set; }
        public DateTime ExpirationDate { get; set; } = DateTime.Now;

        public string UniqueCode { get; set; } = string.Empty;

        public string ExpirationDateDisplay
        {
            get
            {
                // Dacă ExpirationDate este null sau data este invalidă, folosim un an de la data curentă
                return ExpirationDate == default(DateTime)
                    ? DateTime.Now.AddYears(1).ToShortDateString()
                    : ExpirationDate.ToShortDateString();
            }
        }

        public string ? BarcodeSvg { get; internal set; }
        public string? ExpirationMessage
        {
            get
            {
                var daysRemaining = (ExpirationDate.Date - DateTime.Now.Date).Days;

                if (daysRemaining < 0)
                {
                    return "⚠️ Cardul dumneavoastră a expirat. Vă rugăm să contactați biblioteca emitentă pentru reînnoire!";
                }
                else if (daysRemaining <= 30)
                {
                    return $"🔔 Cardul dumneavoastră va expira în {daysRemaining} {(daysRemaining == 1 ? "zi" : "zile")}. Vă rugăm să aveți în vedere prelungirea acestuia!";
                }
                return null; // niciun mesaj in alte cazuri
            }
        }
    }
}
