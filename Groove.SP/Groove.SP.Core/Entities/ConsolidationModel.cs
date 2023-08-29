using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ConsolidationModel : Entity
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public long? ContainerId { get; set; }

        public string ConsolidationNo { get; set; }

        public string ModeOfTransport { get; set; }

        public string EquipmentType { get; set; }

        public string OriginCFS { get; set; }

        public DateTime CFSCutoffDate { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string CarrierSONo { get; set; }

        public ConsolidationStage Stage { get; set; }

        public DateTime? LoadingDate { get; set; }

        public virtual ContainerModel Container { get; set; }

        public virtual ICollection<ShipmentLoadModel> ShipmentLoads { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }

        public virtual ICollection<BillOfLadingShipmentLoadModel> BillOfLadingShipmentLoads { get; set; }
    }
}
