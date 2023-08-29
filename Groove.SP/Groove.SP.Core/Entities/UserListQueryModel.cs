using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class UserListQueryModel
    {
        public long Id { get; set; }

        public string AccountNumber { get; set; } //ID

        public string Email { get; set; }

        public string Name { get; set; }

        public string RoleName { get; set; }

        public UserStatus Status { get; set; }

        public string StatusName { get; set; }

        public bool IsInternal { get; set; }

        public long? OrganizationId { get; set; }

        public string OrganizationName { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public string OrganizationTypeName { get; set; }

        public DateTime LastSignInDate { get; set; }
    }
}
