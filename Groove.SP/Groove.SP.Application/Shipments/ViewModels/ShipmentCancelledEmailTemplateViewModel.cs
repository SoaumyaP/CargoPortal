using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.Shipments.ViewModels
{
    public class ShipmentCancelledEmailTemplateViewModel
    {
        public string Name { get; set; }
        public string ShipmentNo { get; set; }
        public string DetailPage { get; set; }
        public string SupportEmail { get; set; }
    }
}
