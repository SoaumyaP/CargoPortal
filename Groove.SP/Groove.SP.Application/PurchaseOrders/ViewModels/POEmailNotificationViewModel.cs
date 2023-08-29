using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class POEmailNotificationViewModel
    {
        public POEmailNotificationViewModel()
        {
            POs = new List<POEmailDetailViewModel>();
        }
        public string Here { get; set; }
        public long? SupplierId { get; set; }
        public long? OriginAgentId { get; set; }
        public long shipFromId { set; get; }
        public long customerId { set; get; }
        public string To { get; set; }
        public string CC { get; set; }
        public List<POEmailDetailViewModel> POs { set; get; }
    }
}
