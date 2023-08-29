using Groove.SP.Application.Common;
using Groove.SP.Application.RoutingOrder.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.RoutingOrderContact.ViewModels
{
    public class RoutingOrderContactViewModel : ViewModelBase<RoutingOrderContactModel>
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationCode { get; set; }
        public string OrganizationRole { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }

        public virtual RoutingOrderViewModel RoutingOrder { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}