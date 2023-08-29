using System;
using System.Collections.Generic;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class SelectPurchaseOrderViewModel 
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public int ItemsCount { get; set; }
        public long RecordCount { get; set; }
    }

    public class SelectUnmappedPurchaseOrderViewModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public string ShipFrom { get; set; }
        public string DisplayText { get; set; }
        public long RecordCount { get; set; }
    }
}
