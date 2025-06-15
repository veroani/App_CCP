namespace App_CCP.View_Models
{
    public class AboutCCPViewModel
    {
        public string Titlu { get; set; } = string.Empty;
        public string Descriere { get; set; } = string.Empty;
        public List<string> Avantaje { get; set; } = new List<string>();
        public string ImagineUrl { get; set; } = string.Empty;
    }
}

