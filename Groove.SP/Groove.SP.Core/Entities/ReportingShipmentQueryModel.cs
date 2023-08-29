using System;

namespace Groove.SP.Core.Entities
{
    public class ReportingShipmentQueryModel
    {
        public long Id { get; set; }

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public string Movement { get; set; }

        public string ServiceType { get; set; }

        public string ModeOfTransport { get; set; }

        public decimal OceanVolume { get; set; }

        public DateTime ShipFromETDDate { get; set; }
    }
}
