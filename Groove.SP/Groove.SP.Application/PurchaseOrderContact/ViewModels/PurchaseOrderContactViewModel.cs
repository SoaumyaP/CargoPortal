using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using FluentValidation;
using Groove.SP.Application.PurchaseOrderContact.Validations;
using System.Collections.Generic;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Application.ViewSetting.Interfaces;
using System.Text.Json.Serialization;

namespace Groove.SP.Application.PurchaseOrderContact.ViewModels
{
    public class PurchaseOrderContactViewModel : ViewModelBase<PurchaseOrderContactModel>, IHasViewSetting
    {
        public long Id { get; set; }

        public long PurchaseOrderId { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationCode { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressLine4 { get; set; }

        public string Department { get; set; }

        public string ContactName { get; set; }

        public string Name { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }

        public string WeChatOrWhatsApp { get; set; }

        public string References { get; set; }

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        [JsonIgnore]
        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.PO_DETAIL_CONTACTS;

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new PurchaseOrderContactValidation().ValidateAndThrow(this);
        }
    }
}
