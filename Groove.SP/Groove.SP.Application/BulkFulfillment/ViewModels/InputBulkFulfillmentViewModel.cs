using Groove.SP.Application.Common;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.BulkFulfillment.ViewModels
{
    public class InputBulkFulfillmentViewModel : ViewModelBase<POFulfillmentModel>
    {
        public long Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ModeOfTransportType ModeOfTransport { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public IncotermType Incoterm { get; set; }

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

        // LocationId
        public long? ReceiptPortId { get; set; }

        // LocationId
        public long? DeliveryPortId { get; set; }

        // LocationDescription
        public string ReceiptPort { get; set; }

        // LocationDescription
        public string DeliveryPort { get; set; }

        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? CargoReadyDate { get; set; }

        public long PreferredCarrier { get; set; }

        public string VesselName { get; set; }

        public string VoyageNo { get; set; }

        public bool IsContainDangerousGoods { get; set; }
        public bool IsCIQOrFumigation { get; set; }
        public bool IsBatteryOrChemical { get; set; }
        public bool IsExportLicence { get; set; }

        public bool IsShipperPickup { get; set; }
        public bool IsNotifyPartyAsConsignee { get; set; }

        /// <summary>
        /// Using for bulk booking only
        /// </summary>
        public bool IsAllowMixedCarton { get; set; }


        public string Remarks { get; set; }

        public string Owner { get; set; }

        public bool UpdateOrganizationPreferences { get; set; }

        public bool UpdateOrgContactPreferences { get; set; }

        public POFulfillmentStatus Status { get; set; }

        public POFulfillmentStage Stage { get; set; }

        public FulfillmentType FulfillmentType { get; set; }

        public ICollection<BulkFulfillmentContactViewModel> Contacts { get; set; }
        public ICollection<BulkFulfillmentLoadViewModel> Loads { get; set; }
        public ICollection<BulkFulfillmentOrderViewModel> Orders { get; set; }
        public ICollection<POFulfillmentAttachmentViewModel> Attachments { get; set; }

        public string PoRemark { get; set; }

        protected override void AuditChildren(string userName)
        {
            if (Contacts != null)
            {
                foreach (var contact in Contacts)
                {
                    contact.Audit(userName);
                }
            }
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
