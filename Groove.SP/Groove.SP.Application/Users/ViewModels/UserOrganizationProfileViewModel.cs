using Groove.SP.Core.Models;

namespace Groove.SP.Application.Users.ViewModels
{
    public class UserOrganizationProfileViewModel
    {
        public long? OrganizationId { get; set; }

        public string OrganizationCode { get; set; }

        public string OrganizationName { get; set; }

        public long? OrganizationRoleId { get; set; }

        public OrganizationType OrganizationType { get; set; }
    }
}