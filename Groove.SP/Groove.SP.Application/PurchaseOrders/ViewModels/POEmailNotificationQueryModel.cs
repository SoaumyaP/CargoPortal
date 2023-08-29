using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public  class POEmailNotificationQueryModel
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public DateTime? ProposeDate { get; set; }
        public long OrganizationId { get; set; }
        public string OrganizationRole { get; set; }
        public string ContactEmail { get; set; }

        public long shipFromId { set; get; }
    }
}
