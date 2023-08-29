using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Locations.ViewModels;
using Groove.CSFE.Application.Organizations.Validations;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Groove.CSFE.Application.Converters;
using Groove.CSFE.Application.Converters.Interfaces;
using Newtonsoft.Json;
using Groove.CSFE.Application.EmailNotification.ViewModel;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class OrganizationViewModel : ViewModelBase<OrganizationModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string OrgCodeName => Code + ' ' + Name;

        public string Address { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string EdisonInstanceId { get; set; }

        public string EdisonCompanyCodeId { get; set; }

        public string ContactEmail { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string WeChatOrWhatsApp { get; set; }

        public string WebsiteDomain { get; set; }

        public string ParentId { get; set; }
        public string ParentOrgCode { get; set; }
        public string ParentOrgName{ get; set; }

        public OrganizationStatus Status { get; set; }

        public string CustomerPrefix { get; set; }

        public string OrganizationLogo { get; set; }
        public string TaxpayerId { get; set; }

        public string StatusName => EnumHelper<OrganizationStatus>.GetDisplayValue(this.Status);

        public OrganizationType OrganizationType { get; set; }

        public AgentType AgentType { get; set; }

        public SOFormGenerationFileType SOFormGenerationFileType { get; set; }

        public string OrganizationTypeName => EnumHelper<OrganizationType>.GetDisplayValue(this.OrganizationType);

        public ICollection<long> RemoveAffiliateIds { get; set; }

        public ICollection<long> RemoveCustomerIds { get; set; }

        public ICollection<long> PendingCustomerIds { get; set; }

        public ICollection<EmailNotificationViewModel> EmailNotifications { get; set; }

        public long? LocationId { get; set; }

        public LocationViewModel Location { get; set; }

        public bool IsBuyer { get; set; }

        public string AdminUser { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new OrganizationValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
