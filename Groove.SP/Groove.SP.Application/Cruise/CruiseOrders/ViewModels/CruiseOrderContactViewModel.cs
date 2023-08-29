using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Application.PurchaseOrders.Validations;
using Groove.SP.Core.Entities.Cruise;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class CruiseOrderContactViewModel : ViewModelBase<CruiseOrderContactModel>
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
