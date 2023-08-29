using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.WarehouseFulfillment.ViewModels
{
    public class WarehouseFulfillmentLocationSOFormQueryModel
    {
        public string LocationName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPerson { get; set; }
        public string WorkingHours { get; set; }
        public string Remarks { get; set; }
    }
}
