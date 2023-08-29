using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class POViewModel
    {
        public string PONumber { get; set; }
        public PurchaseOrderStatus? Status { get; set; }
    }
}
