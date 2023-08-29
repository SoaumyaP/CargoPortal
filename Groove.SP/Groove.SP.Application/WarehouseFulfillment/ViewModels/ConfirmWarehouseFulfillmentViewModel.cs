using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class ConfirmWarehouseFulfillmentViewModel
    {
        public long Id { set; get; }
        public string BookingNumber { set; get; }
        public string ShipmentNumber { set; get; }
        public DateTime? ExpectedDeliveryDate { set; get; }
        public long CustomerOrgId { set; get; }
        public long SupplierOrgId { set; get; }
        public string WarehouseLocation { set; get; }
    }
}
