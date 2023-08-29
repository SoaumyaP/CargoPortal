using Groove.SP.Application.Common;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillmentContact.ViewModels
{
    public class POFulfillmentContactViewModel : ViewModelBase<POFulfillmentContactModel>, IHasViewSetting
    {
        public long Id { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }

        public RoleSequence? ContactSequence { get; set; }

        public string WeChatOrWhatsApp { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.FREIGHTBOOKING_DETAIL_CONTACTS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }
    }
}