using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ContainerModel : Entity
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string LoadPlanRefNo { get; set; }

        /// <summary>
        /// It maps to EnumMember attribute from <see cref="Groove.SP.Core.Models.EquipmentType"/>
        /// <br></br> {[EnumMember(Value = "20DG")] TwentyDG = 3} => 20DG
        /// </summary>
        public string ContainerType { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public DateTime? LoadingDate { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string CarrierSONo { get; set; }

        public string Movement { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public int TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public bool IsFCL { get; set; }

        public virtual ConsolidationModel Consolidation { get; set; }

        public virtual ICollection<ShipmentLoadModel> ShipmentLoads { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }

        public virtual ICollection<ContainerItineraryModel> ContainerItineraries { get; set; }

        public virtual ICollection<BillOfLadingShipmentLoadModel> BillOfLadingShipmentLoads { get; set; }
    }
}
