using Groove.SP.Core.Models;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class PurchaseOrderAdhocChangeViewModel
    {
        public long Id { get; set; }
        public long POFulfillmentId { get; set; }
        public long PurchaseOrderId { get; set; }
        public PurchaseOrderAdhocChangePriority Priority { get; set; }
        public string Message { get; set; }

        public static PurchaseOrderAdhocChangeViewModel NotChangeResult()
        {
            return new PurchaseOrderAdhocChangeViewModel
            {
                Message = "There is no purchase order ad-hoc change",
                Priority = PurchaseOrderAdhocChangePriority.NotChanged
            };
        }
    }
    
}
