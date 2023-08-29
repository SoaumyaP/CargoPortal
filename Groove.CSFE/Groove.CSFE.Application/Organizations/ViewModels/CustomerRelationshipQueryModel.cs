using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using System;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class CustomerRelationshipQueryModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string ContactEmail { get; set; }

        public string AdminUser { get; set; }

        public string ContactName { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public string OrganizationTypeName => EnumHelper<OrganizationType>.GetDisplayValue(this.OrganizationType);

        public string CountryName { get; set; }

        public DateTime CreatedDate { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public bool IsConfirmConnectionType { get; set; }

        public string ConnectionTypeName => EnumHelper<ConnectionType>.GetDisplayValue(this.ConnectionType);

        public string CustomerRefId { get; set; }
    }
}
