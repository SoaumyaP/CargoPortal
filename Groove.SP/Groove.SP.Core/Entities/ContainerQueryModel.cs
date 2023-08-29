using System;

namespace Groove.SP.Core.Entities
{
    public class ContainerQueryModel
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string ShipFrom { get; set; }
        public string ShipTo { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Movement { get; set; }
    }
}
