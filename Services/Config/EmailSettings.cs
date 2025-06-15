namespace App_CCP.Services.Config
{
    public class EmailSettings
    {
        public string? SmtpServer { get; set; } 
        public int SmtpPort { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
        public string? SupportEmail { get; set; }
    }
}
//declaram proprietatile ca nullable (?)  dupa tipul lor pentru a opri avertismentele CS8618 datorate sistemului de Null Safety
//constructorul clasei nu poate garanta ca proprietatile sunt populate imediat