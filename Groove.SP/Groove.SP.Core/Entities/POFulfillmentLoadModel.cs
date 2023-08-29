using System;
using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentLoadModel : Entity
    {
        public long Id { get; set; }

        public long POFulfillmentId { get; set; }

        public POFulfillmentModel PoFulfillment { get; set; }

        public string LoadReferenceNumber { get; set; }


        // update container
        public string ContainerNumber { get; set; }

        public string SealNumber { get; set; }

        public string SealNumber2 { get; set; }

        public DateTime? LoadingDate { get; set; }

        public DateTime? GateInDate { get; set; }

        public decimal? TotalGrossWeight { get; set; }

        public decimal? TotalNetWeight { get; set; }



        public EquipmentType EquipmentType { get; set; }

        public decimal PlannedVolume { get; set; }

        public decimal? PlannedNetWeight { get; set; }

        public decimal PlannedGrossWeight { get; set; }

        public int PlannedPackageQuantity { get; set; }

        public int SubtotalPackageQuantity { get; set; }

        public int SubtotalUnitQuantity { get; set; }

        public decimal SubtotalNetWeight { get; set; }
        public decimal SubtotalGrossWeight { get; set; }


        public decimal SubtotalVolume { get; set; }

        public PackageUOMType PackageUOM { get; set; }

        public POFulfillmentLoadStatus Status { get; set; }

        public ICollection<POFulfillmentLoadDetailModel> Details { get; set; }
    }
}