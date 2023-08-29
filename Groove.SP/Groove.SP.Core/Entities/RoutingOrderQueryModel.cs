using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Core.Entities
{
    public class RoutingOrderQueryModel
    {
        public long Id { get; set; }
        public string RoutingOrderNumber { get; set; }
        public string ShipperCompany { get; set; }
        public string ConsigneeCompany { get; set; }
        public DateTime RoutingOrderDate { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public RoutingOrderStageType Stage { get; set; }
        public string StageName { get; set; }
        public RoutingOrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public IncotermType Incoterm { get; set; }
        public string IncotermName { get; set; }
        public string ShipFromName { get; set; }
        public string ShipToName { get; set; }
    }
}
