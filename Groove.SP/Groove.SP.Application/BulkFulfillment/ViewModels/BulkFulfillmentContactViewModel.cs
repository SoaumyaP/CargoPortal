using Groove.SP.Application.Common;
using Groove.SP.Application.ViewSetting.Interfaces;
using Groove.SP.Application.ViewSetting.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.BulkFulfillment.ViewModels
{
    public class BulkFulfillmentContactViewModel : ViewModelBase<POFulfillmentContactModel>, IHasViewSetting
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

        public string ViewSettingModuleId { get; set; } = Core.Models.ViewSettingModuleId.BULKBOOKING_DETAIL_CONTACTS;

        public IEnumerable<ViewSettingDataSourceViewModel> ViewSettings { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
