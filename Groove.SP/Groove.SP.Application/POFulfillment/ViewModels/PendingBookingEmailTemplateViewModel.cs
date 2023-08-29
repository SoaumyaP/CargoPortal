using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class PendingBookingEmailTemplateViewModel
    {
        public string Name { get; set; }
        public string BookingRefNumber { get; set; }
        public string DetailPage { get; set; }
        public string SupportEmail { get; set; }
        public string Eta { get; set; }
        public string WarehouseName { get; set; }
        public string WarehouseAddress { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
    }
}
