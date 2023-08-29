using System;
using System.Collections.Generic;
using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentModel : Entity
    {
        public long Id { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public IncotermType Incoterm { get; set; }

        public bool IsPartialShipment { get; set; }

        public bool IsContainDangerousGoods { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public ModeOfTransportType ModeOfTransport { get; set; }

        public long PreferredCarrier { get; set; }

        public LogisticsServiceType LogisticsService { get; set; }

        public MovementType MovementType { get; set; }

        public string DeliveryMode { get; set; }

        public bool IsGeneratePlanToShip { get; set; }

        /// <summary>
        /// Using for bulk booking only
        /// </summary>
        public bool IsAllowMixedCarton { get; set; }

        // LocationId
        public long ShipFrom { get; set; }

        // LocationId
        public long ShipTo { get; set; }

        public string ShipFromName { get; set; }

        public string ShipToName { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string Remarks { get; set; }

        public bool IsForwarderBookingItineraryReady { get; set; }

        public bool IsRejected { get; set; }

        public bool IsFulfilledFromPO { get; set; }

        public POType FulfilledFromPOType { get; set; }

        public OrderFulfillmentPolicy OrderFulfillmentPolicy { get; set; }

        public long? ReceiptPortId { get; set; }

        public long? DeliveryPortId { get; set; }

        public string ReceiptPort { get; set; }

        public string DeliveryPort { get; set; }

        public bool IsShipperPickup { get; set; }

        public bool IsNotifyPartyAsConsignee { get; set; }

        public bool IsCIQOrFumigation { get; set; }

        public bool IsBatteryOrChemical { get; set; }

        public bool IsExportLicence { get; set; }

        public string AgentAssignmentMode { get; set; }

        public DateTime? BookingDate { get; set; }

        public string VesselName { get; set; }

        public string VoyageNo { get; set; }

        public string PlantNo { get; set; }

        public string SONo { get; set; }

        public string Forwarder { get; set; }

        public DateTime? ActualTimeArrival { get; set; }
        public string ContainerNo { get; set; }
        public string HAWBNo { get; set; }
        public string NameofInternationalAccount { get; set; }
        public string CompanyNo { get; set; }
        public DateTime? ETDOrigin { get; set; }
        public DateTime? ETADestination { get; set; }
        public string ConfirmBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ConfirmedHubArrivalDate { get; set; }
        public string Time { set; get; }
        public string LoadingBay { set; get; }
        public string CYEmptyPickupTerminalCode { get; set; }
        public string CYEmptyPickupTerminalDescription { get; set; }
        public string CFSWarehouseCode { get; set; }
        public string CFSWarehouseDescription { get; set; }
        public DateTime? CYClosingDate { get; set; }
        public DateTime? CFSClosingDate { get; set; }

        /// <summary>
        /// To be used in Warehouse booking that defines email subject prior to sent to
        /// </summary>
        public string EmailSubject { get; set; }

       

        public ICollection<POFulfillmentContactModel> Contacts { get; set; }

        public ICollection<POFulfillmentOrderModel> Orders { get; set; }

        public ICollection<POFulfillmentShortshipOrderModel> ShortshipOrders { get; set; }

        public ICollection<POFulfillmentLoadModel> Loads { get; set; }

        public ICollection<POFulfillmentCargoDetailModel> CargoDetails { get; set; }

        public ICollection<POFulfillmentBookingRequestModel> BookingRequests { get; set; }

        public ICollection<BuyerApprovalModel> BuyerApprovals { get; set; }

        public ICollection<POFulfillmentItineraryModel> Itineraries { get; set; }

        public ICollection<ShipmentModel> Shipments { get; set; }

        public ICollection<PurchaseOrderAdhocChangeModel> PurchaseOrderAdhocChanges { get; set; }
        public string PoRemark { get; set; }
    }
}