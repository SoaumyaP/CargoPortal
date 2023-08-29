using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class BillOfLadingModel : Entity
    {
        public long Id { get; set; }

        public string BillOfLadingNo { get; set; }

        /// <summary>
        /// OrganizationID
        /// </summary>
        public long? ExecutionAgentId { get; set; }

        public string BillOfLadingType { get; set; }

        public string MainCarrier { get; set; }

        public string MainVessel { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string JobNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public string ModeOfTransport { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Movement { get; set; }

        public string Incoterm { get; set; }

        public virtual ICollection<BillOfLadingContactModel> Contacts { get; set; }

        public virtual ICollection<ShipmentBillOfLadingModel> ShipmentBillOfLadings { get; set; }

        public virtual ICollection<BillOfLadingItineraryModel> BillOfLadingItineraries { get; set; }

        public virtual ICollection<ConsignmentModel> Consignments { get; set; }

        /// <summary>
        /// To define linkage between bill of lading and master bill of lading
        /// </summary>
        public virtual ICollection<BillOfLadingShipmentLoadModel> BillOfLadingShipmentLoads { get; set; }

        public virtual ICollection<BillOfLadingConsignmentModel> BillOfLadingConsignments { get; set; }
    }
}
