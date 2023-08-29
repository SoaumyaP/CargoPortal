using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class DelegationPOViewModel
    {
        public long Id { get; set; }

        public long OrganizationId { get; set; }

        public long? NotifyUserId { get; set; }

        public string UpdatedBy { get; set; }
    }
}
