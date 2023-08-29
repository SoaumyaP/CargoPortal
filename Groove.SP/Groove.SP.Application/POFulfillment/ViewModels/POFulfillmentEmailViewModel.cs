
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class POFulfillmentEmailViewModel
    {
        public string Name { get; set; }

        public string BookingRefNumber { get; set; }

        public string DetailPage { get; set; }

        public string Shipper { get; set; }

        public string Consignee { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public List<string> EquipmentTypes { get; set; }

        public string SupportEmail { get; set; }
    }
}