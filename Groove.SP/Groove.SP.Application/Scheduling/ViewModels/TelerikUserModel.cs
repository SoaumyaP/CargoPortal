namespace Groove.SP.Application.Scheduling.ViewModels
{
    public class TelerikUserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Enabled { get; set; }
        public string[] UserRoleIds { get; set; }
        public long OrganizationId { get; set; }

    }
}