using System.Collections.Generic;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class AssignPurchaseOrdersViewModel
    {
        public List<long> PurchaseOrderIds { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationRole { get; set; }
    }
}
