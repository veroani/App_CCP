namespace App_CCP.View_Models
{
    public class UserRoleViewModel
    {
        public string ? UserId { get; set; }
        public string ? UserName { get; set; }
        public string ? Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsPartner { get; set; }
        public List<string> Roles { get; set; } = new();

    }
}

