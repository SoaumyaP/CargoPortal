using System;

namespace Groove.SP.Core.Entities
{
    public class ShipmentScheduleQueryModel
    {
        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string Shipper { get; set; }

        public string Consignee { get; set; }

        public decimal TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public DateTime CargoReadyDate { get; set; }

        public string LatestMilestone { get; set; }
    }
}
