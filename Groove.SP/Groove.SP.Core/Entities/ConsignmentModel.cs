using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ConsignmentModel : Entity
    {
        public long Id { get; set; }

        public long ShipmentId { get; set; }

        public string ConsignmentType { get; set; }

        public DateTime? ConsignmentDate { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        // OrganizationId
        public long ExecutionAgentId { get; set; }

        public string ExecutionAgentName { get; set; }

        public string AgentReferenceNumber { get; set; }

        public string ConsignmentMasterBL { get; set; }

        public string ConsignmentHouseBL { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime ShipToETADate { get; set; }

        public string Status { get; set; }

        public string ModeOfTransport { get; set; }

        public string Movement { get; set; }

        public decimal Unit { get; set; }

        public string UnitUOM { get; set; }

        public decimal Package { get; set; }

        public string PackageUOM { get; set; }

        public decimal Volume { get; set; }

        public string VolumeUOM { get; set; }

        public decimal GrossWeight { get; set; }

        public string GrossWeightUOM { get; set; }

        public decimal NetWeight { get; set; }

        public string NetWeightUOM { get; set; }

        public long? HouseBillId { get; set; }

        public long? MasterBillId { get; set; }

        public bool TriangleTradeFlag { get; set; }

        public bool MemoBOLFlag { get; set; }

        public int Sequence { get; set; }

        public string ServiceType { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ShipmentModel Shipment { get; set; }

        public virtual BillOfLadingModel HouseBill { get; set; }

        public virtual MasterBillOfLadingModel MasterBill { get; set; }

        public virtual ICollection<ConsignmentItineraryModel> ConsignmentItineraries { get; set; }

        public virtual ICollection<ShipmentLoadModel> ShipmentLoads { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }

        public virtual ICollection<BillOfLadingConsignmentModel> BillOfLadingConsignments { get; set; }
    }
}
