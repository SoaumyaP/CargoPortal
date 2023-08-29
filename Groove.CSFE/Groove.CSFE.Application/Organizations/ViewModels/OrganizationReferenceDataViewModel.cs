using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using System.ComponentModel.DataAnnotations;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class OrganizationReferenceDataViewModel
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string OrgCodeName => Code + ' ' + Name;

        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string ContactEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string CustomerPrefix { get; set; }

        public bool IsBuyer { get; set; }

        public OrganizationType OrganizationType { get; set; }

        public AgentType AgentType { get; set; }

        public string OrganizationTypeName => EnumHelper<OrganizationType>.GetDisplayValue(this.OrganizationType, nameof(DisplayAttribute.Description));

        public OrganizationStatus Status { get; set; }

        public string StatusName => EnumHelper<OrganizationStatus>.GetDisplayValue(this.Status, nameof(DisplayAttribute.Description));
        public string WeChatOrWhatsApp { get; set; }
    }
}
