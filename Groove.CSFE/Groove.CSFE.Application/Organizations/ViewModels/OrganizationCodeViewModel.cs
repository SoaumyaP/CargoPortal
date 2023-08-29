using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class OrganizationCodeViewModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public string WebsiteDomain { get; set; }
      
        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }
    }
}
