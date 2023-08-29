using System;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class BulkInsertOrganizationViewModel
    {
        public long Id { get; set; }

        public int OrganizationType { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public long LocationId { get; set; }

        public string ContactEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string WebDomain { get; set; }

        public string EdisonInstanceId { get; set; }

        public string EdisonCompanyCodeId { get; set; }

        public string CustomerPrefix { get; set; }

        public string TaxpayerId { get; set; }

        public string WeChatOrWhatsApp { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
