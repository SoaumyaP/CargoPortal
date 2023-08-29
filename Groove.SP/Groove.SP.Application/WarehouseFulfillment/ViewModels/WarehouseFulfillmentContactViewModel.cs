using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseFulfillmentContactViewModel : ViewModelBase<POFulfillmentContactModel>
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

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
