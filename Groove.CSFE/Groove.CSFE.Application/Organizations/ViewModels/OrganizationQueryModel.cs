using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using System;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class OrganizationQueryModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string ContactEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string WebsiteDomain { get; set; }

        public OrganizationStatus Status { get; set; }

        public string StatusName { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public string OrganizationTypeName { get; set; }

        public string CountryName { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
