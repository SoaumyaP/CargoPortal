using Groove.SP.Core.Events;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ShipmentModel : Entity
    {
        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string BookingNo { get; set; }

        public string BuyerCode { get; set; }

        public string CustomerReferenceNo { get; set; }

        public string AgentReferenceNo { get; set; }

        public string ShipperReferenceNo { get; set; }

        public string ModeOfTransport { get; set; }

        public DateTime CargoReadyDate { get; set; }

        public DateTime BookingDate { get; set; }

        public string ShipmentType { get; set; }

        public string ShipFrom { get; set; }

        public DateTime ShipFromETDDate { get; set; }

        public string ShipTo { get; set; }

        public DateTime? ShipToETADate { get; set; }

        public string Movement { get; set; }

        public int? Factor { get; set; }

        public decimal TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalUnit { get; set; }

        public string TotalUnitUOM { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string ServiceType { get; set; }

        public string Incoterm { get; set; }

        public string Status { get; set; }
        
        public bool IsFCL { get; set; }

        public bool IsItineraryConfirmed { get; set; }

        public long? POFulfillmentId { get; set; }

        public OrderType OrderType { get; set; }

        public string CommercialInvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// To map with <see cref="ContractMasterModel.CarrierContractNo"/> then show <see cref="ContractMasterModel.RealContractNo"/>
        /// </summary>
        public string CarrierContractNo { get; set; }

        public ContractMasterModel ContractMaster { get; set; }

        public POFulfillmentModel POFulfillment { get; set; }

        public virtual ICollection<ShipmentBillOfLadingModel> ShipmentBillOfLadings { get; set; }

        public virtual ICollection<ShipmentContactModel> Contacts { get; set; }

        public virtual ICollection<CargoDetailModel> CargoDetails { get; set; }

        public virtual ICollection<ShipmentLoadModel> ShipmentLoads { get; set; }

        public virtual ICollection<ShipmentLoadDetailModel> ShipmentLoadDetails { get; set; }

        /// <summary>
        /// To define linkage between shipment and master bill of lading
        /// </summary>
        public virtual ICollection<ConsignmentModel> Consignments { get; set; }

        /// <summary>
        /// To define linkage between shipment and master bill of lading
        /// </summary>
        public virtual ICollection<ConsignmentItineraryModel> ConsignmentItineraries { get; set; }

        public virtual ICollection<BillOfLadingConsignmentModel> BillOfLadingConsignments { get; set; }

        public virtual ICollection<ShipmentItemModel> ShipmentItems { get; set; }

        public virtual ICollection<POFulfillmentAllocatedOrderModel> POFulfillmentAllocatedOrders { get; set; }

        #region Methods
        public void SetCancelledStatus()
        {
            Status = StatusType.INACTIVE;
            AddDomainEvent(new ShipmentCancelledDomainEvent(this.Id));
        }
        #endregion
    }
}
