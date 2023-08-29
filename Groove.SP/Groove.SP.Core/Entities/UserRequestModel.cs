using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class UserRequestModel : Entity
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public string CompanyAddressLine1 { get; set; }

        public string CompanyAddressLine2 { get; set; }

        public string CompanyAddressLine3 { get; set; }

        public string CompanyAddressLine4 { get; set; }

        public string CompanyWeChatOrWhatsApp { get; set; }

        public string Customer { get; set; }

        public UserStatus Status { get; set; }

        public bool IsInternal { get; set; }

        public long? OrganizationId { get; set; }

        public long? OrganizationRoleId { get; set; }

        public string OrganizationName { get; set; }

        public string OrganizationCode { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public long? OPCountryId { get; set; }

        public string OPLocationName { get; set; }

        public string OPContactEmail { get; set; }

        public string OPContactName { get; set; }

        public string TaxpayerId { get; set; }

        public long? RoleId { get; set; }

        public string RoleName { get; set; }
    }
}
