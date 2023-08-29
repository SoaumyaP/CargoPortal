using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class PODelegationEmailTemplateViewModel
    {
        public string Name { get; set; }
        public string PONumber { get; set; }
        public string CompanyName { get; set; }
        public string SupportEmail { get; set; }
        public string DetailPage { get; set; }
    }
}
