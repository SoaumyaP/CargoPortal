using Groove.SP.Core.Data;
using Groove.SP.Core.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.SP.Core.Entities
{
    public class ShipmentQueryModel
    {
        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string CustomerReferenceNo { get; set; }

        public string AgentReferenceNo { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime BookingDate { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string Status { get; set; }

        public string Shipper { get; set; }

        public string Consignee { get; set; }
        public string ActivityCode { get; set; }
        public string Milestone { get; set; }

    }

    public class ShipmentMilestoneSingleQueryModel: DataSourceSingleQueryModel
    {
        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string CustomerReferenceNo { get; set; }

        public string AgentReferenceNo { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime BookingDate { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string Status { get; set; }

        public string Shipper { get; set; }

        public string Consignee { get; set; }
        public string ActivityCode { get; set; }
        public string Milestone { get; set; }
    }
}
