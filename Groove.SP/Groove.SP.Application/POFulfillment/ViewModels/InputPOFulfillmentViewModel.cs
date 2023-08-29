using System;
using System.Collections.Generic;
using FluentValidation;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.POFulfillment.Validations;
using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillmentContact.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class InputPOFulfillmentViewModel : ViewModelBase<POFulfillmentModel>
    {
        public long Id { get; set; }

        public string CustomerPrefix { get; set; }

        public string Number { get; set; }

        public string Owner { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }

        public bool IsPartialShipment { get; set; }

        public bool IsContainDangerousGoods { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        public long PreferredCarrier { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogisticsServiceType LogisticsService { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MovementType MovementType { get; set; }

        // LocationId
        public long ShipFrom { get; set; }

        // LocationId
        public long ShipTo { get; set; }

        // LocationDescription
        public string ShipFromName { get; set; }

        // LocationDescription
        public string ShipToName { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string Remarks { get; set; }

        public bool IsForwarderBookingItineraryReady { get; set; }

        public bool IsFulfilledFromPO { get; set; }

        public POType FulfilledFromPOType { get; set; }

        public bool IsShipperPickup { get; set; }

        public bool IsNotifyPartyAsConsignee { get; set; }

        public bool IsCIQOrFumigation { get; set; }

        public bool IsBatteryOrChemical { get; set; }

        public bool IsExportLicence { get; set; }

        public bool IsPurchaseOrderRefreshed { get; set; }

        // LocationId
        public long? ReceiptPortId { get; set; }

        // LocationId
        public long? DeliveryPortId { get; set; }

        // LocationDescription
        public string ReceiptPort { get; set; }

        // LocationDescription
        public string DeliveryPort { get; set; }

        public long? OrganizationId { get; set; }

        //To check if user update CustomerPo of booking on UI
        public bool UpdateOrganizationPreferences { get; set; }

        public string AgentAssignmentMode { get; set; }

        public OrderFulfillmentPolicy OrderFulfillmentPolicy { get; set; }

        public ICollection<POFulfillmentContactViewModel> Contacts { get; set; }
        public ICollection<POFulfillmentLoadViewModel> Loads { get; set; }
        public ICollection<POFulfillmentOrderViewModel> Orders { get; set; }
        public ICollection<POFulfillmentCargoDetailViewModel> CargoDetails { get; set; }
        public ICollection<POFulfillmentItineraryViewModel> Itineraries { get; set; }
        public ICollection<POFulfillmentAttachmentViewModel> Attachments { get; set; }
        public string PoRemark { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new InputPOFulfillmentValidator().ValidateAndThrow(this);
        }

        protected override void AuditChildren(string user)
        {
            if (Contacts != null)
            {
                foreach (var contact in Contacts)
                {
                    contact.Audit(user);
                }
            }

            if (Itineraries != null)
            {
                foreach (var iItinerary in Itineraries)
                {
                    iItinerary.Audit(user);
                }
            }
        }
    }
}